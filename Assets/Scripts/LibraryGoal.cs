using UnityEngine;

public class LibraryGoal : MonoBehaviour
{
    public int pointsOnReach = 50;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null) return;

        GameManager.Instance.MarkLibraryReached(pointsOnReach);
    }
}