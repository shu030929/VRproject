using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerMover : MonoBehaviour
{
    [Header("Move")]
    public float walkSpeed = 4.0f;
    public float sprintSpeed = 7.0f;
    public float gravity = -9.81f;

    [Header("Turn")]
    public float turnSpeedDeg = 270f;     // A/D를 누르고 있는 동안 초당 회전 각도
    public bool snapOnKeyTurn = false;    // true면 A/D 누를 때 즉시 90도씩 스냅 회전

    [Header("Look (camera is child of player)")]
    public Transform cameraTransform;     // 플레이어의 자식 Main Camera
    public float mouseSensitivity = 80f;
    public float minPitch = -25f;         // 아래로 제한
    public float maxPitch = 80f;          // 위로 제한
    public float maxYawOffset = 45f;      // 좌우 미세 보정 최대(±도)
    public bool lockCursor = true;

    private CharacterController cc;
    private float yVel;
    private float pitch = 0f;             // 카메라 상하 보정
    private float yawOffset = 0f;         // 카메라 좌우 보정(플레이어 회전에 더해짐)
    private float snapAccum = 0f;         // 스냅 중복 입력 방지용

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (lockCursor) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        if (!cameraTransform) cameraTransform = Camera.main ? Camera.main.transform : null;
    }

    void Update()
    {
        HandleLook();
        HandleMoveAndTurn();
        ApplyCameraRotation();
    }

    void HandleLook()
    {
        if (!cameraTransform) return;

        float mouseX = 0f, mouseY = 0f;
        #if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 d = mouse.delta.ReadValue();
            mouseX = d.x;
            mouseY = d.y;
        }
        else
        #endif
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }

        mouseX *= mouseSensitivity * Time.deltaTime;
        mouseY *= mouseSensitivity * Time.deltaTime;

        // 마우스: 좌우/상하 미세 보정만
        yawOffset += mouseX;
        yawOffset = Mathf.Clamp(yawOffset, -maxYawOffset, maxYawOffset);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void HandleMoveAndTurn()
    {
        float v = 0f;       // 전/후
        float turn = 0f;    // 좌/우 회전 입력
        bool sprint = false;

        #if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            // 전/후
            if (kb.wKey.isPressed) v += 1f;
            if (kb.sKey.isPressed) v -= 1f;

            // 회전
            if (kb.aKey.isPressed) turn -= 1f;
            if (kb.dKey.isPressed) turn += 1f;

            sprint = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;

            // 스냅 회전(옵션)
            if (snapOnKeyTurn)
            {
                if (kb.aKey.wasPressedThisFrame) { transform.Rotate(0f, -90f, 0f); yawOffset = 0f; }
                if (kb.dKey.wasPressedThisFrame) { transform.Rotate(0f,  90f, 0f); yawOffset = 0f; }
                turn = 0f; // 스냅 모드일 땐 연속 회전 사용 안 함
            }
        }
        else
        #endif
        {
            v = Input.GetKey(KeyCode.W) ? 1f : (Input.GetKey(KeyCode.S) ? -1f : 0f);
            if (Input.GetKey(KeyCode.A)) turn -= 1f;
            if (Input.GetKey(KeyCode.D)) turn += 1f;
            sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (snapOnKeyTurn)
            {
                if (Input.GetKeyDown(KeyCode.A)) { transform.Rotate(0f, -90f, 0f); yawOffset = 0f; }
                if (Input.GetKeyDown(KeyCode.D)) { transform.Rotate(0f,  90f, 0f); yawOffset = 0f; }
                turn = 0f;
            }
        }

        // 연속 회전 모드: A/D 누르는 동안 부드럽게 회전 + 카메라 보정 리셋
        if (!snapOnKeyTurn && Mathf.Abs(turn) > 0.0001f)
        {
            transform.Rotate(0f, turn * turnSpeedDeg * Time.deltaTime, 0f);
            yawOffset = 0f; // 키로 방향을 바꾸면 시야는 즉시 그쪽을 보도록 보정 제거
        }

        // 전/후 이동
        float speed = (sprint ? sprintSpeed : walkSpeed);
        Vector3 planar = transform.forward * v * speed;

        // 중력
        if (cc.isGrounded) yVel = -0.5f;
        else yVel += gravity * Time.deltaTime;

        planar.y = yVel;
        cc.Move(planar * Time.deltaTime);
    }

    void ApplyCameraRotation()
    {
        if (!cameraTransform) return;
        // 플레이어 Y 회전을 상속(자식)이므로 여기서는 보정만 적용
        cameraTransform.localRotation = Quaternion.Euler(pitch, yawOffset, 0f);
    }
}
