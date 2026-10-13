using UnityEngine;

// Walks an NPC around a loop of waypoints.
// The NPC is kinematic, so it moves by script but still sends trigger events.
public class NPCPatrol : MonoBehaviour
{
    public Vector3[] waypoints;
    public float speed = 1.5f;

    Rigidbody rb;
    int index;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Vector3 target = waypoints[index];
        Vector3 pos = rb.position;
        Vector3 next = Vector3.MoveTowards(pos, target, speed * Time.fixedDeltaTime);

        // Face the direction of travel
        Vector3 dir = target - pos;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f) rb.MoveRotation(Quaternion.LookRotation(dir));

        rb.MovePosition(next);

        // Reached this waypoint, so move on to the next one (and loop back to the start)
        if ((next - target).sqrMagnitude < 0.01f)
            index = (index + 1) % waypoints.Length;
    }
}