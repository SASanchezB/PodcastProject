using Unity.Netcode;
using UnityEngine;

public class PlayerCameraLook : NetworkBehaviour
{
    public float sensitivity = 200f;

    private float rotationX = 0f;

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;  // Desactivo el script para otros jugadores
            return;
        }

        // Oculta y bloquea el cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!IsOwner) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Rotación vertical (mirar arriba y abajo)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Rotación horizontal (gira el cuerpo del jugador)
        if (transform.parent != null)
        {
            transform.parent.Rotate(Vector3.up * mouseX);
        }
    }
}
