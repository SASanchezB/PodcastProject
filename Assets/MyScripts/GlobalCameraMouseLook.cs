using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Colocar este script en la CÁMARA GLOBAL (solo 1 en la escena).
/// Funciona incluso si la cámara NO es owner.
/// Detecta input del cliente local y rota SU PLAYER.
/// </summary>
public class GlobalCameraMouseLook : NetworkBehaviour
{
    [Header("Sensibilidad del mouse")]
    public float mouseSensitivity = 4f;

    [Header("Rotación suave")]
    public float rotationSmoothSpeed = 10f;

    [Header("Idle Look (cuando no se mueve el mouse)")]
    public bool enableIdleLook = true;
    public float idleLookSpeed = 25f;
    public float idleLookAmplitude = 10f;

    [Header("Input")]
    public bool requireMouseButton = true;   // Nuevo → requiere click izquierdo

    private float idleTimer = 0f;
    private float currentYaw;
    private float targetYaw;
    private bool usingGlobalCam = false;

    // Referencias del player local
    private NetworkObject localPlayerObject;
    private Transform localBodyTransform;
    private Transform localHeadTransform;

    public void SetUsingGlobalCamera(bool active)
    {
        usingGlobalCam = active;

        Debug.Log($"[GlobalCameraMouseLook] Using global camera = {active}");

        if (!active)
            idleTimer = 0f;
    }

    private void Start()
    {
        currentYaw = 0f;
        targetYaw = 0f;

        Debug.Log("[GlobalCameraMouseLook] Script iniciado correctamente.");
    }

    private void Update()
    {
        if (!usingGlobalCam)
        {
            //Debug.Log("[GlobalCameraMouseLook] No usando cámara global.");
            return;
        }

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("[GlobalCameraMouseLook] No hay NetworkManager o no es cliente.");
            return;
        }

        EnsureLocalPlayerReference();
        if (localBodyTransform == null)
        {
            Debug.LogWarning("[GlobalCameraMouseLook] No se encontró el Body del jugador local.");
            return;
        }

        //----------------------------
        // 🔹 MOVIMIENTO SOLO SI CLICK IZQUIERDO
        //----------------------------
        bool mousePressed = Input.GetMouseButton(0);

        if (requireMouseButton && !mousePressed)
        {
            //Debug.Log("[GlobalCameraMouseLook] Click no presionado → no rotamos.");
            return;
        }

        //----------------------------
        // 🔹 INPUT DEL MOUSE
        //----------------------------
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.001f)
        {
            Debug.Log("[GlobalCameraMouseLook] Mouse detectado → yaw rotando.");
            targetYaw += mouseX * mouseSensitivity;
            idleTimer = 0f;
        }
        else if (enableIdleLook)
        {
            idleTimer += Time.deltaTime;
            targetYaw += Mathf.Sin(idleTimer * idleLookSpeed) * (idleLookAmplitude * Time.deltaTime);

            Debug.Log("[GlobalCameraMouseLook] Idle look activo.");
        }

        //----------------------------
        // 🔹 ROTACIÓN SUAVE DEL CUERPO
        //----------------------------
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * rotationSmoothSpeed);

        Quaternion bodyTarget = Quaternion.Euler(0f, currentYaw, 0f);
        localBodyTransform.rotation = Quaternion.Slerp(localBodyTransform.rotation, bodyTarget, Time.deltaTime * rotationSmoothSpeed);

        Debug.Log("[GlobalCameraMouseLook] Rotando cuerpo del player.");

        //----------------------------
        // 🔹 MOVIMIENTO DE CABEZA (opcional)
        //----------------------------
        if (localHeadTransform != null)
        {
            float headPitch = -mouseY * 0.2f;
            float headYaw = mouseX * 0.4f;

            Quaternion headTarget = Quaternion.Euler(headPitch, headYaw, 0f);
            localHeadTransform.localRotation = Quaternion.Slerp(localHeadTransform.localRotation, headTarget, Time.deltaTime * rotationSmoothSpeed);

            Debug.Log("[GlobalCameraMouseLook] Rotando cabeza del player.");
        }
        else
        {
            Debug.LogWarning("[GlobalCameraMouseLook] No se encontró HeadTransform. Solo se rota el cuerpo.");
        }
    }


    // ---------------------------------------------------------
    // 🔎 LOCAL PLAYER CACHE
    // ---------------------------------------------------------
    private void EnsureLocalPlayerReference()
    {
        if (localPlayerObject != null && localPlayerObject.IsSpawned)
            return;

        if (NetworkManager.Singleton == null)
            return;

        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(localClientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                localPlayerObject = client.PlayerObject;
                localBodyTransform = localPlayerObject.transform;

                Debug.Log("[GlobalCameraMouseLook] Encontrado Player local: " + localPlayerObject.name);

                // Buscar la cabeza
                Transform head = localBodyTransform.Find("Head");
                if (head == null) head = localBodyTransform.Find("Cabeza");

                if (head == null)
                {
                    Debug.LogWarning("[GlobalCameraMouseLook] No se encontró Head. Solo rotará el cuerpo.");
                }
                else
                {
                    localHeadTransform = head;
                    Debug.Log("[GlobalCameraMouseLook] Head detectada correctamente.");
                }
            }
            else
            {
                Debug.LogWarning("[GlobalCameraMouseLook] El PlayerObject no existe todavía.");
            }
        }
        else
        {
            Debug.LogWarning("[GlobalCameraMouseLook] No se encontró el ConnectedClient local.");
        }
    }
}
