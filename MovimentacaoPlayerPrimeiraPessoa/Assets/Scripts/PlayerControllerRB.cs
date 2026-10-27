using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerRB : MonoBehaviour
{
    [SerializeField] private float WalkSpeed = 5.5f;
    [SerializeField] private float RunningSpeed = 9.0f;
    [SerializeField] private float JumpForce = 8.0f;

    private Rigidbody rb;
    private Vector2 moveVector = Vector2.zero;
    private float currentMoveSpeed = 0.0f;

    private bool jumpRequested = false;

    [SerializeField] private float groundCheckExtra = 0.1f;
    private CapsuleCollider capsule;
    [SerializeField] private LayerMask groundMak;
    private bool isGrounded;

    private InputAction moveInput;
    private InputAction runInput;
    private InputAction jumpInput;
    private InputAction lookInput;

    [SerializeField] private float LookSensivity = 0.2f;
    [SerializeField] private float LookAngleLimit = 90f;
    private float lookAngle = 0.0f;
    private Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        mainCamera = GetComponentInChildren<Camera>();
        rb.freezeRotation = true;

        //sera utilizada  para definir distancia do raycast para verificar se o player esta no chap
        capsule = GetComponent<CapsuleCollider>();

        moveInput = InputSystem.actions.FindAction("Move");
        runInput = InputSystem.actions.FindAction("Sprint");
        jumpInput = InputSystem.actions.FindAction("Jump");
        lookInput = InputSystem.actions.FindAction("Look");

        //omitir cursor da tela 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //definindo a velocidade atual para iniciar com a velocidade de andar
        currentMoveSpeed = WalkSpeed;

    }

    // Update is called once per frame
    void Update()
    {
        // input é lido todo frame; a fisica é aplicada no fixedUpdate
        moveVector = moveInput.ReadValue<Vector2>();
        currentMoveSpeed = runInput.IsPressed() ? RunningSpeed :  WalkSpeed;
        
        if(jumpInput.WasPressedThisFrame())
        {
            jumpRequested = true;
        }
        Vector2 lookVector = lookInput.ReadValue<Vector2>();
        HandleCamera(lookVector);
    }

    void FixedUpdate()
    {
        //calcula a direção considerando a posição do player pelo transform 
        Vector3 move = (transform.forward * moveVector.y + transform.right * moveVector.x) *currentMoveSpeed;

        // por enquanto mantem a mesma velocidade na vertical
        float verticalVelocity = rb.linearVelocity.y;

        //verifica se o player esta no chao
        float distance = capsule.height / 2f + groundCheckExtra;
        Vector3 origin = transform.TransformPoint(capsule.center);
        isGrounded = Physics.Raycast(origin, Vector3.down, distance, groundMak);
        if(jumpRequested && isGrounded)
        {
            verticalVelocity = JumpForce;
        }
        jumpRequested = false;

        rb.linearVelocity = new Vector3(move.x, verticalVelocity, move.z);
    }
    private void HandleCamera(Vector2 lookVector)
    {
        // atualizao angulo da camnera a partir do eixo y lido (para cima ou para baixo)
        //  é necessario inverter pois quando vamos para frente com o mouse ou o joystick
        // queremos que a rotação da camera em relacao ao eixo x diminua
        lookAngle += -lookVector.y * LookSensivity;
        // aqui garantimos nao permitir uma rotacao maior que o limite definido (90 graus)
        lookAngle = Mathf.Clamp(lookAngle, -LookAngleLimit, LookAngleLimit);
        // aplica a rotação da camera para cima ou para baixo 
        mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);
        // aplica a rotação para esquerda ou direita no objeto do player
        transform.rotation *= Quaternion.Euler(0, lookVector.x * LookSensivity, 0);
    }
}
