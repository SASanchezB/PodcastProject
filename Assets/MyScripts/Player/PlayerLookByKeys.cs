using Unity.Netcode;
using UnityEngine;

public class PlayerLookByKeys : NetworkBehaviour
{
    [Header("Referencias")]
    public Transform headTransform;
    public Transform bodyTransform; 

    [Header("Velocidades")]
    public float headYawSpeed = 45f;
    public float headPitchSpeed = 45f;
    public float bodyTurnSpeed = 90f;

    [Header("Límites de cabeza")]
    public float maxYaw = 35f;
    public float maxPitch = 35f;

    [Header("Cuerpo sigue cabeza")]
    public float bodyFollowSpeed = 4f;

    private float headYaw;   // axis para izquierda / derecha
    private float headPitch; // axis para arriba / abajo
    private float bodyYaw;   // rotación del player

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        bodyYaw = bodyTransform.eulerAngles.y;
    }

    private void Update()
    {
        HandleInput();
        ApplyHeadRotation();
        ApplyBodyFollow();
    }

    // ---------------------INPUT---------------------
    private void HandleInput()
    {
        // rotacion horizontal de cabeza -> A–D
        if (Input.GetKey(KeyCode.A))
            headYaw -= headYawSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            headYaw += headYawSpeed * Time.deltaTime;

        // rotacion vertical de cabeza -> W–S
        if (Input.GetKey(KeyCode.W))
            headPitch -= headPitchSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            headPitch += headPitchSpeed * Time.deltaTime;

        // clamp de cabeza
        headYaw = Mathf.Clamp(headYaw, -maxYaw, maxYaw);
        headPitch = Mathf.Clamp(headPitch, -maxPitch, maxPitch);

        // rotacion del cuerpo -> Q–E
        if (Input.GetKey(KeyCode.Q))
            bodyYaw -= bodyTurnSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E))
            bodyYaw += bodyTurnSpeed * Time.deltaTime;
    }

    // ------------------HEAD ROTATION------------------------
    private void ApplyHeadRotation()
    {
        headTransform.localRotation = Quaternion.Euler(
            headPitch,
            headYaw,
            0f
        );
    }

    // -------------------QUE EL CUERPO GIRE CON LA CABEZA-----------------------
    private void ApplyBodyFollow()
    {
        // cuando la cabeza mira mucho hacia un lado el cuerpo acompaña la rotacion
        float followThreshold = maxYaw * 0.8f;

        if (Mathf.Abs(headYaw) > followThreshold)
        {
            // la parte que el cuerpo sigo
            bodyYaw += headYaw * Time.deltaTime * bodyFollowSpeed;

            // centrar la cabeza (vuelve al centro)
            headYaw = Mathf.Lerp(headYaw, 0f, Time.deltaTime * 2f);
        }

        bodyTransform.rotation = Quaternion.Euler(0f, bodyYaw, 0f);
    }
}
