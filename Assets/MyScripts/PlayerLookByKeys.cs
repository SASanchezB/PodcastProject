using Unity.Netcode;
using UnityEngine;

public class PlayerLookByKeys : NetworkBehaviour
{
    [Header("Referencias")]
    public Transform headTransform;
    public Transform bodyTransform; // PlayerPrefab root

    [Header("Velocidades")]
    public float headYawSpeed = 45f;
    public float headPitchSpeed = 45f;
    public float bodyTurnSpeed = 90f;

    [Header("Límites de cabeza")]
    public float maxYaw = 35f;
    public float maxPitch = 35f;

    [Header("Cuerpo sigue cabeza")]
    public float bodyFollowSpeed = 4f;

    private float headYaw;   // izquierda / derecha
    private float headPitch; // arriba / abajo
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

    // ------------------------------------------
    // 1) INPUT
    // ------------------------------------------
    private void HandleInput()
    {
        // Rotación horizontal de cabeza (A–D)
        if (Input.GetKey(KeyCode.A))
            headYaw -= headYawSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            headYaw += headYawSpeed * Time.deltaTime;

        // Rotación vertical de cabeza (W–S)
        if (Input.GetKey(KeyCode.W))
            headPitch -= headPitchSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            headPitch += headPitchSpeed * Time.deltaTime;

        // Clamp de cabeza
        headYaw = Mathf.Clamp(headYaw, -maxYaw, maxYaw);
        headPitch = Mathf.Clamp(headPitch, -maxPitch, maxPitch);

        // Rotación del cuerpo (Q–E)
        if (Input.GetKey(KeyCode.Q))
            bodyYaw -= bodyTurnSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E))
            bodyYaw += bodyTurnSpeed * Time.deltaTime;
    }

    // ------------------------------------------
    // 2) HEAD ROTATION
    // ------------------------------------------
    private void ApplyHeadRotation()
    {
        headTransform.localRotation = Quaternion.Euler(
            headPitch,
            headYaw,
            0f
        );
    }

    // ------------------------------------------
    // 3) BODY FOLLOWS THE HEAD
    // ------------------------------------------
    private void ApplyBodyFollow()
    {
        // Cuando la cabeza mira mucho hacia un lado, el cuerpo acompaña.
        float followThreshold = maxYaw * 0.8f;

        if (Mathf.Abs(headYaw) > followThreshold)
        {
            // Cuerpo sigue
            bodyYaw += headYaw * Time.deltaTime * bodyFollowSpeed;

            // La cabeza vuelve un poco hacia el centro
            headYaw = Mathf.Lerp(headYaw, 0f, Time.deltaTime * 2f);
        }

        bodyTransform.rotation = Quaternion.Euler(0f, bodyYaw, 0f);
    }
}
