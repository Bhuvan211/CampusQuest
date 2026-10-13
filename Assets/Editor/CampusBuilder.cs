using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public static class CampusBuilder
{
    const string ScenePath = "Assets/Scenes/CampusOrientation.unity";
    const string MatFolder = "Assets/Materials/Campus";
    static readonly Dictionary<string, Material> matCache = new Dictionary<string, Material>();

    [MenuItem("Campus/Build Full Scene")]
    public static void BuildScene()
    {
        matCache.Clear();
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        System.IO.Directory.CreateDirectory(MatFolder);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(5, 1, 5);
        SetColor(ground, new Color(0.25f, 0.6f, 0.25f), "Ground");

        // Light
        var light = Object.FindAnyObjectByType<Light>();
        if (light != null)
        {
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        // Buildings: name, position, and the message shown when the player visits
        var buildings = new List<(string name, Vector3 pos, string msg)>
        {
            ("Admin_Block",    new Vector3(-20, 0, 20),  "Admin Block: get your student ID and fee details here."),
            ("Library",        new Vector3(0, 0, 20),    "Library: find study spaces and books here."),
            ("Academic_Block", new Vector3(20, 0, 20),   "Academic Block: your classrooms are in this building."),
            ("Food_Court",     new Vector3(-20, 0, -10), "Food Court: grab lunch and snacks here."),
            ("Sports_Complex", new Vector3(0, 0, -10),   "Sports Complex: gym and grounds for all students."),
            ("Hostel",         new Vector3(20, 0, -10),  "Hostel: student accommodation and wardens' office.")
        };

        var root = new GameObject("Campus");
        foreach (var b in buildings)
        {
            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = b.name;
            building.transform.SetParent(root.transform);
            building.transform.position = b.pos + Vector3.up * 3;
            building.transform.localScale = new Vector3(8, 6, 8);
            SetColor(building, new Color(0.8f, 0.75f, 0.65f), "Building");

            // Invisible trigger zone around the building, so the visit is detected before the player bumps into the wall
            var zone = new GameObject("Zone_" + b.name);
            zone.transform.SetParent(root.transform);
            zone.transform.position = b.pos + Vector3.up * 3;
            var zoneBox = zone.AddComponent<BoxCollider>();
            zoneBox.isTrigger = true;
            zoneBox.size = new Vector3(12, 6, 12);
            var visit = zone.AddComponent<BuildingVisit>();
            visit.buildingName = b.name.Replace("_", " ");
            visit.message = b.msg;
            visit.points = 20;
        }

        // Library dome (walk into it to complete the mission)
        var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dome.name = "Library_Dome";
        dome.transform.SetParent(root.transform);
        dome.transform.position = new Vector3(0, 6.5f, 20);
        dome.transform.localScale = new Vector3(6, 3, 6);
        SetColor(dome, new Color(0.6f, 0.7f, 0.9f), "Dome");
        var goal = dome.AddComponent<SphereCollider>();
        goal.isTrigger = true;
        goal.radius = 1.2f;
        dome.AddComponent<LibraryGoal>();

        // Pathway
        var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
        path.name = "Pathway_Main";
        path.transform.SetParent(root.transform);
        path.transform.position = new Vector3(0, 0.05f, 5);
        path.transform.localScale = new Vector3(50, 0.1f, 4);
        SetColor(path, new Color(0.5f, 0.5f, 0.5f), "Path");

        // NPCs: each walks between pointA and pointB, and gives dialogue on contact
        var npcs = new List<(string name, Vector3 a, Vector3 b)>
        {
            ("NPC_Librarian", new Vector3(-4, 1, 12),  new Vector3(4, 1, 12)),
            ("NPC_Guide",     new Vector3(10, 1, 5),   new Vector3(10, 1, -2)),
            ("NPC_Coach",     new Vector3(-10, 1, -2), new Vector3(6, 1, -2))
        };
        var npcRoot = new GameObject("NPCs");
        foreach (var n in npcs)
        {
            var npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = n.name;
            npc.transform.SetParent(npcRoot.transform);
            npc.transform.position = n.a;
            SetColor(npc, new Color(1f, 0.6f, 0.2f), "NPC");

            var npcBody = npc.AddComponent<Rigidbody>();
            npcBody.isKinematic = true;
            npcBody.useGravity = false;
            npcBody.freezeRotation = true;

            var trigger = npc.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 3f;
            npc.AddComponent<NPCInteraction>();

            var patrol = npc.AddComponent<NPCPatrol>();
            patrol.pointA = n.a;
            patrol.pointB = n.b;
        }

        // Player
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0, 1, 0);
        var playerRenderer = player.GetComponent<Renderer>();
        if (playerRenderer != null) playerRenderer.enabled = false; // hide body in first person
        var rb = player.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        player.AddComponent<PlayerController>();
        player.AddComponent<CameraLook>();

        // Camera (first person, attached to player)
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.SetParent(player.transform);
            cam.transform.localPosition = new Vector3(0, 0.6f, 0);
            cam.transform.localRotation = Quaternion.identity;
        }

        // Game manager + UI
        var gm = new GameObject("GameManager");
        var gmScript = gm.AddComponent<GameManager>();
        CreateUI(gmScript);

        // Save
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(ScenePath);
        Debug.Log("Campus scene built. Press Play.");
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

        Debug.Log("CampusBuilder using shader: " + (s != null ? s.name : "NONE") +
                  " | Render pipeline asset assigned: " + hasSRP);
        return s;
    }

    static void SetColor(GameObject go, Color c, string key)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;

        string cacheKey = key + "_" + ColorUtility.ToHtmlStringRGB(c);
        if (!matCache.TryGetValue(cacheKey, out Material mat))
        {
            Shader shader = PickShader();
            mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);

            string assetPath = MatFolder + "/" + cacheKey + ".mat";
            AssetDatabase.CreateAsset(mat, assetPath);
            matCache[cacheKey] = mat;
        }

        r.sharedMaterial = mat;
    }

    static void CreateUI(GameManager gm)
    {
        // Canvas: scales with screen size so it reads well on phones
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // EventSystem: required for any UI input (joystick, mouse, touch)
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var pointsText = CreateText(canvasGO.transform, "PointsText", "Points: 0", new Vector2(20, -20));
        var missionText = CreateText(canvasGO.transform, "MissionText", "Mission: Find the Library", new Vector2(20, -60));
        var dialogueText = CreateText(canvasGO.transform, "DialogueText", "", new Vector2(20, -200));
        dialogueText.gameObject.SetActive(false);

        CreateJoystick(canvasGO.transform);

        gm.pointsText = pointsText;
        gm.missionText = missionText;
        gm.dialogueText = dialogueText;
    }

    // Bottom-left virtual joystick. Works with touch on phones and the mouse on PC.
    static void CreateJoystick(Transform canvasT)
    {
        var bg = new GameObject("Joystick", typeof(Image), typeof(VirtualJoystick));
        bg.transform.SetParent(canvasT, false);
        bg.GetComponent<Image>().color = new Color(1, 1, 1, 0.25f);
        var rt = bg.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(200, 200);
        rt.anchoredPosition = new Vector2(160, 160);

        var knob = new GameObject("Knob", typeof(Image));
        knob.transform.SetParent(bg.transform, false);
        knob.GetComponent<Image>().color = new Color(1, 1, 1, 0.6f);
        var kr = knob.GetComponent<RectTransform>();
        kr.anchorMin = kr.anchorMax = new Vector2(0.5f, 0.5f);
        kr.pivot = new Vector2(0.5f, 0.5f);
        kr.sizeDelta = new Vector2(80, 80);
        kr.anchoredPosition = Vector2.zero;

        bg.GetComponent<VirtualJoystick>().knob = kr;
    }

    static Text CreateText(Transform parent, string name, string value, Vector2 pos)
    {
        var go = new GameObject(name, typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.text = value;
        t.fontSize = 24;
        t.color = Color.white;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(600, 40);
        return t;
    }
}