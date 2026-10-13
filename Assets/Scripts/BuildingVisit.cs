using UnityEngine;

public class BuildingVisit : MonoBehaviour
{
    public string buildingName = "Building";
    public string message = "";
    public int points = 20;
    bool visited;

    void OnTriggerEnter(Collider other)
    {
        if (visited || !other.CompareTag("Player")) return;
        visited = true;

        GameManager.Instance.AddPoints(points);
        GameManager.Instance.ShowDialogue(buildingName + ": " + message + " (+" + points + ")");
        GameManager.Instance.RegisterBuildingVisit(buildingName);
    }
}