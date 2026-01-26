using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerCameraController : NetworkBehaviour
{
    [Header("Sensibilidad")]
    public float sensitivity = 200f;

    [Header("Referencias")]
    public Camera playerCamera;
    public Camera globalCamera;  // esta camara NO es Camera.main [es la que pasas desde el editor]
    [Header("Transforms")]
    public Transform headTransform;
    public Transform bodyTransform;

    [Header("Rotación")]
    public float bodyTurnSpeed = 8f;
    public float maxHeadYawOffset = 35f;
    public float bodyFollowSpeed = 4f;
    public float headReturnSpeed = 6f;
    public float plusKeyRotation = 2f;

    [Header("Fade")]
    public float fadeDuration = 0.4f;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private float currentHeadYaw = 0f;
    private bool isFPS = false;

    [SerializeField] private CanvasGroup fadeGroup;

    // net-working
    private float sendTimer = 0f;
    private const float sendInterval = 0.05f; // 20 Hz

    // remote interpolation
    private Quaternion targetBodyRot;
    private Quaternion smoothBodyRot;

    private Quaternion targetHeadRot;
    private Quaternion smoothHeadRot;

    [Header("Teclado")]
    public float keySensitivity = 60f;


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

        // inicializar rotaciones
        rotationY = bodyTransform.eulerAngles.y;
        currentHeadYaw = 0f;

        if (globalCamera == null)
            Debug.LogError("[PlayerCameraController] La cámara global NO está asignada en el inspector!");

        // FadeScreen
        /* // Dejo de funcionar cuando se separo por escenas
        GameObject fadeObj = GameObject.Find("FadeScreen");
        if (fadeObj != null)
        {
            fadeGroup = fadeObj.GetComponent<CanvasGroup>();
            if (fadeGroup == null) fadeGroup = fadeObj.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
        }
        */
        StartCoroutine(FindFadeCanvas());

        // que mire a la camara global
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

        // ------------TOGGLE CAMARA, ENTRE LA GLOBAL Y LA FPS-------------
        if (Input.GetKeyDown(KeyCode.V))
            StartCoroutine(ToggleCameraSmooth());

        // -------------CONTROL FPS------------
        if (isFPS)
        {
            RotateCamera();
            HandleNetworkSend();
        }
        else
        {
            RotateCameraByKeys();
            HandleNetworkSend();
        }
    }


    // --------------NETWORK RATE LIMIT---------------
    private void HandleNetworkSend()
    {
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            SendRotationToServerServerRpc(rotationX, rotationY, currentHeadYaw);
        }
    }


    // ------------CAMARA TOGGLE (FADE)-----------------
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


    // -------------CAMARA ROTATION (primera persona)----------------
    private void RotateCamera()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity * Time.deltaTime;

        // PITCH
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        // YAW
        rotationY += mouseX;
        currentHeadYaw += mouseX;
        currentHeadYaw = Mathf.Clamp(currentHeadYaw, -maxHeadYawOffset, maxHeadYawOffset);

        // ROTACION DEL CUERPO      espero que ahora ande el codigo de re carajo, ya 3 horas, quiero COMEEEEEER AAAAAAA
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


    // ------------RPC SYNC-----------------
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


    // --------------REMOVE INTERPOLATION---------------
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

    private void RotateCameraByKeys()
    {
        float inputYaw = 0f;
        float inputPitch = 0f;

        if (Input.GetKey(KeyCode.W)) inputPitch = -1f;
        if (Input.GetKey(KeyCode.S)) inputPitch = 1f;
        if (Input.GetKey(KeyCode.A)) inputYaw = -1f;
        if (Input.GetKey(KeyCode.D)) inputYaw = 1f;

        inputYaw *= keySensitivity * Time.deltaTime;
        inputPitch *= keySensitivity * Time.deltaTime;

        rotationX += inputPitch;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        rotationY += inputYaw;
        currentHeadYaw += inputYaw;
        currentHeadYaw = Mathf.Clamp(currentHeadYaw, -maxHeadYawOffset, maxHeadYawOffset);

        // MISMA LOGICA QUE FPS
        if (Mathf.Abs(currentHeadYaw) >= maxHeadYawOffset - 0.1f)
        {
            Quaternion targetBody = Quaternion.Euler(0f, rotationY, 0f);
            bodyTransform.rotation = Quaternion.Slerp(
                bodyTransform.rotation,
                targetBody,
                1f - Mathf.Exp(-bodyFollowSpeed * Time.deltaTime)
            );

            currentHeadYaw = Mathf.Lerp(currentHeadYaw, 0f, Time.deltaTime * headReturnSpeed);
        }
        else
        {
            Quaternion targetBody = Quaternion.Euler(0f, rotationY - currentHeadYaw, 0f);
            bodyTransform.rotation = Quaternion.Slerp(
                bodyTransform.rotation,
                targetBody,
                1f - Mathf.Exp(-(bodyFollowSpeed * 0.4f) * Time.deltaTime)
            );
        }

        headTransform.localRotation = Quaternion.Euler(rotationX, currentHeadYaw, 0f);

        // -obsoleto-
        // -obsoleto-

        // --- ROTACIÓN DEL CUERPO (Q - E) --- ESTO SE PUEDE COMENTAR PORQUE GENERA BUGS
        if (Input.GetKey(KeyCode.Q))
        {
            rotationY -= bodyTurnSpeed * Time.deltaTime * plusKeyRotation;
            bodyTransform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        }

        if (Input.GetKey(KeyCode.E))
        {
            rotationY += bodyTurnSpeed * Time.deltaTime * plusKeyRotation;
            bodyTransform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        }

    }

    // --------------BUSCAR CANVAS---------------
    private IEnumerator FindFadeCanvas()
    {
        while (fadeGroup == null)
        {
            GameObject fadeObj = GameObject.FindGameObjectWithTag("FadeScreen");

            if (fadeObj != null)
            {
                fadeGroup = fadeObj.GetComponent<CanvasGroup>();
                if (fadeGroup == null)
                    fadeGroup = fadeObj.AddComponent<CanvasGroup>();

                fadeGroup.alpha = 0f;
                yield break;
            }

            yield return null; // espera al próximo frame
        }
    }


}
