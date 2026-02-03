using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerCameraController : NetworkBehaviour
{
    [Header("Sensibilidad")]
    public float sensitivity = 200f;

    [Header("Referencias")]
    public Camera playerCamera;
    public Camera globalCamera;
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

    private CanvasGroup fadeGroup;

    // networking
    private float sendTimer = 0f;
    private const float sendInterval = 0.05f;

    // remote interpolation
    private Quaternion targetBodyRot;
    private Quaternion smoothBodyRot;
    private Quaternion targetHeadRot;
    private Quaternion smoothHeadRot;

    [Header("Teclado")]
    public float keySensitivity = 60f;

    // INPUT SYSTEM
    private PlayerInput playerInput;
    private InputAction toggleCameraAction;
    // Rotaciones InputSystem
    private InputAction lookUpAction;
    private InputAction lookDownAction;
    private InputAction lookLeftAction;
    private InputAction lookRightAction;


    // ================== NETCODE ==================
    public override void OnNetworkSpawn()
    {
        Debug.Log("<color=yellow>[PlayerCameraController] OnNetworkSpawn</color>");

        if (!IsOwner)
        {
            if (playerCamera != null)
                playerCamera.enabled = false;

            return;
        }

        // Buscar PlayerInput en el prefab padre
        playerInput = GetComponentInParent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("[PlayerCameraController] PlayerInput NO encontrado en el padre");
            return;
        }

        toggleCameraAction = playerInput.actions["ToggleCamara"];

        if (toggleCameraAction == null)
        {
            Debug.LogError("[PlayerCameraController] Action 'ToggleCamara' no existe");
            return;
        }

        toggleCameraAction.performed += OnToggleCamaraInput;

        // inicializar rotaciones
        rotationY = bodyTransform.eulerAngles.y;
        currentHeadYaw = 0f;

        if (globalCamera == null)
            Debug.LogError("[PlayerCameraController] La cámara global NO está asignada");

        // Fade
        GameObject fadeObj = GameObject.Find("FadeScreen");
        if (fadeObj != null)
        {
            fadeGroup = fadeObj.GetComponent<CanvasGroup>();
            if (fadeGroup == null) fadeGroup = fadeObj.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
        }

        if (playerCamera != null) playerCamera.enabled = false;
        if (globalCamera != null) globalCamera.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //
        lookUpAction = playerInput.actions["LookUp"];
        lookDownAction = playerInput.actions["LookDown"];
        lookLeftAction = playerInput.actions["LookLeft"];
        lookRightAction = playerInput.actions["LookRight"];

        lookUpAction.Enable();
        lookDownAction.Enable();
        lookLeftAction.Enable();
        lookRightAction.Enable();
    }

    private void OnDestroy()
    {
        if (toggleCameraAction != null)
            toggleCameraAction.performed -= OnToggleCamaraInput;
    }

    // ================== UPDATE ==================
    private void Update()
    {
        if (!IsOwner)
        {
            ApplyRemoteInterpolation();
            return;
        }

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

    // ================== INPUT CALLBACK ==================
    private void OnToggleCamaraInput(InputAction.CallbackContext ctx)
    {
        Debug.Log("<color=green>[INPUT] ToggleCamara</color>");

        if (!IsOwner)
            return;

        StartCoroutine(ToggleCameraSmooth());
    }

    // ================== NETWORK ==================
    private void HandleNetworkSend()
    {
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            SendRotationToServerServerRpc(rotationX, rotationY, currentHeadYaw);
        }
    }

    // ================== CAMERA TOGGLE ==================
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

    // ================== FPS ROTATION ==================
    private void RotateCamera()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        rotationY += mouseX;
        currentHeadYaw += mouseX;
        currentHeadYaw = Mathf.Clamp(currentHeadYaw, -maxHeadYawOffset, maxHeadYawOffset);

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

    // ================== RPC ==================
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

    // ================== REMOTE ==================
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

    // ================== KEY ROTATION (LEGACY) ==================
    private void RotateCameraByKeys()
    {
        float inputYaw = 0f;
        float inputPitch = 0f;

        if (lookUpAction.ReadValue<float>() > 0f)
            inputPitch -= 1f;

        if (lookDownAction.ReadValue<float>() > 0f)
            inputPitch += 1f;

        if (lookLeftAction.ReadValue<float>() > 0f)
            inputYaw -= 1f;

        if (lookRightAction.ReadValue<float>() > 0f)
            inputYaw += 1f;

        inputYaw *= keySensitivity * Time.deltaTime;
        inputPitch *= keySensitivity * Time.deltaTime;

        rotationX += inputPitch;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        rotationY += inputYaw;
        currentHeadYaw += inputYaw;
        currentHeadYaw = Mathf.Clamp(currentHeadYaw, -maxHeadYawOffset, maxHeadYawOffset);

        Quaternion targetBody = Quaternion.Euler(0f, rotationY - currentHeadYaw, 0f);
        bodyTransform.rotation = Quaternion.Slerp(
            bodyTransform.rotation,
            targetBody,
            1f - Mathf.Exp(-bodyFollowSpeed * Time.deltaTime)
        );

        headTransform.localRotation = Quaternion.Euler(rotationX, currentHeadYaw, 0f);
    }


    // ================== LEGACY (NO SE BORRA) ==================
    public void OnToggleCamara()
    {
        Debug.Log("OnToggleCamara (legacy) llamado");
    }
}
