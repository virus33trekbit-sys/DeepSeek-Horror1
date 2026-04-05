using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float mouseSensitivity = 2f;
    
    [Header("Гравитация")]
    public float gravity = -9.81f;
    
    private CharacterController controller;
    private Camera playerCamera;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (playerCamera == null)
        {
            Debug.LogError("Нет камеры! Добавь Camera в дочерний объект");
        }
    }
    
    void Update()
    {
        // ========== ПОВОРОТ КАМЕРЫ (ИСПРАВЛЕНО) ==========
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Накопление поворотов
        yRotation += mouseX;   // Влево-вправо (Yaw)
        xRotation -= mouseY;   // Вверх-вниз (Pitch)
        
        // Ограничиваем поворот вверх-вниз (чтобы не переворачиваться)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        // ПОВОРАЧИВАЕМ ВЕСЬ ОБЪЕКТ игрока влево-вправо
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        
        // ПОВОРАЧИВАЕМ КАМЕРУ и вверх-вниз, и влево-вправо
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // ========== ДВИЖЕНИЕ ==========
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        controller.Move(move * currentSpeed * Time.deltaTime);
        
        // ========== ПРЫЖКИ ==========
        isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // Диагностика по F1
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"Y Rotation (влево-вправо): {yRotation}");
            Debug.Log($"X Rotation (вверх-вниз): {xRotation}");
            Debug.Log($"Поворот игрока: {transform.eulerAngles}");
            Debug.Log($"Поворот камеры: {playerCamera.transform.localEulerAngles}");
        }
    }
}