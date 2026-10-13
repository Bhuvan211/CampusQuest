using UnityEngine;

// One entry per building on the Alliance layout.
// CampusSetup builds the scene from this list. GameManager and Minimap read it too.
[System.Serializable]
public class BuildingData
{
    public string objectName;    // Hierarchy name, e.g. "Central_Library"
    public string displayName;   // Label text, e.g. "Central Library"
    public string description;   // Shown when the player visits
    public Vector3 position;     // Centre of the footprint on the ground (y = 0)
    public Vector3 size;         // Width (x) x height (y) x depth (z), in metres
    public Color zoneColor;      // Colour of the shell and the ground zone
}

public static class BuildingList
{
    #region Data
    public static readonly BuildingData[] All =
    {
        new BuildingData { objectName = "Admin_Block",    displayName = "Administration Block", description = "Greek-inspired admin hub",
                           position = new Vector3(0, 0, -80),   size = new Vector3(30, 15, 20), zoneColor = Hex("8B7355") },
        new BuildingData { objectName = "Learning_Centre_1", displayName = "Learning Centre 1", description = "80+ multimedia classrooms",
                           position = new Vector3(-50, 0, -80), size = new Vector3(25, 12, 18), zoneColor = Hex("4A90D9") },
        new BuildingData { objectName = "Learning_Centre_2", displayName = "Learning Centre 2", description = "Engineering & Design block",
                           position = new Vector3(50, 0, -80),  size = new Vector3(25, 12, 18), zoneColor = Hex("4A90D9") },
        new BuildingData { objectName = "Central_Library", displayName = "Central Library", description = "Dome-shaped, 52K+ books, 1000+ seating",
                           position = new Vector3(0, 0, -30),   size = new Vector3(35, 18, 35), zoneColor = Hex("D4A017") },
        new BuildingData { objectName = "Labs_Complex",   displayName = "Labs Complex", description = "55+ labs across disciplines",
                           position = new Vector3(-55, 0, -30), size = new Vector3(28, 10, 22), zoneColor = Hex("50C878") },
        new BuildingData { objectName = "Kalataranga",    displayName = "Kalataranga", description = "Cultural & performance centre",
                           position = new Vector3(55, 0, -30),  size = new Vector3(22, 14, 18), zoneColor = Hex("E85D75") },
        new BuildingData { objectName = "Sports_Zone",    displayName = "Sports Zone", description = "18 acres – basketball, football, track",
                           position = new Vector3(0, 0, 30),    size = new Vector3(120, 1, 80), zoneColor = Hex("2ECC71") },
        new BuildingData { objectName = "Boys_Hostel",    displayName = "Boys' Hostel", description = "Residential – Boys",
                           position = new Vector3(-60, 0, 80),  size = new Vector3(20, 20, 15), zoneColor = Hex("F39C12") },
        new BuildingData { objectName = "Girls_Hostel",   displayName = "Girls' Hostel", description = "Residential – Girls",
                           position = new Vector3(0, 0, 80),    size = new Vector3(20, 20, 15), zoneColor = Hex("F39C12") },
        new BuildingData { objectName = "Food_Court",     displayName = "Food Court", description = "Multi-cuisine dining",
                           position = new Vector3(60, 0, 80),   size = new Vector3(18, 10, 14), zoneColor = Hex("9B59B6") },
        new BuildingData { objectName = "Fitness_Centre", displayName = "Fitness Centre", description = "Gym & wellness",
                           position = new Vector3(-80, 0, 50),  size = new Vector3(12, 8, 10),  zoneColor = Hex("7F8C8D") },
        new BuildingData { objectName = "Healthcare_Centre", displayName = "Healthcare Centre", description = "On-campus medical",
                           position = new Vector3(80, 0, 50),   size = new Vector3(12, 8, 10),  zoneColor = Hex("7F8C8D") },
        new BuildingData { objectName = "Moot_Court",     displayName = "Moot Court", description = "Law school facility",
                           position = new Vector3(30, 0, 50),   size = new Vector3(15, 10, 12), zoneColor = Hex("7F8C8D") },
    };
    #endregion

    public static int Count => All.Length;

    // Converts "RRGGBB" into a Color
    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }
}