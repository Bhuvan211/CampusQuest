using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region References
    public static GameManager Instance;
    public Text pointsText, missionText, dialogueText;
    #endregion

    #region Settings
    public int tourBonus = 100;
    #endregion

    #region State
    int points;
    int buildingsVisited;
    int totalBuildings;
    bool tourComplete;
    public bool LibraryReached { get; private set; }
    #endregion

    void Awake()
    {
        Instance = this;
        totalBuildings = BuildingList.Count;   // every building on the list counts toward the tour
        UpdateUI();
        UpdateMission();
    }

    #region Points and dialogue
    public void AddPoints(int p) { points += p; UpdateUI(); }

    public void ShowDialogue(string msg)
    {
        dialogueText.text = msg;
        dialogueText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideDialogue));
        Invoke(nameof(HideDialogue), 4f);
    }

    void HideDialogue() { dialogueText.gameObject.SetActive(false); }
    #endregion

    #region Mission
    public void SetMission(string text)
    {
        if (missionText != null) missionText.text = text;
    }

    // Called by LibraryGoal when the player enters the library zone
    public void MarkLibraryReached(int bonus)
    {
        if (LibraryReached) return;

        LibraryReached = true;
        AddPoints(bonus);
        ShowDialogue("You found the Library! Mission complete. Talk to the NPCs for your next task.");
        UpdateMission();
    }

    // Called by BuildingVisit when the player enters a new building
    public void RegisterBuildingVisit(string buildingName)
    {
        if (tourComplete) return;

        buildingsVisited++;

        if (buildingsVisited >= totalBuildings)
        {
            tourComplete = true;
            AddPoints(tourBonus);
            ShowDialogue("Campus tour complete! Bonus +" + tourBonus + " points.");
        }

        UpdateMission();
    }

    void UpdateMission()
    {
        string goal = LibraryReached
            ? "Mission: Library found! Talk to an NPC for your next task."
            : "Mission: Find the Library";

        string tour = tourComplete
            ? "Campus tour complete!"
            : "Explore buildings: " + buildingsVisited + "/" + totalBuildings;

        SetMission(goal + "\n" + tour);
    }
    #endregion

    void UpdateUI() { pointsText.text = "Points: " + points; }
}