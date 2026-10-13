using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public int pointsOnTalk = 10;

    bool talkedBefore;   // first chat, before the Library is found
    bool talkedAfter;    // follow-up chat, after the Library is found

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
        }
        else
        {
            if (talkedAfter) return;
            talkedAfter = true;
            gm.ShowDialogue(AfterLine());
        }

        gm.AddPoints(pointsOnTalk);
    }

    string BeforeLine()
    {
        switch (gameObject.name)
        {
            case "NPC_Librarian": return "Welcome! Head to the Central Library dome to start your mission.";
            case "NPC_Guide": return "Explore the campus. Every building you visit earns points.";
            case "NPC_Coach": return "Stay active! The Sports Zone is open to all students.";
            case "NPC_Student": return "The hostels are at the far end. Visit them for the tour.";
            default: return "Welcome to campus!";
        }
    }

    string AfterLine()
    {
        switch (gameObject.name)
        {
            case "NPC_Librarian": return "Nice work finding the Library! Next, visit the Food Court.";
            case "NPC_Guide": return "Your next task: visit all the buildings for the tour bonus.";
            case "NPC_Coach": return "Great progress. Keep exploring to finish the campus tour.";
            case "NPC_Student": return "Well done! Try the Kalataranga and the Labs Complex next.";
            default: return "Good work! Keep exploring.";
        }
    }
}