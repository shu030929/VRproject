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
    public float turnSpeedDeg = 270f;
    public bool snapOnKeyTurn = false;

    [Header("Look (camera is child of player)")]
    public Transform cameraTransform;
    public float mouseSensitivity = 80f;
    public float minPitch = -25f;
    public float maxPitch = 80f;
    public float maxYawOffset = 45f;
    public bool lockCursor = true;

    [Header("Push Ball")]
    public float pushPower = 3f;

    private CharacterController cc;
    private float yVel;
    private float pitch = 0f;
    private float yawOffset = 0f;

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
        #else
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        #endif

        mouseX *= mouseSensitivity * Time.deltaTime;
        mouseY *= mouseSensitivity * Time.deltaTime;

        yawOffset += mouseX;
        yawOffset = Mathf.Clamp(yawOffset, -maxYawOffset, maxYawOffset);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void HandleMoveAndTurn()
    {
        float v = 0f; float turn = 0f; bool sprint = false;

        #if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed) v += 1f;
            if (kb.sKey.isPressed) v -= 1f;
            if (kb.aKey.isPressed) turn -= 1f;
            if (kb.dKey.isPressed) turn += 1f;
            sprint = kb.leftShiftKey.isPressed;
        }
        #else
        v = Input.GetKey(KeyCode.W) ? 1f : (Input.GetKey(KeyCode.S) ? -1f : 0f);
        if (Input.GetKey(KeyCode.A)) turn -= 1f;
        if (Input.GetKey(KeyCode.D)) turn += 1f;
        sprint = Input.GetKey(KeyCode.LeftShift);
        #endif

        if (Mathf.Abs(turn) > 0.0001f)
        {
            transform.Rotate(0f, turn * turnSpeedDeg * Time.deltaTime, 0f);
            yawOffset = 0f;
        }

        float speed = (sprint ? sprintSpeed : walkSpeed);
        Vector3 planar = transform.forward * v * speed;

        if (cc.isGrounded) yVel = -0.5f;
        else yVel += gravity * Time.deltaTime;

        planar.y = yVel;
        cc.Move(planar * Time.deltaTime);
    }

    void ApplyCameraRotation()
    {
        if (!cameraTransform) return;
        cameraTransform.localRotation = Quaternion.Euler(pitch, yawOffset, 0f);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.rigidbody;
        if (rb != null && !rb.isKinematic)
        {
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            rb.AddForce(pushDir * pushPower, ForceMode.Impulse);
        }
    }
}
