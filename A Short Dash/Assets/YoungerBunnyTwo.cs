using Unity.VisualScripting;
using UnityEngine;

public class YoungerBunnyTwo : TextWindowLoader
{
    [SerializeField]
    GameObject shovelWorldModel;

    void Start()
    {
        CheckDialogueState();
        if (PlayerPrefs.GetInt("hasShovel") == 1)
        {
            shovelWorldModel.SetActive(false);
        }
    }
    public override void StartText()
    {
        CheckDialogueState();
        base.StartText();
        if(lineIdToDisplay == "youngerBunny2First")
        {
            PlayerPrefs.SetInt("yBunny2FirstTalked",1);
        }
        if(lineIdToDisplay == "youngerBunny2GiveCrown")
        {
            PlayerPrefs.SetInt("yBunny2QuestFinished",1);
            PlayerPrefs.SetInt("hasFlowerCrown",0);
            PlayerPrefs.SetInt("hasShovel",1);
            shovelWorldModel.SetActive(false);
        }
    }

    void CheckDialogueState()
    {
        if(PlayerPrefs.GetInt("yBunny2FirstTalked") == 0)
        {
            lineIdToDisplay = "youngerBunny2First";
            return;
        }
        if(PlayerPrefs.GetInt("yBunny2FirstTalked")== 1 && PlayerPrefs.GetInt("yBunny2QuestFinished")== 0 && PlayerPrefs.GetInt("hasFlowerCrown")== 0)
        {
            lineIdToDisplay = "youngerBunny2Second";
            return;
        }
        if(PlayerPrefs.GetInt("yBunny2FirstTalked")== 1 && PlayerPrefs.GetInt("yBunny2QuestFinished")== 0 && PlayerPrefs.GetInt("hasFlowerCrown")== 1)
        {
            lineIdToDisplay = "youngerBunny2GiveCrown";
            return;
        }
        if(PlayerPrefs.GetInt("yBunny2FirstTalked")== 1 && PlayerPrefs.GetInt("yBunny2QuestFinished")== 1)
        {
            lineIdToDisplay = "youngerBunny2AfterCrown";
            return;
        }
    }
}
