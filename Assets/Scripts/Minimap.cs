using UnityEngine;

// Draws a minimap in the top-right corner that rotates with the player.
// The player is always in the centre, facing up the map, so left and right on the map
// match left and right on screen. Buildings are coloured rectangles, and the player is a red dot.
// Attach to a child of Main Camera (CampusSetup does this for you).
public class Minimap : MonoBehaviour
{
    #region Settings
    public int size = 256;
    public int margin = 16;
    public float viewRange = 60f;   // metres shown from the player to the edge of the map
    #endregion

    Texture2D white;

    void OnGUI()
    {
        if (white == null) white = Texture2D.whiteTexture;

        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Rect map = new Rect(Screen.width - size - margin, margin, size, size);
        float scale = size / (viewRange * 2f);   // pixels per metre

        // Background
        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.DrawTexture(map, white);

        // Draw inside the map area only, so shapes outside the edge are clipped
        GUI.BeginGroup(map);

        Transform p = player.transform;
        Vector2 centre = new Vector2(size * 0.5f, size * 0.5f);

        // Buildings
        foreach (var b in BuildingList.All)
        {
            Vector2 c = ToMap(p, b.position, centre, scale);
            float w = Mathf.Max(b.size.x * scale, 2f);
            float h = Mathf.Max(b.size.z * scale, 2f);
            GUI.color = b.zoneColor;
            GUI.DrawTexture(new Rect(c.x - w * 0.5f, c.y - h * 0.5f, w, h), white);
        }

        // Player dot, always at the centre
        GUI.color = Color.red;
        GUI.DrawTexture(new Rect(centre.x - 4, centre.y - 4, 8, 8), white);

        GUI.EndGroup();
        GUI.color = Color.white;
    }

    // Converts a world position into map pixels, relative to the player.
    // InverseTransformPoint gives the position in the player's own space:
    //   local.x = how far right of the player, local.z = how far in front.
    // Forward (in front of the player) is drawn up the map, and right is drawn right.
    Vector2 ToMap(Transform player, Vector3 worldPos, Vector2 centre, float scale)
    {
        Vector3 local = player.InverseTransformPoint(worldPos);
        return new Vector2(centre.x + local.x * scale, centre.y - local.z * scale);
    }
}