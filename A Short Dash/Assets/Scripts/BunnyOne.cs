using UnityEngine;

public class BunnyOne : TextWindowLoader
{
    void Start()
    {
        CheckDialogueState();
    }

    public override void StartText()
    {
        CheckDialogueState();
        base.StartText();
        if(lineIdToDisplay =="bunny1Tutorial")
        {
            PlayerPrefs.SetInt("bunny1FirstTalked",1);
        }
    }


    void CheckDialogueState()
    {
        if (PlayerPrefs.GetInt("bunny1FirstTalked") == 0)
        {
            lineIdToDisplay = "bunny1Tutorial";
            return;
        }
        if(PlayerPrefs.GetInt("bunny1FirstTalked") == 1)
        {
            lineIdToDisplay = "bunny1Idle";
            return;
        }
    }
}
