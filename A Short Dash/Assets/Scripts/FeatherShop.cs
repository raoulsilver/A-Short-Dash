using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using UnityEngine;

public class FeatherShop : TextWindowLoader
{
    void Start()
    {
        CheckDialogueState();
    }
    public override void StartText()
    {
        CheckDialogueState();
        base.StartText();

        if(lineIdToDisplay=="dog1AfterFirstLevel")
        {
            PlayerPrefs.SetInt("ShopAlreadyTalkedFirst", 1);
        }
        if(lineIdToDisplay=="dog1Feather1YesMoney")
        {
            PlayerPrefs.SetInt("Coins",PlayerPrefs.GetInt("Coins")-2);
            PlayerPrefs.SetInt("Feathers",PlayerPrefs.GetInt("Feathers")+1);
        }
        if(lineIdToDisplay=="dog1Feather2YesMoney")
        {
            PlayerPrefs.SetInt("Coins",PlayerPrefs.GetInt("Coins")-1);
            PlayerPrefs.SetInt("Feathers",PlayerPrefs.GetInt("Feathers")+1);
        }
        if (lineIdToDisplay == "dog1StartHatQuest")
        {
            PlayerPrefs.SetInt("HatQuestAlreadyStarted",1);
        }
        if(lineIdToDisplay == "dog1HatYesMoney")
        {
            PlayerPrefs.SetInt("Coins",PlayerPrefs.GetInt("Coins")-5);
            PlayerPrefs.SetInt("hasHat",1);
            PlayerPrefs.SetInt("HatQuestFinished",1);
        }
    }


    void CheckDialogueState()
    {
        if (PlayerPrefs.GetInt("FinishedFirstLevel") == 0)
        {
            lineIdToDisplay = "dog1First";
            return;
        }
        if (PlayerPrefs.GetInt("FinishedFirstLevel") == 1 && PlayerPrefs.GetInt("Feathers") == 0)
        {
            if(PlayerPrefs.GetInt("ShopAlreadyTalkedFirst") == 0)
            {
                lineIdToDisplay = "dog1AfterFirstLevel";
                return;
            }
            if (PlayerPrefs.GetInt("ShopAlreadyTalkedFirst") == 1 && PlayerPrefs.GetInt("Coins")<2)
            {
                lineIdToDisplay = "dog1Feather1NoMoney";
                return;
            }
            if (PlayerPrefs.GetInt("ShopAlreadyTalkedFirst") == 1 && PlayerPrefs.GetInt("Coins")>=2)
            {
                lineIdToDisplay = "dog1Feather1YesMoney";
                return;
            }

        }
        if(PlayerPrefs.GetInt("Feathers") == 1)
        {
            if (PlayerPrefs.GetInt("Coins") == 0)
            {
                lineIdToDisplay = "dog1Feather2NoMoney";
                return;
            }
            if (PlayerPrefs.GetInt("Coins") == 1)
            {
                lineIdToDisplay = "dog1Feather2YesMoney";
                return;
            }
        }
        if (PlayerPrefs.GetInt("Feathers") == 2)
        {
            if (PlayerPrefs.GetInt("HatQuestFinished")==1)
            {
                lineIdToDisplay = "dog1AfterQuest";
                return;
            }
            if (PlayerPrefs.GetInt("HatQuestAlreadyStarted") == 0)
            {
                lineIdToDisplay = "dog1StartHatQuest";
                return;
            }
            if (PlayerPrefs.GetInt("HatQuestAlreadyStarted") == 1 && PlayerPrefs.GetInt("Coins")<5)
            {
                lineIdToDisplay = "dog1HatNoMoney";
                return;
            }
            if (PlayerPrefs.GetInt("HatQuestAlreadyStarted") == 1 && PlayerPrefs.GetInt("Coins")==5)
            {
                lineIdToDisplay = "dog1HatYesMoney";
                return;
            }
        }
        
        
    }
}
