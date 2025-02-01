using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private Camera playerCamera;
    private float verticalVelocity;
    private float gravity = 9.81f;
    private float rotationX = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // Bloque le curseur au centre de l'écran
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Move();
        RotateView();
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal"); // A & D ou flèches gauche/droite
        float moveZ = Input.GetAxis("Vertical");   // W & S ou flèches avant/arrière

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move *= moveSpeed * Time.deltaTime;

        // Gérer la gravité et le saut
        if (controller.isGrounded)
        {
            verticalVelocity = -gravity * Time.deltaTime;

            if (Input.GetButtonDown("Jump")) // Espace pour sauter
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        move.y = verticalVelocity * Time.deltaTime;
        controller.Move(move);
    }

    void RotateView()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotation du joueur sur l'axe Y
        transform.Rotate(Vector3.up * mouseX);

        // Rotation de la caméra sur l'axe X (haut/bas)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}
