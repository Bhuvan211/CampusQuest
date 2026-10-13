using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    Rigidbody rb;

    void Awake() { rb = GetComponent<Rigidbody>(); }

    void FixedUpdate()
    {
        // Keyboard input (Windows, Mac, Editor)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Touch joystick overrides keyboard when it is being used (Android, iOS)
        Vector2 stick = VirtualJoystick.Direction;
        if (stick.sqrMagnitude > 0.01f)
        {
            h = stick.x;
            v = stick.y;
        }

        Vector3 move = transform.forward * v + transform.right * h;
        if (move.magnitude > 1f) move.Normalize();
        move *= speed;

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}