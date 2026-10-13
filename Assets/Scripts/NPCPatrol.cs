using UnityEngine;

public class NPCPatrol : MonoBehaviour
{
    public Vector3 pointA;
    public Vector3 pointB;
    public float speed = 1.5f;

    Rigidbody rb;
    Vector3 target;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;   // moved by script, still detects triggers
        rb.useGravity = false;
        rb.freezeRotation = true;
        target = pointB;
    }

    void FixedUpdate()
    {
        Vector3 pos = rb.position;
        Vector3 next = Vector3.MoveTowards(pos, target, speed * Time.fixedDeltaTime);

        // Face the direction of travel
        Vector3 dir = target - pos;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f) rb.rotation = Quaternion.LookRotation(dir);

        rb.MovePosition(next);

        // Reached the end of this leg, so turn around
        if ((next - target).sqrMagnitude < 0.01f)
            target = (target == pointA) ? pointB : pointA;
    }
}