using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Mono.Cecil.Cil;
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

        if(PlayerPrefs.GetInt("FinishedFirstLevel") == 1 && PlayerPrefs.GetInt("ShopAlreadyTalkedFirst") == 0)
        {
            PlayerPrefs.SetInt("ShopAlreadyTalkedFirst", 1);
        }
        if(PlayerPrefs.GetInt("FinishedFirstLevel") == 1 && PlayerPrefs.GetInt("ShopAlreadyTalkedFirst") == 1 && PlayerPrefs.GetInt("Coins") >= 2)
        {
            PlayerPrefs.SetInt("Coins",PlayerPrefs.GetInt("Coins")-2);
            PlayerPrefs.SetInt("Feathers",PlayerPrefs.GetInt("Feathers")+1);
            PlayerPrefs.SetInt("AlreadyBoughtFeather", 1);
        }
    }


    void CheckDialogueState()
    {
        if (PlayerPrefs.GetInt("FinishedFirstLevel") == 0)
        {
            lineIdToDisplay = "shopNotFinishedFirstLevel";
            return;
        }
        if(PlayerPrefs.GetInt("FinishedFirstLevel") == 1 && PlayerPrefs.GetInt("ShopAlreadyTalkedFirst") == 0)
        {
            lineIdToDisplay = "shopFinishedFirstLevel";
            return;
        }
        if(PlayerPrefs.GetInt("AlreadyBoughtFeather") == 1)
        {
            lineIdToDisplay = "shopAlreadyBought";
            return;
        }
        if(PlayerPrefs.GetInt("FinishedFirstLevel") == 1 && PlayerPrefs.GetInt("ShopAlreadyTalkedFirst") == 1 && PlayerPrefs.GetInt("Coins") <2)
        {
            lineIdToDisplay = "shopNotEnoughCoins";
            return;
        }
        if(PlayerPrefs.GetInt("FinishedFirstLevel") == 1 && PlayerPrefs.GetInt("ShopAlreadyTalkedFirst") == 1 && PlayerPrefs.GetInt("Coins") >=2)
        {
            lineIdToDisplay = "shopBuyFeather";
            return;
        }
        
    }
}
