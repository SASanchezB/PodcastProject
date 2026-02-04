using UnityEngine;
using UnityEngine.UI;

public class RawImageToQuad : MonoBehaviour
{
    [Header("Source")]
    public RawImage sourceRawImage;

    [Header("Target")]
    public Renderer targetRenderer;

    void Update()
    {
        if (sourceRawImage == null || targetRenderer == null)
            return;

        if (sourceRawImage.texture != null)
        {
            targetRenderer.material.mainTexture = sourceRawImage.texture;
        }
    }
}
