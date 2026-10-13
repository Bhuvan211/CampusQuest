using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Menu: Campus > Reset to Alliance Layout
// Deletes the previous campus and rebuilds it from BuildingList. Safe to run again.
public static class CampusSetup
{
    #region Constants
    const string MatFolder = "Assets/Materials/Alliance";

    // Top-level objects this script owns. They are deleted and rebuilt on each run.
    static readonly string[] OwnedRoots =
    {
        "Campus", "Environment", "NPCs", "Ground", "Decor", "Pathway_Main", "Main_Road"
    };

    static readonly Dictionary<string, Material> matCache = new Dictionary<string, Material>();
    #endregion

    #region Menu entry
    [MenuItem("Campus/Reset to Alliance Layout")]
    public static void ResetLayout()
    {
        System.IO.Directory.CreateDirectory(MatFolder);
        matCache.Clear();

        // 1. Remove the previous layout
        foreach (var name in OwnedRoots)
        {
            var old = GameObject.Find(name);
            if (old != null) Object.DestroyImmediate(old);
        }

        // 2. Ground and light
        BuildGround();

        // 3. Buildings, zones, labels, dome and the sports field details
        var campus = new GameObject("Campus").transform;
        foreach (var b in BuildingList.All)
        {
            var group = BuildBuilding(campus, b);
            if (b.objectName == "Central_Library") AddDome(group, b);
            if (b.objectName == "Sports_Zone") BuildSportsDetails(group);
        }

        // 4. Environment: paths, gate, road, trees
        var env = new GameObject("Environment").transform;
        BuildPaths(env);
        BuildGate(env);
        BuildRoad(env);
        BuildTrees(env);

        // 5. NPCs on patrol routes
        BuildNpcs();

        // 6. Player spawn at the main gate, plus the minimap on the camera
        PlacePlayer();

        // 7. Save
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Alliance layout applied: " + BuildingList.Count + " buildings. Press Play.");
    }
    #endregion

    #region Ground and light
    static void BuildGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(30, 1, 30); // plane is 10 x 10, so 30 = 300 m
        Paint(ground, BuildingList.Hex("4CAF50"));

        var light = Object.FindAnyObjectByType<Light>();
        if (light != null)
        {
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }
    #endregion

    #region Buildings
    // Each building is a group named after the building. Inside it: Shell, Zone_*, Label.
    static Transform BuildBuilding(Transform parent, BuildingData b)
    {
        var group = new GameObject(b.objectName).transform;
        group.SetParent(parent, false);
        group.position = new Vector3(b.position.x, 0, b.position.z);

        // Shell. The sports field is walkable, so it has no collider.
        bool solid = b.objectName != "Sports_Zone";
        Prim(PrimitiveType.Cube, "Shell", group, new Vector3(0, b.size.y * 0.5f, 0), b.size, b.zoneColor, solid);

        // Zone: an invisible trigger box that detects visits, plus a flat transparent quad on the ground
        var zone = new GameObject("Zone_" + b.objectName).transform;
        zone.SetParent(group, false);

        var box = zone.gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0, b.size.y * 0.5f, 0);
        box.size = new Vector3(b.size.x + 4, b.size.y + 1, b.size.z + 4);

        var visit = zone.gameObject.AddComponent<BuildingVisit>();
        visit.buildingName = b.displayName;
        visit.message = b.description;
        visit.points = 20;

        if (b.objectName == "Central_Library") zone.gameObject.AddComponent<LibraryGoal>();

        var quad = Prim(PrimitiveType.Quad, "Zone_Visual", zone, new Vector3(0, 0.05f, 0),
                        new Vector3(b.size.x + 2, b.size.z + 2, 1), b.zoneColor, false, 0.3f);
        quad.localRotation = Quaternion.Euler(90, 0, 0); // lay flat on the ground

        // Label above the building. The library label sits above its dome.
        float labelY = b.objectName == "Central_Library" ? b.size.y + 19f : b.size.y + 2f;
        MakeLabel(group, b.displayName, new Vector3(0, labelY, 0), b.displayName.Length * 0.6f + 1f);

