using UnityEngine;

public class BirdTwo : TextWindowLoader
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CheckDialogueState();
    }
    public override void StartText()
    {
        CheckDialogueState();
        base.StartText();
        if(lineIdToDisplay == "bird2Tutorial")
        {
            PlayerPrefs.SetInt("bird2FirstTalked",1);
        }
    }

    void CheckDialogueState()
    {
        if (PlayerPrefs.GetInt("bird2FirstTalked") == 0)
        {
            lineIdToDisplay = "bird2Tutorial";
            return;
        }
        if(PlayerPrefs.GetInt("bird2FirstTalked") == 1)
        {
            lineIdToDisplay = "bird2Idle";
            return;
        }
    }
}
