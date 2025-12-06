using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(NetworkObject))]
public class NetPlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float gravity   = -20f;

    [Header("Jump Grace")]
    public float coyoteTime = 0.1f;  // after leaving ground
    public float jumpBuffer = 0.1f;  // before landing

    CharacterController cc;

    // server-side vertical velocity / jump timers
    float vy;
    float lastGroundedTime     = float.NegativeInfinity;
    float lastJumpPressedTime  = float.NegativeInfinity;

    // latest input for this player (used by the server)
    Vector2 serverMoveInput;
    bool serverJumpPressed;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        // Only the local owner should lock the cursor / read input
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Update()
    {
        // 1) OWNERS (host and client) read input every frame
        if (IsOwner)
        {
            ReadAndSendInput();
        }

        // 2) Only the SERVER simulates actual movement
        if (!IsServer)
            return;

        SimulateOnServer();
    }

    // ---------------- INPUT (runs on owners) ----------------

    void ReadAndSendInput()
    {
        // WASD input (normalized)
        Vector2 in2 = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        bool jumpDown = Input.GetKeyDown(KeyCode.Space);

        // If this player is the HOST (IsServer + IsOwner),
        // we can write input directly without RPC
        if (IsServer)
        {
            serverMoveInput = in2;
            if (jumpDown)
                serverJumpPressed = true;
        }
        else
        {
            // pure client: send input to server
            SendInputServerRpc(in2, jumpDown);
        }
    }

    [ServerRpc]
    void SendInputServerRpc(Vector2 move, bool jump)
    {
        serverMoveInput = move;
        if (jump)
            serverJumpPressed = true;
    }

    // ---------------- SERVER SIMULATION ----------------

    void SimulateOnServer()
    {
        // direction based on main camera (host’s view)
        Transform cam = Camera.main ? Camera.main.transform : transform;

        Vector3 f = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 r = Vector3.ProjectOnPlane(cam.right,  Vector3.up).normalized;

        Vector3 v = (f * serverMoveInput.y + r * serverMoveInput.x) * moveSpeed;

        // grounded
        if (cc.isGrounded)
            lastGroundedTime = Time.time;

        if (cc.isGrounded && vy < 0f)
            vy = -2f;

        // buffered jump
        if (serverJumpPressed)
        {
            lastJumpPressedTime = Time.time;
            serverJumpPressed = false; // consume edge
        }

        bool canJump    = (Time.time - lastGroundedTime)    <= coyoteTime;
        bool queuedJump = (Time.time - lastJumpPressedTime) <= jumpBuffer;

        if (canJump && queuedJump)
        {
            vy = jumpSpeed;
            lastJumpPressedTime = float.NegativeInfinity;
        }

        // gravity
        vy += gravity * Time.deltaTime;

        // apply move
        v.y = vy;
        cc.Move(v * Time.deltaTime);

        // face movement direction (horizontal only)
        Vector3 horizVel = cc.velocity; horizVel.y = 0f;
        if (horizVel.sqrMagnitude > 0.001f)
        {
            Vector3 dir = horizVel.normalized;
            transform.forward = Vector3.Slerp(transform.forward, dir, 10f * Time.deltaTime);
        }
    }
}
