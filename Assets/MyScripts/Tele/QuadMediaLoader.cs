using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.Networking;
using UnityEngine.Video;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro; 

public class QuadMediaLoader : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer quadRenderer;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;

    [Header("UI References")]
    [SerializeField] private GameObject panel;           // Panel del link
    [SerializeField] private TMP_InputField urlInputField;

    [Header("Host URL Input")]
    [SerializeField] private string mediaUrl; // Obsoleto (Solo para testing, ahora los links van en el urlinputfield

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
            audioSource.playOnAwake = false;

        if (panel != null)
            panel.SetActive(false); // POR LAS DUDAS, oculto el panel, en el testing habia veces que se activa... o lo dejaba prendido
    }

    public override void OnNetworkSpawn()
    {
        syncedMediaUrl.OnValueChanged += OnMediaUrlChanged;
    }

    void Update()
    {
        if (!IsHost)
            return;

        // SHIFT -> ABRE EL PANEL
        if (Input.GetKeyDown(KeyCode.LeftShift) && panel != null)   
        {
            panel.SetActive(!panel.activeSelf);
        }

        // TAB -> Manda o lee el enlace para el todo
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ApplyInputFieldUrl();
        }
    }

    public void ApplyInputFieldUrl()
    {
        if (urlInputField != null)
            mediaUrl = urlInputField.text;

        if (!string.IsNullOrEmpty(mediaUrl))
            syncedMediaUrl.Value = ConvertToDirectLink(mediaUrl);
    }

    void OnMediaUrlChanged(FixedString512Bytes oldValue, FixedString512Bytes newValue)
    {
        string url = newValue.ToString();

        if (string.IsNullOrEmpty(url))
            return;

        StopAllCoroutines();
        if (videoPlayer != null) videoPlayer.Stop();
        if (audioSource != null) audioSource.Stop();

        StartCoroutine(DetectAndLoad(url));
    }

    #region DETECCION AUTOMATICA
    IEnumerator DetectAndLoad(string url)
    {
        using UnityWebRequest req = UnityWebRequest.Head(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            //LINK NO VALIDO
            Debug.LogError("Error detectando tipo de contenido: " + req.error);
            yield break;
        }

        string contentType = req.GetResponseHeader("Content-Type") ?? "";
        contentType = contentType.ToLower();

        if (contentType.StartsWith("image/"))
            StartCoroutine(DownloadImage(url));
        else if (contentType.StartsWith("video/"))
            PlayVideo(url);
        else if (contentType.StartsWith("audio/"))
            StartCoroutine(PlayAudio(url));
        else
            //TIPO DE LINK NO SE PUEDE LEER
            Debug.LogWarning("Tipo de contenido no soportado: " + contentType);
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
    string ConvertToDirectLink(string url)
    {
        url = url.Trim();

        // Google Drive
        //PUEDE: - Ler imagenes y videos (SIEMPRE Y CUANDO NO SEAN MUY PESADOS O PIDAN UN SCANNER DE VIRUS
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
        //No se llego a testear muy bien, con enlaces con videos de mas de una hora no funciono
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
