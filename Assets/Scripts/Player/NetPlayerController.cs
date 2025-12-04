using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController), typeof(NetworkObject))]
public class NetPlayerController : NetworkBehaviour {
    public float moveSpeed = 5f, jumpSpeed = 5f, gravity = -20f, lookSens = 150f;
    public float coyoteTime = 0.1f;   // grace after leaving ground
    public float jumpBuffer = 0.1f;   // grace after pressing jump

    CharacterController cc;
    float vy;
    float lastGroundedTime = float.NegativeInfinity;
    float lastJumpPressedTime = float.NegativeInfinity;

    void Awake() {
        cc = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn() {
        // Only the local owner should lock the cursor / read input.
        if (IsOwner) Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        // IMPORTANT: only the owner drives the character
        if (!IsOwner) return;

        // move in camera plane (fixed camera is fine)
        var cam = Camera.main.transform;
        Vector3 f = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 r = Vector3.ProjectOnPlane(cam.right,  Vector3.up).normalized;
        Vector2 in2 = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        Vector3 v = (f * in2.y + r * in2.x) * moveSpeed;

        // jump buffering / coyote
        if (Input.GetKeyDown(KeyCode.Space)) lastJumpPressedTime = Time.time;
        if (cc.isGrounded) lastGroundedTime = Time.time;
        if (cc.isGrounded && vy < 0f) vy = -2f;

        bool canJump    = (Time.time - lastGroundedTime) <= coyoteTime;
        bool queuedJump = (Time.time - lastJumpPressedTime) <= jumpBuffer;
        if (canJump && queuedJump) {
            vy = jumpSpeed;
            lastJumpPressedTime = float.NegativeInfinity; // consume
        }

        // gravity
        vy += gravity * Time.deltaTime;

        // apply move
        v.y = vy;
        cc.Move(v * Time.deltaTime);

        // face movement direction (horizontal only)
        Vector3 horizVel = cc.velocity; horizVel.y = 0f;
        if (horizVel.sqrMagnitude > 0.001f) {
            Vector3 dir = horizVel.normalized;
            transform.forward = Vector3.Slerp(transform.forward, dir, 10f * Time.deltaTime);
        }
    }
}
