using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public float mouseSensitivity = 2f;
    public float touchSensitivity = 0.15f;
    public float pitchLimit = 70f;

    Transform cam;
    float pitch;

    void Start()
    {
        var c = GetComponentInChildren<Camera>();
        if (c != null) cam = c.transform;
    }

    void Update()
    {
        float yawDelta = 0f;
        float pitchDelta = 0f;

        // PC and Mac: hold left mouse button and drag.
        // Ignore the bottom-left area so the joystick doesn't turn the camera.
        if (Input.GetMouseButton(0))
        {
            Vector2 m = Input.mousePosition;
            bool onJoystick = m.x < Screen.width * 0.3f && m.y < Screen.height * 0.4f;
            if (!onJoystick)
            {
                yawDelta += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitchDelta -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            }
        }

        // Phones: drag one finger on the right half of the screen
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Moved && t.position.x > Screen.width * 0.5f)
            {
                yawDelta += t.deltaPosition.x * touchSensitivity;
                pitchDelta -= t.deltaPosition.y * touchSensitivity;
            }
        }

        // Left and right turns the player body, so movement follows the view
        transform.Rotate(0, yawDelta, 0);

        // Up and down tilts only the camera
        pitch = Mathf.Clamp(pitch + pitchDelta, -pitchLimit, pitchLimit);
        if (cam != null) cam.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
}