using UnityEngine;
using UnityEngine.AI;

public class GuardFSM : MonoBehaviour {
    public enum State { Idle, Patrol, Chase, Search }
    public State state = State.Idle;

    [Header("Refs")]
    public Transform player;
    public Transform[] waypoints;   // fill at least 2 in Inspector
    public Transform eye;           // optional; otherwise uses transform

    [Header("State Speeds")]
    public float idleTime    = 2f;
    public float patrolSpeed = 2f;
    public float chaseSpeed  = 4f;
    public float searchSpeed = 2.2f;
    public float arriveDist  = 0.25f;

    [Header("Detection")]
    public float sightDistance = 12f;
    public float fovDegrees    = 90f;
    public LayerMask occluders;     // walls; leave 0 if you don’t use it

    // runtime
    int wpIndex = 0;
    float stateEnd = 0f;
    Vector3 lastKnownPos;
    float lostTimer = 0f;
    float searchStartTime = 0f;

    NavMeshAgent agent;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        if (!agent) Debug.LogError("GuardFSM requires a NavMeshAgent.");
    }

    void Start() {
        if (agent) {
            agent.updateRotation = true;
            agent.updatePosition = true;
        }
        To(State.Idle);
    }

    void Update() {
        if (!agent) return;

        bool seen = CanSeePlayer();
        if (seen) { 
            lastKnownPos = player.position; 
            lostTimer = 0f; 
        }

        switch (state) {
            case State.Idle:
                agent.isStopped = true;
                if (Time.time >= stateEnd) To(State.Patrol);
                if (seen) To(State.Chase);
                break;

            case State.Patrol:
                agent.speed = patrolSpeed;
                agent.isStopped = false;

                if (waypoints != null && waypoints.Length > 0) {
                    // assign first destination if none
                    if (!agent.hasPath && !agent.pathPending) {
                        agent.SetDestination(waypoints[wpIndex].position);
                    }

                    // reached current waypoint?
                    if (!agent.pathPending && agent.remainingDistance <= arriveDist) {
                        wpIndex = (wpIndex + 1) % waypoints.Length;
                        agent.SetDestination(waypoints[wpIndex].position);
                    }
                }

                if (seen) To(State.Chase);
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                agent.SetDestination(player.position);

                if (!seen) {
                    lostTimer += Time.deltaTime;
                    if (lostTimer > 1.5f) To(State.Search);
                }
                break;

            case State.Search:
                agent.speed = searchSpeed;
                agent.isStopped = false;
                agent.SetDestination(lastKnownPos);

                // when we get near last known position OR search too long, back to patrol
                if (!agent.pathPending && agent.remainingDistance <= arriveDist) {
                    To(State.Patrol);
                } else if (Time.time - searchStartTime > 5f) {
                    To(State.Patrol);
                }

                if (seen) To(State.Chase);
                break;
        }
    }

    bool CanSeePlayer() {
        if (!player) return false;

        Vector3 from = eye ? eye.position : transform.position + Vector3.up * 1.2f;
        Vector3 toPlayer = player.position - from;

        // range
        if (toPlayer.sqrMagnitude > sightDistance * sightDistance)
            return false;

        // FOV
        Vector3 forward = eye ? eye.forward : transform.forward;
        float angle = Vector3.Angle(forward, toPlayer);
        if (angle > fovDegrees * 0.5f)
            return false;

        // optional occlusion check
        if (occluders.value != 0) {
            if (Physics.Raycast(from, toPlayer.normalized, out RaycastHit hit, sightDistance, occluders)) {
                return hit.transform == player;
            }
        }

        return true;
    }

    void To(State s) {
        state = s;
        switch (s) {
            case State.Idle:
                stateEnd = Time.time + idleTime;
                break;
            case State.Patrol:
                // reset waypoint timer etc.
                break;
            case State.Chase:
                break;
            case State.Search:
                searchStartTime = Time.time;
                break;
        }

        // visual debug: color by state
        var r = GetComponentInChildren<Renderer>();
        if (r) {
            r.material.color = s switch {
                State.Idle   => Color.gray,
                State.Patrol => Color.cyan,
                State.Chase  => Color.red,
                State.Search => Color.yellow,
                _            => Color.white
            };
        }
        Debug.Log($"Guard -> {state}");
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightDistance);
    }
}
