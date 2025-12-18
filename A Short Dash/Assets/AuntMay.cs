using UnityEngine;

public class AuntMay : TextWindowLoader
{

    void Start()
    {
        CheckDialogueState();
    }

    void Update()
    {
        
    }
    public override void StartText()
    {
        CheckDialogueState();
        base.StartText();
        if(lineIdToDisplay == "auntMayEnding")
        {
            PlayerPrefs.SetInt("AuntMayFinishedSecondLevelYet",1);
        }
        if (lineIdToDisplay == "auntMayIntro")
        {
            PlayerPrefs.SetInt("auntMayFirstTalked",1);
        }
        if (lineIdToDisplay == "auntMayGiveQuest")
        {
            PlayerPrefs.SetInt("auntMayQuestGiven",1);
        }
        if (lineIdToDisplay == "auntMayQuestFinish")
        {
            PlayerPrefs.SetInt("auntMayQuestFinished",1);
            PlayerPrefs.SetInt("gotChips",0);
        }
    }

    void CheckDialogueState()
    {
        if(PlayerPrefs.GetInt("FinishedSecondLevel") == 1 && PlayerPrefs.GetInt("AuntMayFinishedSecondLevelYet") == 0)
        {
            lineIdToDisplay = "auntMayEnding";
            return;
        }
        if(PlayerPrefs.GetInt("auntMayQuestFinished")==1)
        {
            lineIdToDisplay = "auntMayAfterQuest";
            return;
        }
        if(PlayerPrefs.GetInt("auntMayFirstTalked") == 0)
        {
            lineIdToDisplay = "auntMayIntro";
            return;
        }
        if(PlayerPrefs.GetInt("auntMayFirstTalked")==1 && PlayerPrefs.GetInt("auntMayQuestGiven") == 0)
        {
            lineIdToDisplay = "auntMayGiveQuest";
            return;
        }
        if(PlayerPrefs.GetInt("auntMayQuestGiven")==1 && PlayerPrefs.GetInt("gotChips") == 0)
        {
            lineIdToDisplay = "auntMayQuestIdle";
            return;
        }
        if(PlayerPrefs.GetInt("auntMayQuestGiven")==1 && PlayerPrefs.GetInt("gotChips") == 1)
        {
            lineIdToDisplay = "auntMayQuestFinish";
            return;
        }
        
         
    }

}
