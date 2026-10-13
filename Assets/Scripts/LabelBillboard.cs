using UnityEngine;

// Turns a label to match the camera's rotation, so the text is readable
// in first-person view and in top-down map view.
public class LabelBillboard : MonoBehaviour
{
    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;
    }
}