        return group;
    }

    // Library dome: a sphere of radius 17 centred at roof level, so the top half shows.
    static void AddDome(Transform group, BuildingData b)
    {
        Prim(PrimitiveType.Sphere, "Library_Dome", group, new Vector3(0, b.size.y, 0),
             Vector3.one * 34f, b.zoneColor, false);
    }
    #endregion

    #region Sports field
    // Sports details are children of the Sports_Zone group. Coordinates are world (x, z).
    static void BuildSportsDetails(Transform sports)
    {
        // Converts a world (x, z) into the field's local space, at the given height
        Vector3 At(float x, float z, float y = 1.05f)
        {
            Vector3 local = sports.InverseTransformPoint(new Vector3(x, 0, z));
            return new Vector3(local.x, y, local.z);
        }

        // Football pitch: 60 x 40, centred at (20, 30)
        Prim(PrimitiveType.Cube, "Football_Pitch", sports, At(20, 30), new Vector3(60, 0.1f, 40),
             BuildingList.Hex("27AE60"), false);

        // Basketball court: 28 x 15, at (-30, 30)
        Prim(PrimitiveType.Cube, "Basketball_Court", sports, At(-30, 30), new Vector3(28, 0.1f, 15),
             BuildingList.Hex("E67E22"), false);

        // Running track: four thin strips forming a ring around the pitch
        var track = new GameObject("Running_Track").transform;
        track.SetParent(sports, false);
        Color trackColor = BuildingList.Hex("CD6155");
        Prim(PrimitiveType.Cube, "Track_North", track, At(20, 6.5f), new Vector3(70, 0.1f, 3), trackColor, false);
        Prim(PrimitiveType.Cube, "Track_South", track, At(20, 53.5f), new Vector3(70, 0.1f, 3), trackColor, false);
        Prim(PrimitiveType.Cube, "Track_West", track, At(-13.5f, 30), new Vector3(3, 0.1f, 44), trackColor, false);
        Prim(PrimitiveType.Cube, "Track_East", track, At(53.5f, 30), new Vector3(3, 0.1f, 44), trackColor, false);

        // Badminton: four 13 x 6 courts in a 2 x 2 block centred at (-60, 50).
        // The spec places all four at one point, which would overlap, so they are spread out.
        var badminton = new GameObject("Badminton_Courts").transform;
        badminton.SetParent(sports, false);
        Color badColor = BuildingList.Hex("3498DB");
        Prim(PrimitiveType.Cube, "Court_1", badminton, At(-68, 46), new Vector3(13, 0.1f, 6), badColor, false);
        Prim(PrimitiveType.Cube, "Court_2", badminton, At(-68, 54), new Vector3(13, 0.1f, 6), badColor, false);
        Prim(PrimitiveType.Cube, "Court_3", badminton, At(-52, 46), new Vector3(13, 0.1f, 6), badColor, false);
        Prim(PrimitiveType.Cube, "Court_4", badminton, At(-52, 54), new Vector3(13, 0.1f, 6), badColor, false);
    }
    #endregion

    #region Paths
    static void BuildPaths(Transform parent)
    {
        Color c = BuildingList.Hex("D5D8DC");
        MakePath(parent, "Path_Gate_To_Admin", Pt(0, -100), Pt(0, -90), c);
        MakePath(parent, "Path_Admin_To_LC1", Pt(0, -80), Pt(-50, -80), c);
        MakePath(parent, "Path_Admin_To_LC2", Pt(0, -80), Pt(50, -80), c);
        MakePath(parent, "Path_Admin_To_Library", Pt(0, -70), Pt(0, -48), c);
        MakePath(parent, "Path_Library_To_Labs", Pt(-17, -30), Pt(-41, -30), c);
        MakePath(parent, "Path_Library_To_Kalataranga", Pt(17, -30), Pt(44, -30), c);
        MakePath(parent, "Path_Admin_To_Sports", Pt(0, -60), Pt(0, -10), c);
        MakePath(parent, "Path_Sports_To_Hostels_W", Pt(0, 70), Pt(-60, 80), c);
        MakePath(parent, "Path_Sports_To_Hostels_E", Pt(0, 70), Pt(60, 80), c);
    }

    // A thin 4 m wide box from point a to point b
    static void MakePath(Transform parent, string name, Vector3 a, Vector3 b, Color c)
    {
        Vector3 d = b - a;
        Vector3 mid = (a + b) * 0.5f;
        var p = Prim(PrimitiveType.Cube, name, parent, new Vector3(mid.x, 0.05f, mid.z),
                     new Vector3(d.magnitude, 0.1f, 4f), c, false);
        p.localRotation = Quaternion.Euler(0, -Mathf.Atan2(d.z, d.x) * Mathf.Rad2Deg, 0);
    }
    #endregion

    #region Gate and road
    static void BuildGate(Transform parent)
    {
        var gate = new GameObject("Main_Gate").transform;
        gate.SetParent(parent, false);
        Color stone = BuildingList.Hex("8B7355");

        // Pillars: radius 1, height 8. The default cylinder is 2 units tall with radius 0.5.
        Prim(PrimitiveType.Cylinder, "Pillar_Left", gate, new Vector3(-5, 4, -100), new Vector3(2, 4, 2), stone, true);
        Prim(PrimitiveType.Cylinder, "Pillar_Right", gate, new Vector3(5, 4, -100), new Vector3(2, 4, 2), stone, true);
        Prim(PrimitiveType.Cube, "Gate_Bar", gate, new Vector3(0, 8, -100), new Vector3(12, 1, 1), stone, false);

        MakeLabel(gate, "MAIN GATE", new Vector3(0, 10, -100), 10f);
    }

    static void BuildRoad(Transform parent)
    {
        Prim(PrimitiveType.Cube, "Main_Road", parent, new Vector3(0, 0.05f, -105),
             new Vector3(300, 0.1f, 10), BuildingList.Hex("555555"), false);
        MakeLabel(parent, "Chandapura-Anekal Main Road", new Vector3(0, 0.3f, -105), 30f);
    }
    #endregion

    #region Trees
    // Unity has no cone primitive, so each tree is a trunk plus a round crown.
    static void BuildTrees(Transform parent)
    {
        Vector2[] spots =
        {
            new Vector2(-40, -115), new Vector2(-20, -115), new Vector2(20, -115), new Vector2(40, -115),
            new Vector2(65, -10),   new Vector2(65, 10),    new Vector2(65, 30),   new Vector2(65, 50),
            new Vector2(-70, 10),   new Vector2(-70, 30),
            new Vector2(-80, 80),   new Vector2(-40, 95),   new Vector2(-10, 95),  new Vector2(10, 95),
            new Vector2(40, 95),    new Vector2(80, 80),
            new Vector2(-90, -30),  new Vector2(-90, -10),  new Vector2(90, -30),  new Vector2(90, -10)
        };

        var trees = new GameObject("Trees").transform;
        trees.SetParent(parent, false);

        int i = 0;
        foreach (var s in spots)
        {
            i++;
            var t = new GameObject("Tree_" + i).transform;
            t.SetParent(trees, false);
            t.position = new Vector3(s.x, 0, s.y);
            Prim(PrimitiveType.Cylinder, "Trunk", t, new Vector3(0, 1, 0), new Vector3(0.4f, 1, 0.4f),
                 BuildingList.Hex("6B4423"), false);
            Prim(PrimitiveType.Sphere, "Crown", t, new Vector3(0, 3.2f, 0), Vector3.one * 2.4f,
                 BuildingList.Hex("2E7D32"), false);
        }
    }
    #endregion

    #region NPCs
    static void BuildNpcs()
    {
        var root = new GameObject("NPCs").transform;

        // Each NPC walks its route in a loop. Routes avoid building footprints.
        var npcs = new (string name, Vector3[] route)[]
        {
            ("NPC_Librarian", new[] { N(0, -68), N(0, -50) }),
            ("NPC_Guide",     new[] { N(0, -96), N(-30, -96), N(-30, -10), N(30, -10), N(30, -96) }),
            ("NPC_Coach",     new[] { N(-65, -10), N(65, -10), N(65, 20), N(-65, 20) }),
            ("NPC_Student",   new[] { N(0, 70), N(-44, 70), N(0, 70), N(44, 70) })
        };

        foreach (var n in npcs)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = n.name;
            go.transform.SetParent(root, false);
            go.transform.position = n.route[0];
            Paint(go, BuildingList.Hex("FF9933"));

            var body = go.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.freezeRotation = true;

            var trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 3f;

            go.AddComponent<NPCInteraction>();
            var patrol = go.AddComponent<NPCPatrol>();
            patrol.waypoints = n.route;
        }
    }
    #endregion

    #region Player
    static void PlacePlayer()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("CampusSetup: no object tagged Player was found.");
            return;
        }

        // Spawn at the main gate, facing into campus (+Z)
        player.transform.position = new Vector3(0, 1, -100);
        player.transform.rotation = Quaternion.identity;

        var cam = Camera.main;
        if (cam == null) return;

        cam.orthographic = false;
        if (cam.transform.parent != player.transform) cam.transform.SetParent(player.transform, false);
        cam.transform.localPosition = new Vector3(0, 0.6f, 0);
        cam.transform.localRotation = Quaternion.identity;

        // Minimap is a child of the camera, so it follows the view
        if (cam.GetComponentInChildren<Minimap>() == null)
        {
            var mm = new GameObject("Minimap");
            mm.transform.SetParent(cam.transform, false);
            mm.AddComponent<Minimap>();
        }
    }
    #endregion

    #region Helpers
    static Vector3 Pt(float x, float z) => new Vector3(x, 0, z);
    static Vector3 N(float x, float z) => new Vector3(x, 1f, z); // NPC centre height

    // Creates a primitive with a material, and removes its collider when solid is false
    static Transform Prim(PrimitiveType type, string name, Transform parent, Vector3 localPos,
                          Vector3 scale, Color color, bool solid, float alpha = 1f)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
        Paint(go, color, alpha);
        return go.transform;
    }

    // Text label with a semi-transparent black background, always facing the camera
    static void MakeLabel(Transform parent, string text, Vector3 localPos, float bgWidth)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.font = font;
        tm.characterSize = 0.6f;
        tm.fontSize = 40;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;
        go.GetComponent<MeshRenderer>().sharedMaterial = font.material;

        go.AddComponent<LabelBillboard>();

        Prim(PrimitiveType.Quad, "Label_BG", go.transform, new Vector3(0, 0, 0.05f),
             new Vector3(bgWidth, 1.2f, 1), Color.black, false, 0.5f);
    }

    static void Paint(GameObject go, Color color, float alpha = 1f)
    {
        var r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = GetMaterial(color, alpha);
    }

    // Opaque colours use the render pipeline's Lit shader.
    // Transparent colours (alpha below 1) use Sprites/Default.
    static Material GetMaterial(Color color, float alpha)
    {
        bool transparent = alpha < 1f;
        var col = new Color(color.r, color.g, color.b, alpha);
        string key = (transparent ? "T_" : "O_") + ColorUtility.ToHtmlStringRGBA(col);

        if (matCache.TryGetValue(key, out Material cached)) return cached;

        string path = MatFolder + "/" + key + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = transparent ? Shader.Find("Sprites/Default") : PickShader();
            mat = new Material(shader);
            SetTint(mat, col);
            AssetDatabase.CreateAsset(mat, path);
        }

        matCache[key] = mat;
        return mat;
    }

    static void SetTint(Material mat, Color c)
    {
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
    }

    static Shader PickShader()
    {
        Shader s = null;
        if (GraphicsSettings.currentRenderPipeline != null)
        {
            s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Universal Render Pipeline/Simple Lit");
        }
        if (s == null) s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Legacy Shaders/Diffuse");
        return s;
    }
    #endregion
}