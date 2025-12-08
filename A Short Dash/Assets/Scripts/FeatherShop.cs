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

        if(VariableManager.instance.CheckVariable("FinishedFirstLevel") == 1 && VariableManager.instance.CheckVariable("ShopAlreadyTalkedFirst") == 0)
        {
            VariableManager.instance.UpdateVariable("ShopAlreadyTalkedFirst", 1);
        }
        if(VariableManager.instance.CheckVariable("FinishedFirstLevel") == 1 && VariableManager.instance.CheckVariable("ShopAlreadyTalkedFirst") == 1 && VariableManager.instance.CheckVariable("Coins") >= 2)
        {
            VariableManager.instance.UpdateVariable("Coins",VariableManager.instance.CheckVariable("Coins")-2);
            VariableManager.instance.UpdateVariable("Feathers",VariableManager.instance.CheckVariable("Feathers")+1);
            VariableManager.instance.UpdateVariable("AlreadyBoughtFeather", 1);
        }
    }


    void CheckDialogueState()
    {
        if (VariableManager.instance.CheckVariable("FinishedFirstLevel")==0)
        {
            lineIdToDisplay = "shopNotFinishedFirstLevel";
            return;
        }
        if(VariableManager.instance.CheckVariable("FinishedFirstLevel") == 1 && VariableManager.instance.CheckVariable("ShopAlreadyTalkedFirst") == 0)
        {
            lineIdToDisplay = "shopFinishedFirstLevel";
            return;
        }
        if(VariableManager.instance.CheckVariable("AlreadyBoughtFeather") == 1)
        {
            lineIdToDisplay = "shopAlreadyBought";
            return;
        }
        if(VariableManager.instance.CheckVariable("FinishedFirstLevel") == 1 && VariableManager.instance.CheckVariable("ShopAlreadyTalkedFirst") == 1 && VariableManager.instance.CheckVariable("Coins") <2)
        {
            lineIdToDisplay = "shopNotEnoughCoins";
            return;
        }
        if(VariableManager.instance.CheckVariable("FinishedFirstLevel") == 1 && VariableManager.instance.CheckVariable("ShopAlreadyTalkedFirst") == 1 && VariableManager.instance.CheckVariable("Coins") >=2)
        {
            lineIdToDisplay = "shopBuyFeather";
            return;
        }
        
    }
}
