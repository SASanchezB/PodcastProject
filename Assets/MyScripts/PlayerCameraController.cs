using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerCameraController : NetworkBehaviour
{
    [Header("Sensibilidad")]
    public float sensitivity = 200f;

    [Header("Referencias")]
    public Camera playerCamera;
    public Camera globalCamera;  // Esta cámara NO es Camera.main, la arrastrás vos desde la escena

    [Header("Transforms")]
    public Transform headTransform;
    public Transform bodyTransform;

    [Header("Rotación")]
    public float bodyTurnSpeed = 8f;
    public float maxHeadYawOffset = 35f;
    public float bodyFollowSpeed = 4f;
    public float headReturnSpeed = 6f;

    [Header("Fade")]
    public float fadeDuration = 0.4f;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private float currentHeadYaw = 0f;
    private bool isFPS = false;

    private CanvasGroup fadeGroup;

    // Networking
    private float sendTimer = 0f;
    private const float sendInterval = 0.05f; // 20 Hz

    // Remote interpolation
    private Quaternion targetBodyRot;
    private Quaternion smoothBodyRot;

    private Quaternion targetHeadRot;
    private Quaternion smoothHeadRot;

    private void Start()
    {
        Debug.Log("<color=yellow>[PlayerCameraController] START ejecutado</color>");

        if (!IsOwner)
        {
            if (playerCamera != null)
                playerCamera.enabled = false;

            enabled = true;
            return;
        }

        // Inicializar rotaciones
        rotationY = bodyTransform.eulerAngles.y;
        currentHeadYaw = 0f;

        if (globalCamera == null)
            Debug.LogError("[PlayerCameraController] La cámara global NO está asignada en el inspector!");

        // Fade screen
        GameObject fadeObj = GameObject.Find("FadeScreen");
        if (fadeObj != null)
        {
            fadeGroup = fadeObj.GetComponent<CanvasGroup>();
            if (fadeGroup == null) fadeGroup = fadeObj.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
        }

        // Arrancamos mirando con cámara global
        if (playerCamera != null) playerCamera.enabled = false;
        if (globalCamera != null) globalCamera.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (!IsOwner)
        {
            ApplyRemoteInterpolation();
            return;
        }

        // -------------------------
        // TOGGLE CÁMARA GLOBAL / FPS
        // -------------------------
        if (Input.GetKeyDown(KeyCode.V))
            StartCoroutine(ToggleCameraSmooth());

        // -------------------------
        // CONTROL FPS
        // -------------------------
        if (isFPS)
        {
            RotateCamera();
            HandleNetworkSend();
        }
    }


    // -----------------------------
    // NETWORK RATE LIMITING
    // -----------------------------
    private void HandleNetworkSend()
    {
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            SendRotationToServerServerRpc(rotationX, rotationY, currentHeadYaw);
        }
    }


    // -----------------------------
    // FADE - CAMERA TOGGLE
    // -----------------------------
    private IEnumerator ToggleCameraSmooth()
    {
        if (fadeGroup == null)
        {
            ToggleCamera();
            yield break;
        }

        yield return Fade(1f);
        ToggleCamera();
        yield return Fade(0f);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float start = fadeGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            fadeGroup.alpha = Mathf.Lerp(start, targetAlpha, t / fadeDuration);
            t += Time.deltaTime;
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


    // -----------------------------
    // FPS CAMERA ROTATION
    // -----------------------------
    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // PITCH
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        // YAW
        rotationY += mouseX;
        currentHeadYaw += mouseX;
        currentHeadYaw = Mathf.Clamp(currentHeadYaw, -maxHeadYawOffset, maxHeadYawOffset);

        // ROTACIÓN DEL CUERPO
        if (Mathf.Abs(currentHeadYaw) >= maxHeadYawOffset - 0.1f)
        {
            Quaternion targetBody = Quaternion.Euler(0f, rotationY, 0f);
            bodyTransform.rotation = Quaternion.Slerp(
                bodyTransform.rotation, targetBody,
                1f - Mathf.Exp(-bodyFollowSpeed * Time.deltaTime)
            );

            currentHeadYaw = Mathf.Lerp(currentHeadYaw, 0f, Time.deltaTime * headReturnSpeed);
        }
        else
        {
            Quaternion targetBody = Quaternion.Euler(0f, rotationY - currentHeadYaw, 0f);
            bodyTransform.rotation = Quaternion.Slerp(
                bodyTransform.rotation, targetBody,
                1f - Mathf.Exp(-(bodyFollowSpeed * 0.4f) * Time.deltaTime)
            );
        }

        headTransform.localRotation = Quaternion.Euler(rotationX, currentHeadYaw, 0f);
    }


    // -----------------------------
    // RPC SYNC
    // -----------------------------
    [ServerRpc]
    private void SendRotationToServerServerRpc(float rotX, float rotY, float headYaw)
    {
        UpdateRotationClientRpc(rotX, rotY, headYaw);
    }

    [ClientRpc]
    private void UpdateRotationClientRpc(float rotX, float rotY, float headYaw)
    {
        if (IsOwner) return;

        rotationX = rotX;
        rotationY = rotY;
        currentHeadYaw = headYaw;

        targetBodyRot = Quaternion.Euler(0f, rotationY, 0f);
        Quaternion localHeadRot = Quaternion.Euler(rotationX, currentHeadYaw, 0f);
        targetHeadRot = targetBodyRot * localHeadRot;
    }


    // -----------------------------
    // REMOTE INTERPOLATION
    // -----------------------------
    private void ApplyRemoteInterpolation()
    {
        smoothBodyRot = Quaternion.Slerp(
            smoothBodyRot, targetBodyRot, Time.deltaTime * 10f
        );
        bodyTransform.rotation = smoothBodyRot;

        smoothHeadRot = Quaternion.Slerp(
            smoothHeadRot, targetHeadRot, Time.deltaTime * 12f
        );
        headTransform.rotation = smoothHeadRot;
    }
}
