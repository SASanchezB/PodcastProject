using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerCameraController : NetworkBehaviour
{
    [Header("Sensibilidad")]
    public float sensitivity = 200f;

    [Header("Referencias")]
    public Camera playerCamera;
    [SerializeField] private Camera globalCamera;

    [Header("Transforms")]
    [Tooltip("Transform que representa la cabeza (la cámara debe ser hija de este)")]
    public Transform headTransform;
    [Tooltip("Transform que representa el cuerpo (raíz del modelo)")]
    public Transform bodyTransform;

    [Header("Suavizado del cuerpo")]
    [Tooltip("Velocidad con la que el cuerpo gira para seguir la cabeza (mayor = más rápido)")]
    public float bodyTurnSpeed = 8f;

    private float rotationX = 0f; // pitch (arriba/abajo)
    private float rotationY = 0f; // yaw (izq/der)
    private bool isFPS = false;

    // Fade
    private CanvasGroup fadeGroup;
    public float fadeDuration = 0.4f;

    // Rotación de la cabeza
    private float currentHeadYaw = 0f; // Yaw local de la cabeza (solo visual)
    public float maxHeadYawOffset = 35f; // Grados que puede girar la cabeza sola
    public float bodyFollowSpeed = 4f;   // Velocidad con la que el cuerpo sigue
    public float headReturnSpeed = 6f;   // Velocidad de "volver al centro" de la cabeza

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
            fadeGroup = fadeObj.GetComponent<CanvasGroup>();
            if (fadeGroup == null) fadeGroup = fadeObj.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
        }
        else
        {
            Debug.LogWarning("No existe FadeScreen en la escena.");
        }

        playerCamera.enabled = false;
        if (globalCamera != null) globalCamera.enabled = true;
    }

    private void Update()
    {
        if (!IsOwner)
        {
            ApplyNetworkRotation(); // Para clientes que no son dueños
            return;
        }

        if (Input.GetKeyDown(KeyCode.V))
            StartCoroutine(ToggleCameraSmooth());

        if (isFPS)
        {
            RotateCamera();

            // Enviar rotación al servidor
            SendRotationToServerServerRpc(rotationX, rotationY, currentHeadYaw);
        }
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

        if (playerCamera != null) playerCamera.enabled = isFPS;
        if (globalCamera != null) globalCamera.enabled = !isFPS;

        Cursor.lockState = isFPS ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isFPS;
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // ---- PITCH (arriba/abajo) ----
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        // ---- YAW (izq/der) ----
        rotationY += mouseX;
        currentHeadYaw += mouseX;
        currentHeadYaw = Mathf.Clamp(currentHeadYaw, -maxHeadYawOffset, maxHeadYawOffset);

        float headToBodyDiff = Mathf.DeltaAngle(bodyTransform.eulerAngles.y, rotationY);

        if (Mathf.Abs(currentHeadYaw) >= maxHeadYawOffset - 0.1f)
        {
            Quaternion target = Quaternion.Euler(0f, rotationY, 0f);
            bodyTransform.rotation = Quaternion.Slerp(
                bodyTransform.rotation,
                target,
                1f - Mathf.Exp(-bodyFollowSpeed * Time.deltaTime)
            );

            currentHeadYaw = Mathf.Lerp(currentHeadYaw, 0f, Time.deltaTime * headReturnSpeed);
        }
        else
        {
            Quaternion target = Quaternion.Euler(0f, rotationY - currentHeadYaw, 0f);
            bodyTransform.rotation = Quaternion.Slerp(
                bodyTransform.rotation,
                target,
                1f - Mathf.Exp(-(bodyFollowSpeed * 0.4f) * Time.deltaTime)
            );
        }

        headTransform.localRotation = Quaternion.Euler(rotationX, currentHeadYaw, 0f);
    }

    // ------------------ NETWORK ------------------

    [ServerRpc]
    private void SendRotationToServerServerRpc(float rotX, float rotY, float headYaw)
    {
        // Propagar a todos los clientes desde el servidor
        UpdateRotationClientRpc(rotX, rotY, headYaw);
    }

    [ClientRpc]
    private void UpdateRotationClientRpc(float rotX, float rotY, float headYaw)
    {
        if (IsOwner) return; // no aplicamos al dueño

        rotationX = rotX;
        rotationY = rotY;
        currentHeadYaw = headYaw;

        // Aplicar rotación al cuerpo
        Quaternion targetBody = Quaternion.Euler(0f, rotationY, 0f);
        bodyTransform.rotation = Quaternion.Slerp(
            bodyTransform.rotation,
            targetBody,
            1f - Mathf.Exp(-bodyFollowSpeed * Time.deltaTime)
        );

        // Aplicar rotación a la cabeza
        headTransform.localRotation = Quaternion.Euler(rotationX, currentHeadYaw, 0f);
    }

    private void ApplyNetworkRotation()
    {
        // Para clientes no dueños
        // Solo por si quieres un extra de suavizado si se quiere
        // (Aquí llamamos a UpdateRotationClientRpc indirectamente)
    }
}
