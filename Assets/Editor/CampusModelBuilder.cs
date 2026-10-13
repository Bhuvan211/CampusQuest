using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class CampusModelBuilder
{
    const string MatFolder = "Assets/Materials/Campus";
    static readonly Dictionary<string, Material> matCache = new Dictionary<string, Material>();

    static readonly string[] BuildingNames =
    {
        "Admin_Block", "Library", "Academic_Block", "Food_Court", "Sports_Complex", "Hostel"
    };

    [MenuItem("Campus/Add 3D Details")]
    public static void AddDetails()
    {
        var campus = GameObject.Find("Campus");
        if (campus == null)
        {
            Debug.LogError("Campus not found. Run Campus → Build Full Scene first.");
            return;
        }

        matCache.Clear();
        System.IO.Directory.CreateDirectory(MatFolder);

        // Remove previous details so this can be run again
        var oldDecor = GameObject.Find("Decor");
        if (oldDecor != null) Object.DestroyImmediate(oldDecor);

        // Restore placeholder renderers before rebuilding
        foreach (var name in BuildingNames)
        {
            var shell = campus.transform.Find(name);
            if (shell != null)
            {
                var r = shell.GetComponent<Renderer>();
                if (r != null) r.enabled = true;
            }
        }

        var decor = new GameObject("Decor").transform;

        foreach (var name in BuildingNames)
        {
            var shell = campus.transform.Find(name);
            if (shell != null) BuildBuilding(decor, shell, name);
        }

        AddTrees(decor);
        AddLamps(decor);
        AddBenches(decor);

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("3D details added. Press Play.");
    }

    static void BuildBuilding(Transform decor, Transform shell, string name)
    {
        // Hide the placeholder cube visually. Its collider stays, so the player still can't walk through.
        var shellRenderer = shell.GetComponent<Renderer>();
        if (shellRenderer != null) shellRenderer.enabled = false;

        Color wall = name == "Library" ? new Color(0.9f, 0.88f, 0.8f) : new Color(0.8f, 0.75f, 0.65f);
        Color roof = new Color(0.45f, 0.3f, 0.25f);
        Color window = new Color(0.25f, 0.4f, 0.55f);
        Color door = new Color(0.35f, 0.2f, 0.1f);

        var root = new GameObject(name + "_Model").transform;
        root.SetParent(decor, false);
        root.position = shell.position;

        // Walls (8 x 6 x 8, same size as the placeholder)
        Box("Walls", root, Vector3.zero, new Vector3(8, 6, 8), wall);

        // Roof with a slight overhang
        Box("Roof", root, new Vector3(0, 3.25f, 0), new Vector3(8.8f, 0.5f, 8.8f), roof);

        // Windows on all four sides. The front-center bottom spot is left for the door.
        for (int row = 0; row < 2; row++)
        {
            float y = row == 0 ? -1.2f : 1.2f;
            foreach (float u in new[] { -2.5f, 0f, 2.5f })
            {
                bool doorSpot = (row == 0 && u == 0f);
                if (!doorSpot) Box("Win_Front", root, new Vector3(u, y, -4.05f), new Vector3(1.2f, 1.2f, 0.1f), window);
                Box("Win_Back", root, new Vector3(u, y, 4.05f), new Vector3(1.2f, 1.2f, 0.1f), window);
                Box("Win_Left", root, new Vector3(-4.05f, y, u), new Vector3(0.1f, 1.2f, 1.2f), window);
                Box("Win_Right", root, new Vector3(4.05f, y, u), new Vector3(0.1f, 1.2f, 1.2f), window);
            }
        }

        // Door on the front face (negative Z)
        Box("Door", root, new Vector3(0, -2.1f, -4.07f), new Vector3(1.4f, 1.8f, 0.1f), door);

        // Name sign above the door
        var sign = new GameObject("Sign").transform;
        sign.SetParent(root, false);
        sign.localPosition = new Vector3(0, 2.3f, -4.1f);
        var tm = sign.gameObject.AddComponent<TextMesh>();
        tm.text = name.Replace("_", " ");
        tm.characterSize = 0.35f;
        tm.fontSize = 48;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.black;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        sign.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;

        // Library gets a few extra columns at the entrance
        if (name == "Library")
        {
            Box("Column_L", root, new Vector3(-1.8f, -1.5f, -4.3f), new Vector3(0.3f, 3f, 0.3f), wall);
            Box("Column_R", root, new Vector3(1.8f, -1.5f, -4.3f), new Vector3(0.3f, 3f, 0.3f), wall);
        }
    }

    static void AddTrees(Transform decor)
    {
        Vector3[] spots =
        {
            new Vector3(-23, 0, 0), new Vector3(23, 0, 2), new Vector3(-22, 0, -22),
            new Vector3(22, 0, -22), new Vector3(-6, 0, -20), new Vector3(6, 0, -20),
            new Vector3(-23, 0, 12), new Vector3(0, 0, -23)
        };

        foreach (var p in spots)
        {
            var t = new GameObject("Tree").transform;
            t.SetParent(decor, false);
            t.position = p;
            Prim(PrimitiveType.Cylinder, "Trunk", t, new Vector3(0, 1f, 0), new Vector3(0.4f, 1f, 0.4f), new Color(0.4f, 0.26f, 0.13f));
            Prim(PrimitiveType.Sphere, "Leaves", t, new Vector3(0, 3.2f, 0), new Vector3(2.4f, 2.4f, 2.4f), new Color(0.2f, 0.5f, 0.2f));
        }
    }

    static void AddLamps(Transform decor)
    {
        Vector3[] spots = { new Vector3(-16, 0, 9.5f), new Vector3(0, 0, 9.5f), new Vector3(16, 0, 9.5f) };
        foreach (var p in spots)
        {
            var t = new GameObject("Lamp").transform;
            t.SetParent(decor, false);
            t.position = p;
            Prim(PrimitiveType.Cylinder, "Pole", t, new Vector3(0, 1.8f, 0), new Vector3(0.15f, 1.8f, 0.15f), new Color(0.2f, 0.2f, 0.2f));
            Prim(PrimitiveType.Sphere, "Light", t, new Vector3(0, 3.7f, 0), new Vector3(0.6f, 0.6f, 0.6f), new Color(1f, 0.95f, 0.6f));
        }
    }

    static void AddBenches(Transform decor)
    {
        Vector3[] spots = { new Vector3(-8, 0, 9.5f), new Vector3(8, 0, 9.5f) };
        Color wood = new Color(0.55f, 0.35f, 0.2f);
        foreach (var p in spots)
        {
            var t = new GameObject("Bench").transform;
            t.SetParent(decor, false);
            t.position = p;
            Prim(PrimitiveType.Cube, "Seat", t, new Vector3(0, 0.45f, 0), new Vector3(2f, 0.15f, 0.6f), wood);
            Prim(PrimitiveType.Cube, "Back", t, new Vector3(0, 0.8f, -0.27f), new Vector3(2f, 0.6f, 0.1f), wood);
        }
    }

    static GameObject Box(string name, Transform parent, Vector3 localPos, Vector3 scale, Color c)
    {
        return Prim(PrimitiveType.Cube, name, parent, localPos, scale, c);
    }

    static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 scale, Color c)
    {
        var g = GameObject.CreatePrimitive(type);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = localPos;
        g.transform.localScale = scale;
        Object.DestroyImmediate(g.GetComponent<Collider>()); // collision comes from the placeholder cubes
        SetColor(g, c);
        return g;
    }

    static Shader PickShader()
    {
        bool hasSRP = GraphicsSettings.currentRenderPipeline != null;
        Shader s = null;
        if (hasSRP)
        {
            s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Universal Render Pipeline/Simple Lit");
        }
        if (s == null) s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Legacy Shaders/Diffuse");
        return s;
    }

    static void SetColor(GameObject go, Color c)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;

        string key = ColorUtility.ToHtmlStringRGB(c);
        if (!matCache.TryGetValue(key, out Material mat))
        {
            string path = MatFolder + "/Decor_" + key + ".mat";
            mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(PickShader());
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
                AssetDatabase.CreateAsset(mat, path);
            }
            matCache[key] = mat;
        }

        r.sharedMaterial = mat;
    }
}