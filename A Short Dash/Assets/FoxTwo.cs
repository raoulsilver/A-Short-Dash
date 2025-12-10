using UnityEngine;

public class FoxTwo : TextWindowLoader
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public override void StartText()
    {
        CheckDialogueState();
        base.StartText();
        if(lineIdToDisplay == "fox1Car")
        {
            PlayerPrefs.SetInt("fox2FirstTalked",1);
        }
    }

    void CheckDialogueState()
    {
        if (PlayerPrefs.GetInt("fox2FirstTalked") == 0)
        {
            //Debug.Log()
            lineIdToDisplay = "fox1Car";
            return;
        }
        if(PlayerPrefs.GetInt("fox2FirstTalked") == 1)
        {
            lineIdToDisplay = "fox1CarIdle";
            return;
        }
    }
}
