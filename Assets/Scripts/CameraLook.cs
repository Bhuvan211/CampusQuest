using UnityEngine;

// Looking around (mouse drag on PC, right-half drag on phones).
// Press R to toggle a top-down map view.
public class CameraLook : MonoBehaviour
{
    #region Settings
    public float mouseSensitivity = 2f;
    public float touchSensitivity = 0.15f;
    public float pitchLimit = 70f;
    public Vector3 mapCentre = new Vector3(0f, 150f, -10f);
    public float mapOrthoSize = 120f;
    #endregion

    #region State
    Camera cam;
    Transform firstPersonParent;
    Vector3 firstPersonLocalPos;
    float pitch;
    bool topDown;
    #endregion

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ToggleTopDown();

        if (topDown || cam == null) return;
        HandleLook();
    }

    #region Look controls
    void HandleLook()
    {
        float yawDelta = 0f;
        float pitchDelta = 0f;

        // PC and Mac: hold the left mouse button and drag.
        // The bottom-left area is ignored so the joystick doesn't turn the camera.
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

        // Left and right turns the body, so movement follows the view
        transform.Rotate(0, yawDelta, 0);

        // Up and down tilts only the camera
        pitch = Mathf.Clamp(pitch + pitchDelta, -pitchLimit, pitchLimit);
        cam.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
    #endregion

    #region Top-down map view
    void ToggleTopDown()
    {
        if (cam == null) return;
        topDown = !topDown;

        if (topDown)
        {
            // Detach the camera so it floats above the campus
            firstPersonParent = cam.transform.parent;
            firstPersonLocalPos = cam.transform.localPosition;

            cam.transform.SetParent(null, true);
            cam.orthographic = true;
            cam.orthographicSize = mapOrthoSize;
            cam.transform.position = mapCentre;
            cam.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        else
        {
            // Reattach to the player and restore the first-person view
            cam.orthographic = false;
            cam.transform.SetParent(firstPersonParent, false);
            cam.transform.localPosition = firstPersonLocalPos;
            cam.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
    }
    #endregion
}