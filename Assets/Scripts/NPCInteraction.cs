using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public int pointsOnTalk = 10;

    bool talkedBefore;   // first conversation, before the Library is found
    bool talkedAfter;    // follow-up conversation, after the Library is found

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (!gm.LibraryReached)
        {
            if (talkedBefore) return;
            talkedBefore = true;
            gm.ShowDialogue(BeforeLine());
            gm.AddPoints(pointsOnTalk);
        }
        else
        {
            if (talkedAfter) return;
            talkedAfter = true;
            gm.ShowDialogue(AfterLine());
            gm.AddPoints(pointsOnTalk);
        }
    }

    string BeforeLine()
    {
        switch (gameObject.name)
        {
            case "NPC_Librarian": return "Welcome! Head to the Library dome to start your mission.";
            case "NPC_Guide": return "Explore the campus. Every building you visit earns points.";
            case "NPC_Coach": return "Stay active! The Sports Complex is open to all students.";
            default: return "Welcome to campus!";
        }
    }

    string AfterLine()
    {
        switch (gameObject.name)
        {
            case "NPC_Librarian": return "Nice work finding the Library! Next, visit the Food Court.";
            case "NPC_Guide": return "Your next task: visit all six buildings for the tour bonus.";
            case "NPC_Coach": return "Great progress. Keep exploring to finish the campus tour.";
            default: return "Good work! Keep exploring.";
        }
    }
}