using UnityEngine;
using UnityEngine.AI;

public class GuardFSM : MonoBehaviour
{
    public enum State { Patrol, Chase, Search }
    public State state = State.Patrol;

    [Header("Waypoints")]
    public Transform[] waypoints;      // W_0..W_4

    [Header("Detection")]
    public float sightDistance = 12f;
    public float fovDegrees    = 90f;
    public float loseSightTime = 1.5f; // how long after losing vision before Search
    public float searchDuration = 3f;  // how long to search at lastKnownPos
    public float catchDistance = 1f;

    [Header("Speeds")]
    public float patrolSpeed = 2f;
    public float chaseSpeed  = 4f;
    public float searchSpeed = 2.2f;

    [Header("Optional Eye")]
    public Transform eye;              // can be null

    NavMeshAgent agent;
    Transform player;                  // NOT shown in Inspector
    int wpIndex = 0;
    float lostTimer = 0f;
    float searchTimer = 0f;
    Vector3 lastKnownPos;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        // basic nav setup
        if (agent && waypoints != null && waypoints.Length > 0)
        {
            wpIndex = 0;
            agent.SetDestination(waypoints[wpIndex].position);
        }

        // force initial state + color to Patrol (blue)
        To(State.Patrol);
    }

    void Update()
    {
        if (!agent) return;

        AutoBindPlayer();

        bool seen = player && CanSeePlayer();
        if (seen && player)
        {
            lastKnownPos = player.position;
            lostTimer = 0f;
        }

        switch (state)
        {
            case State.Patrol:
                PatrolUpdate(seen);
                break;

            case State.Chase:
                ChaseUpdate(seen);
                break;

            case State.Search:
                SearchUpdate(seen);
                break;
        }
    }

    // ---------------- STATE UPDATES ----------------

    void PatrolUpdate(bool seen)
    {
        agent.speed = patrolSpeed;
        agent.isStopped = false;

        if (waypoints != null && waypoints.Length > 0)
        {
            if (!agent.pathPending && agent.remainingDistance <= 0.25f)
            {
                wpIndex = (wpIndex + 1) % waypoints.Length;
                agent.SetDestination(waypoints[wpIndex].position);
            }
        }

        if (seen) To(State.Chase);
    }

    void ChaseUpdate(bool seen)
    {
        if (!player)
        {
            To(State.Patrol);
            return;
        }

        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(player.position);

        float d = Vector3.Distance(transform.position, player.position);
        if (d <= catchDistance)
        {
            Debug.Log("Guard caught the player!");
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
            return;
        }

        if (!seen)
        {
            lostTimer += Time.deltaTime;
            if (lostTimer > loseSightTime)
            {
                // go to search at last known position
                To(State.Search);
            }
        }
        else
        {
            lostTimer = 0f;
        }
    }

    void SearchUpdate(bool seen)
    {
        agent.speed = searchSpeed;
        agent.isStopped = false;
        agent.SetDestination(lastKnownPos);

        searchTimer += Time.deltaTime;

        // if we see the player again while searching → chase
        if (seen)
        {
            To(State.Chase);
            return;
        }

        // if we reached lastKnownPos or searched long enough → back to patrol
        if ((!agent.pathPending && agent.remainingDistance <= 0.25f) ||
            searchTimer >= searchDuration)
        {
            To(State.Patrol);
        }
    }

    // ---------------- HELPERS ----------------

    void To(State s)
    {
        state = s;
        lostTimer = 0f;
        searchTimer = 0f;

        // Color feedback
        var r = GetComponentInChildren<Renderer>();
        if (r)
        {
            r.material.color = s switch
            {
                State.Patrol => Color.cyan,   // blue-ish
                State.Chase  => Color.red,    // red
                State.Search => Color.yellow, // yellow
                _            => Color.white
            };
        }

        Debug.Log("Guard -> " + state);
    }

    void AutoBindPlayer()
    {
        if (player != null) return;

        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return;

        // choose closest
        Transform closest = players[0].transform;
        float best = (closest.position - transform.position).sqrMagnitude;

        for (int i = 1; i < players.Length; i++)
        {
            float sqr = (players[i].transform.position - transform.position).sqrMagnitude;
            if (sqr < best)
            {
                best = sqr;
                closest = players[i].transform;
            }
        }

        player = closest;
        lastKnownPos = player.position;
        Debug.Log("Guard bound to: " + player.name);
    }

    bool CanSeePlayer()
    {
        if (!player) return false;

        Vector3 from = eye ? eye.position : transform.position + Vector3.up * 1.2f;
        Vector3 to = player.position - from;

        // distance
        if (to.sqrMagnitude > sightDistance * sightDistance)
            return false;

        // FOV
        Vector3 forward = eye ? eye.forward : transform.forward;
        float angle = Vector3.Angle(forward, to);
        if (angle > fovDegrees * 0.5f)
            return false;

        // LOS: anything that has NetPlayerController in its parents
        if (Physics.Raycast(from, to.normalized, out RaycastHit hit, sightDistance,
                            ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.transform.GetComponentInParent<NetPlayerController>() != null;
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightDistance);
    }
}
