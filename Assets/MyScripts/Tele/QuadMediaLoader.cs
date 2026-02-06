using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.Networking;
using UnityEngine.Video;
using System.Collections;

public class QuadMediaLoader : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer quadRenderer;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Host URL Input")]
    [SerializeField] private string mediaUrl; // ← el host pega la URL acá

    NetworkVariable<FixedString512Bytes> syncedMediaUrl =
        new NetworkVariable<FixedString512Bytes>(
            "",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    void Awake()
    {
        quadRenderer.material.shader = Shader.Find("Unlit/Texture");

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = quadRenderer;
        videoPlayer.targetMaterialProperty = "_MainTex";
    }

    public override void OnNetworkSpawn()
    {
        syncedMediaUrl.OnValueChanged += OnMediaUrlChanged;
    }

    void Update()
    {
        if (!IsHost)
            return;

        // Presioná TAB para sincronizar la URL
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!string.IsNullOrEmpty(mediaUrl))
                syncedMediaUrl.Value = mediaUrl;
        }
    }

    void OnMediaUrlChanged(FixedString512Bytes oldValue, FixedString512Bytes newValue)
    {
        string url = newValue.ToString();

        if (string.IsNullOrEmpty(url))
            return;

        StopAllCoroutines();
        videoPlayer.Stop();

        if (IsVideo(url))
            PlayVideo(url);
        else
            StartCoroutine(DownloadImage(url));
    }

    bool IsVideo(string url)
    {
        url = url.ToLower();
        return url.EndsWith(".mp4") ||
               url.EndsWith(".webm") ||
               url.EndsWith(".mov") ||
               url.EndsWith(".m3u8");
    }

    void PlayVideo(string url)
    {
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += _ => videoPlayer.Play();
    }

    IEnumerator DownloadImage(string url)
    {
        using UnityWebRequest req =
            UnityWebRequestTexture.GetTexture(url);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(req.error);
            yield break;
        }

        Texture2D tex =
            DownloadHandlerTexture.GetContent(req);

        quadRenderer.material.mainTexture = tex;
    }
}
