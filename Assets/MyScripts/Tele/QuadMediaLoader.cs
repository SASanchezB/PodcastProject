using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.Networking;
using UnityEngine.Video;
using System.Collections;
using System.Text.RegularExpressions;

public class QuadMediaLoader : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer quadRenderer;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;

    [Header("Host URL Input")]
    [SerializeField] private string mediaUrl; // El host pega la URL acá

    NetworkVariable<FixedString512Bytes> syncedMediaUrl =
        new NetworkVariable<FixedString512Bytes>(
            "",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    void Awake()
    {
        if (quadRenderer != null)
            quadRenderer.material.shader = Shader.Find("Unlit/Texture");

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            videoPlayer.targetMaterialRenderer = quadRenderer;
            videoPlayer.targetMaterialProperty = "_MainTex";
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        syncedMediaUrl.OnValueChanged += OnMediaUrlChanged;
    }

    void Update()
    {
        if (!IsHost)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!string.IsNullOrEmpty(mediaUrl))
                syncedMediaUrl.Value = ConvertToDirectLink(mediaUrl);
        }
    }

    void OnMediaUrlChanged(FixedString512Bytes oldValue, FixedString512Bytes newValue)
    {
        string url = newValue.ToString();

        if (string.IsNullOrEmpty(url))
            return;

        StopAllCoroutines();
        if (videoPlayer != null) videoPlayer.Stop();
        if (audioSource != null) audioSource.Stop();

        // 🔹 Detección automática por Content-Type
        StartCoroutine(DetectAndLoad(url));
    }

    #region DETECCION AUTOMATICA
    IEnumerator DetectAndLoad(string url)
    {
        using UnityWebRequest req = UnityWebRequest.Head(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error detectando tipo de contenido: " + req.error);
            yield break;
        }

        string contentType = req.GetResponseHeader("Content-Type") ?? "";
        contentType = contentType.ToLower();

        if (contentType.StartsWith("image/"))
        {
            StartCoroutine(DownloadImage(url));
        }
        else if (contentType.StartsWith("video/"))
        {
            PlayVideo(url);
        }
        else if (contentType.StartsWith("audio/"))
        {
            StartCoroutine(PlayAudio(url));
        }
        else
        {
            Debug.LogWarning("Tipo de contenido no soportado: " + contentType);
        }
    }
    #endregion

    #region IMAGENES
    IEnumerator DownloadImage(string url)
    {
        using UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(req.error);
            yield break;
        }

        Texture2D tex = DownloadHandlerTexture.GetContent(req);
        quadRenderer.material.mainTexture = tex;
    }
    #endregion

    #region VIDEO
    void PlayVideo(string url)
    {
        if (videoPlayer == null) return;

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += _ => videoPlayer.Play();
    }
    #endregion

    #region AUDIO
    IEnumerator PlayAudio(string url)
    {
        using UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(req.error);
            yield break;
        }

        audioSource.clip = DownloadHandlerAudioClip.GetContent(req);
        audioSource.Play();
    }
    #endregion

    #region GOOGLE DRIVE / DROPBOX
    /// <summary>
    /// Convierte enlaces de Google Drive o Dropbox a links directos de descarga
    /// </summary>
    string ConvertToDirectLink(string url)
    {
        url = url.Trim();

        // Google Drive
        if (url.Contains("drive.google.com"))
        {
            Regex rgx = new Regex(@"\/d\/([a-zA-Z0-9_-]+)");
            Match m = rgx.Match(url);
            if (m.Success)
            {
                string id = m.Groups[1].Value;
                return $"https://drive.google.com/uc?export=download&id={id}";
            }
        }

        // Dropbox
        if (url.Contains("dropbox.com"))
        {
            if (url.Contains("?dl=0"))
                url = url.Replace("?dl=0", "?dl=1");
            else if (!url.Contains("?dl=1"))
                url += "?dl=1";
            return url;
        }

        return url;
    }
    #endregion
}
