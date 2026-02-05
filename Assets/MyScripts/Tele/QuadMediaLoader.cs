using UnityEngine;
using Unity.Netcode;
using SFB;
using UnityEngine.Video;
using System.IO;
using System.Collections;
using System.Collections.Generic; // Agrega esto para Dictionary

public class QuadMediaLoader : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer quadRenderer;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;

    private string tempFilePath; // Para almacenar la ruta del archivo temporal actual
    private const int CHUNK_SIZE = 512; // Reducido a 512 bytes para evitar overflow (ajusta a 256 si sigue fallando)

    // Para chunking: estado temporal por cliente
    private Dictionary<ulong, List<byte[]>> clientChunks = new Dictionary<ulong, List<byte[]>>();
    private Dictionary<ulong, string> clientExtensions = new Dictionary<ulong, string>();
    private Dictionary<ulong, int> clientTotalChunks = new Dictionary<ulong, int>();

    void Awake()
    {
        if (quadRenderer != null)
        {
            quadRenderer.material.shader = Shader.Find("Unlit/Texture");
        }

        if (videoPlayer != null)
        {
            videoPlayer.enabled = false;
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true; // Cambia si no quieres loop
        }
    }

    void Update()
    {
        if (!IsHost)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OpenFileExplorer();
        }
    }

    void OpenFileExplorer()
    {
        var extensions = new[]
        {
            new ExtensionFilter("Media Files", "png", "jpg", "jpeg", "mp4", "mp3"),
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel(
            "Seleccionar archivo",
            "",
            extensions,
            false
        );

        if (paths != null && paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            LoadFile(paths[0]);
        }
    }

    void LoadFile(string path)
    {
        string extension = Path.GetExtension(path).ToLower();

        // Limpiar archivo temporal anterior si existe
        if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }

        // Leer bytes del archivo
        byte[] fileBytes = File.ReadAllBytes(path);

        // Reset seguro
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.enabled = false;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        // Cargar localmente primero (solo en host) usando path temporal
        SaveAndLoadFromTemp(fileBytes, extension);

        // Sincronizar con clientes: dividir en chunks y enviar
        int totalChunks = Mathf.CeilToInt((float)fileBytes.Length / CHUNK_SIZE);
        Debug.Log("Enviando archivo con " + totalChunks + " chunks de " + CHUNK_SIZE + " bytes cada uno.");
        StartMediaSyncServerRpc(extension, totalChunks);

        for (int i = 0; i < totalChunks; i++)
        {
            int start = i * CHUNK_SIZE;
            int length = Mathf.Min(CHUNK_SIZE, fileBytes.Length - start);
            byte[] chunk = new byte[length];
            System.Array.Copy(fileBytes, start, chunk, 0, length);
            SendChunkServerRpc(chunk, i, totalChunks);
        }
    }

    // Nuevo: Inicia la sincronización y limpia estado anterior
    [ServerRpc]
    private void StartMediaSyncServerRpc(string extension, int totalChunks)
    {
        StartMediaSyncClientRpc(extension, totalChunks);
    }

    [ClientRpc]
    private void StartMediaSyncClientRpc(string extension, int totalChunks)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        clientExtensions[clientId] = extension;
        clientTotalChunks[clientId] = totalChunks;
        clientChunks[clientId] = new List<byte[]>();
        Debug.Log("Cliente " + clientId + " preparado para recibir " + totalChunks + " chunks.");
    }

    // Nuevo: Envía cada chunk
    [ServerRpc]
    private void SendChunkServerRpc(byte[] chunk, int chunkIndex, int totalChunks)
    {
        SendChunkClientRpc(chunk, chunkIndex, totalChunks);
    }

    [ClientRpc]
    private void SendChunkClientRpc(byte[] chunk, int chunkIndex, int totalChunks)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        if (!clientChunks.ContainsKey(clientId))
        {
            clientChunks[clientId] = new List<byte[]>();
        }
        clientChunks[clientId].Add(chunk);
        Debug.Log("Cliente " + clientId + " recibió chunk " + chunkIndex + " de " + totalChunks);

        // Si todos los chunks han llegado, reconstruir y cargar
        if (clientChunks[clientId].Count == totalChunks)
        {
            byte[] fullBytes = CombineChunks(clientChunks[clientId]);
            string extension = clientExtensions[clientId];
            SaveAndLoadFromTemp(fullBytes, extension);
            Debug.Log("Cliente " + clientId + " reconstruyó y cargó el archivo.");

            // Limpiar estado
            clientChunks.Remove(clientId);
            clientExtensions.Remove(clientId);
            clientTotalChunks.Remove(clientId);
        }
    }

    // Función auxiliar: Combina chunks en un solo byte[]
    private byte[] CombineChunks(List<byte[]> chunks)
    {
        int totalSize = 0;
        foreach (var chunk in chunks)
        {
            totalSize += chunk.Length;
        }
        byte[] result = new byte[totalSize];
        int offset = 0;
        foreach (var chunk in chunks)
        {
            System.Array.Copy(chunk, 0, result, offset, chunk.Length);
            offset += chunk.Length;
        }
        return result;
    }

    // Función auxiliar: Guarda en temporal y carga (usada por host y clientes)
    private void SaveAndLoadFromTemp(byte[] fileBytes, string extension)
    {
        // Guardar en temporal
        string tempDir = Path.GetTempPath();
        string tempFileName = "temp_media_" + System.Guid.NewGuid().ToString() + extension;
        tempFilePath = Path.Combine(tempDir, tempFileName);
        File.WriteAllBytes(tempFilePath, fileBytes);

        // Cargar según tipo
        if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
        {
            LoadImageFromPath(tempFilePath);
        }
        else if (extension == ".mp4")
        {
            LoadVideoFromPath(tempFilePath);
        }
        else if (extension == ".mp3")
        {
            LoadAudioFromPath(tempFilePath);
        }
    }

    void LoadImageFromPath(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(data);
        quadRenderer.material.mainTexture = tex;
    }

    void LoadVideoFromPath(string path)
    {
        if (videoPlayer == null)
            return;

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = "file://" + path; // URL local para archivo temporal
        videoPlayer.enabled = true;
        videoPlayer.Play();
    }

    void LoadAudioFromPath(string path)
    {
        StartCoroutine(LoadAudioCoroutine("file://" + path));
    }

    IEnumerator LoadAudioCoroutine(string url)
    {
        using WWW www = new WWW(url);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(www.error);
            yield break;
        }

        audioSource.clip = www.GetAudioClip();
        audioSource.Play();
    }

    // Limpieza opcional: llamar esto al cambiar de escena o salir
    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }
    }
}