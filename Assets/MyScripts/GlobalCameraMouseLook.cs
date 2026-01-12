using UnityEngine;
using Unity.Netcode;

/// <summary>
/// ESTE SCRIPT VA EN LA CÁMARA GLOBAL DE LA ESCENA (solo 1).
/// Funciona aunque la cámara NO sea owner.
/// Detecta input del cliente local y rota SU player local.
/// </summary>
public class GlobalCameraMouseLook : NetworkBehaviour
{
    [Header("Sensibilidad del mouse")]
    public float mouseSensitivity = 4f;

    [Header("Rotación suave")]
    public float rotationSmoothSpeed = 10f;

    [Header("Idle Look")]
    public bool enableIdleLook = true;
    public float idleLookSpeed = 25f;
    public float idleLookAmplitude = 10f;

    [Header("Input")]
    public bool requireMouseButton = true;

    private float idleTimer = 0f;
    private float currentYaw = 0f;
    private float targetYaw = 0f;

    private bool usingGlobalCam = false;

    // Referencias del player local
    private NetworkObject localPlayerObject;
    private Transform localBodyTransform;
    private Transform localHeadTransform;

    // -------------------------LLAMADO DESDE PlayerCameraController (o directamente desde mi logica)--------------------------------
    public void SetUsingGlobalCamera(bool active)
    {
        usingGlobalCam = active;

        Debug.Log($"[GlobalCameraMouseLook] usingGlobalCam = {active}");

        if (!active)
            idleTimer = 0f;
    }

    // ---------------------------------------------------------
    private void Start()
    {
        Debug.Log("[GlobalCameraMouseLook] START ejecutado.");
    }

    // ---------------------------------------------------------
    private void Update()
    {
        Debug.Log("[GlobalCameraMouseLook] Update() ejecutándose.");

        if (!usingGlobalCam)
        {
            Debug.LogWarning("[GlobalCameraMouseLook] usingGlobalCam = FALSE → se corta Update.");
            return;
        }

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("[GlobalCameraMouseLook] No NetworkManager o no es cliente.");
            return;
        }

        EnsureLocalPlayerReference();
        if (localBodyTransform == null)
        {
            Debug.LogWarning("[GlobalCameraMouseLook] NO se encontró el player local aún.");
            return;
        }

        // --------------SE REQUIERE DEL CLICK IZQUIERDO---------------
        bool mousePressed = Input.GetMouseButton(0);

        if (requireMouseButton && !mousePressed)
        {
            Debug.Log("[GlobalCameraMouseLook] Click no presionado → no rotamos.");
            return;
        }

        // ---------------MOUSE INPUT--------------
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.001f)
        {
            Debug.Log("[GlobalCameraMouseLook] Mouse detectado → rotando yaw.");
            targetYaw += mouseX * mouseSensitivity;
            idleTimer = 0f;
        }
        else if (enableIdleLook)
        {
            idleTimer += Time.deltaTime;
            targetYaw += Mathf.Sin(idleTimer * idleLookSpeed) * (idleLookAmplitude * Time.deltaTime);

            Debug.Log("[GlobalCameraMouseLook] Idle look activado.");
        }

        // -------------ROTACION DEL CUERPO----------------
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * rotationSmoothSpeed);

        Quaternion bodyTarget = Quaternion.Euler(0f, currentYaw, 0f);

        localBodyTransform.rotation = Quaternion.Slerp(
            localBodyTransform.rotation,
            bodyTarget,
            Time.deltaTime * rotationSmoothSpeed
        );

        Debug.Log("[GlobalCameraMouseLook] Rotando cuerpo del player.");

        // --------------Rotacion de cabeza (se puede comentar si uno no lo quiere)---------------
        if (localHeadTransform != null)
        {
            float headPitch = -mouseY * 0.2f;
            float headYaw = mouseX * 0.4f;

            Quaternion headTarget = Quaternion.Euler(headPitch, headYaw, 0f);

            localHeadTransform.localRotation = Quaternion.Slerp(
                localHeadTransform.localRotation,
                headTarget,
                Time.deltaTime * rotationSmoothSpeed
            );

            Debug.Log("[GlobalCameraMouseLook] Rotando cabeza del player.");
        }
        else
        {
            Debug.LogWarning("[GlobalCameraMouseLook] No Head → solo rotamos cuerpo.");
        }
    }

    // ----------------------------BUSCA AL PLAYER LOCAL SIN IMPORTAR CUANDO SPAWNEA-----------------------------
    private void EnsureLocalPlayerReference()
    {
        if (localPlayerObject != null && localPlayerObject.IsSpawned)
            return;

        if (NetworkManager.Singleton == null)
            return;

        ulong myId = NetworkManager.Singleton.LocalClientId;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(myId, out var client))
        {
            Debug.LogWarning("[GlobalCameraMouseLook] No se encontró el ConnectedClient local.");
            return;
        }

        if (client.PlayerObject == null)
        {
            Debug.LogWarning("[GlobalCameraMouseLook] PlayerObject aún NO existe.");
            return;
        }

        localPlayerObject = client.PlayerObject;
        localBodyTransform = localPlayerObject.transform;

        Debug.Log("[GlobalCameraMouseLook] Player local encontrado: " + localPlayerObject.name);

        // Buscar cabeza
        Transform head = localBodyTransform.Find("Head");
        if (head == null) head = localBodyTransform.Find("head");
        if (head == null) head = localBodyTransform.Find("Cabeza");

        if (head != null)
        {
            localHeadTransform = head;
            Debug.Log("[GlobalCameraMouseLook] Head detectada correctamente.");
        }
        else
        {
            Debug.LogWarning("[GlobalCameraMouseLook] No se encontró Head → solo rotamos cuerpo.");
        }
    }
}
