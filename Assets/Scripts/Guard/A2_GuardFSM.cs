using UnityEngine;

public class A2_GuardFSM : MonoBehaviour {
    public enum State { Idle, Patrol, Chase, Search }
    public State state = State.Idle;

    [Header("Refs")]
    public Transform player;
    public Transform[] waypoints;   // fill at least 2
    public Transform eye;           // optional

    [Header("Tuning")]
    public float idleTime = 2f;
    public float patrolSpeed = 2f;
    public float chaseSpeed  = 4f;
    public float searchSpeed = 2.2f;
    public float arriveDist  = 0.25f;          // planar radius to consider "arrived"

    [Header("Detection")]
    public float sightDistance = 12f;
    public float fovDegrees    = 60f;
    public LayerMask occluders;

    // runtime
    int wpIndex = 0;
    float stateEnd = 0f;
    Vector3 lastKnownPos;
    float lostTimer = 0f;
    float segStartTime, searchStartTime;

    void Start(){ To(State.Idle); }

    void Update(){
        bool seen = CanSeePlayer();
        if (seen) { lastKnownPos = player.position; lostTimer = 0f; }
        
        var vc = GetComponent<VisionConeMesh>();
        if (vc && vc.material){
            Color c = vc.material.color;
            c.a = 0.18f;
            vc.material.color = seen ? new Color(1f, 0.3f, 0.2f, c.a)   // red when seeing
                                    : new Color(1f, 0.9f, 0.2f, c.a);  // yellow otherwise
}


        switch (state){
            case State.Idle:
                if (Time.time >= stateEnd) To(State.Patrol);
                if (seen) To(State.Chase);
                break;

            case State.Patrol:
                if (waypoints == null || waypoints.Length == 0) break;
                var target = waypoints[wpIndex].position;
                MoveTowards(target, patrolSpeed);

                // PLANAR check (ignore Y) + segment timeout safeguard
                if (PlanarReached(transform.position, target) || Time.time - segStartTime > 10f)
                    NextWaypoint();

                if (seen) To(State.Chase);
                break;

            case State.Chase:
                MoveTowards(player.position, chaseSpeed);
                if (!seen){
                    lostTimer += Time.deltaTime;
                    if (lostTimer > 1.5f) To(State.Search);
                }
                break;

            case State.Search:
                MoveTowards(lastKnownPos, searchSpeed);

                // PLANAR arrival or timeout returns to patrol
                if (PlanarReached(transform.position, lastKnownPos) || Time.time - searchStartTime > 5f)
                    To(State.Patrol);

                if (seen) To(State.Chase);
                break;
        }
    }

    // ---------- helpers ----------

    bool PlanarReached(Vector3 a, Vector3 b){
        a.y = 0f; b.y = 0f;
        return (a - b).sqrMagnitude <= arriveDist * arriveDist;
    }

    void NextWaypoint(){
        if (waypoints == null || waypoints.Length == 0) return;
        wpIndex = (wpIndex + 1) % waypoints.Length;     // <-- loops forever
        segStartTime = Time.time;
    }

    void MoveTowards(Vector3 target, float speed){
        Vector3 dir = target - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.forward = Vector3.RotateTowards(transform.forward, dir.normalized, 8f * Time.deltaTime, 999f);
        transform.position += dir.normalized * speed * Time.deltaTime;
    }

    bool CanSeePlayer(){
        Vector3 from = eye ? eye.position : transform.position + Vector3.up * 1.2f;
        Vector3 to   = player.position - from;
        if (to.sqrMagnitude > sightDistance * sightDistance) return false;

        float angle = Vector3.Angle(eye ? eye.forward : transform.forward, to);
        if (angle > fovDegrees * 0.5f) return false;

        if (occluders.value != 0 && Physics.Raycast(from, to.normalized, out var hit, sightDistance, occluders))
            return hit.transform == player;

        return true;
    }

    void To(State s){
        state = s;
        switch (s){
            case State.Idle:   stateEnd = Time.time + idleTime; break;
            case State.Patrol: segStartTime = Time.time; break;
            case State.Chase:  break;
            case State.Search: searchStartTime = Time.time; break;
        }
        // visual cue
        var r = GetComponentInChildren<Renderer>();
        if (r) r.material.color = s switch {
            State.Idle   => Color.gray,
            State.Patrol => Color.cyan,
            State.Chase  => Color.red,
            State.Search => Color.yellow,
            _ => Color.white
        };
        Debug.Log($"Guard -> {state}");
    }

    void OnDrawGizmosSelected(){
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, sightDistance);
    }
}
