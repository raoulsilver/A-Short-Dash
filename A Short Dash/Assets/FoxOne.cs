using UnityEngine;

public class FoxOne : TextWindowLoader
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void StartText()
    {
        CheckDialogueState();
        base.StartText();
        if(lineIdToDisplay == "fox2KeyGiveQuest")
        {
            PlayerPrefs.SetInt("fox1FirstTalked",1);
        }
        if(lineIdToDisplay == "fox2YesKey")
        {
            PlayerPrefs.SetInt("fox1QuestFinished",1);
            PlayerPrefs.SetInt("hasKey",0);
            PlayerPrefs.SetInt("hasChips",1);
        }
    }

    void CheckDialogueState()
    {
        if (PlayerPrefs.GetInt("fox1QuestFinished") == 1)
        {
            lineIdToDisplay="fox2AfterQuest";
            return;
        }
        if (PlayerPrefs.GetInt("fox1FirstTalked") == 0)
        {
            lineIdToDisplay="fox2KeyGiveQuest";
            return;
        }
        if (PlayerPrefs.GetInt("fox1FirstTalked") == 1 && PlayerPrefs.GetInt("hasKey")==0)
        {
            lineIdToDisplay="fox2NoKey";
            return;
        }
        if (PlayerPrefs.GetInt("fox1FirstTalked") == 1 && PlayerPrefs.GetInt("hasKey")==0)
        {
            lineIdToDisplay="fox2YesKey";
            return;
        }
    }
}
