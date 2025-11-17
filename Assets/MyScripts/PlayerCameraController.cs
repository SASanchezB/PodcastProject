using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerCameraController : NetworkBehaviour
{
    public float sensitivity = 200f;

    public Camera playerCamera;
    [SerializeField] private Camera globalCamera;

    private float rotationX = 0f;
    private bool isFPS = false;

    // Fade
    private CanvasGroup fadeGroup;
    public float fadeDuration = 0.4f;

    private void Start()
    {
        if (!IsOwner)
        {
            if (playerCamera != null)
                playerCamera.enabled = false;

            enabled = false;
            return;
        }

        // Buscar la cámara global
        globalCamera = Camera.main;

        if (globalCamera == null)
            Debug.LogError("No se encontró ninguna cámara global con tag MainCamera.");

        // Buscar fade screen
        GameObject fadeObj = GameObject.Find("FadeScreen");

        if (fadeObj != null)
        {
            fadeGroup = fadeObj.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
        }
        else
        {
            Debug.LogWarning("No existe FadeScreen en la escena.");
        }

        playerCamera.enabled = false;
        if (globalCamera != null) globalCamera.enabled = true;

        /*
        Debug.Log("Buscando FadeScreen...");
        Debug.Log("Encontrado: " + (fadeObj != null));
        */

    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.V))
            StartCoroutine(ToggleCameraSmooth());

        if (isFPS)
            RotateCamera();
    }

    private IEnumerator ToggleCameraSmooth()
    {
        if (fadeGroup == null)
        {
            ToggleCamera();
            yield break;
        }

        // FADE OUT
        yield return Fade(1f);

        // Cambiar cámaras
        ToggleCamera();

        // FADE IN
        yield return Fade(0f);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float start = fadeGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            fadeGroup.alpha = Mathf.Lerp(start, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
    }

    private void ToggleCamera()
    {
        isFPS = !isFPS;

        playerCamera.enabled = isFPS;

        if (globalCamera != null)
            globalCamera.enabled = !isFPS;

        if (isFPS)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        if (transform.parent != null)
            transform.parent.Rotate(Vector3.up * mouseX);
    }
}
