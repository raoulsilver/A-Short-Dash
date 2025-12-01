using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using UnityEngine.Video;
using System.Data.Common;
using System.Linq;


[System.Serializable]
public class TextLine
{
    public string id;
    public string type;
    public string textToDisplay;
}


public class LoadTextManager : MonoBehaviour
{
    [SerializeField]
    TextAsset txt;
    [SerializeField]
    GameObject textBoxObj;
    TMP_Text textBoxText;
    List<string> dialogueList = new List<string>();
    PlayerInteract playerInteract;
    PlayerMovement3d playerMovementAdvanced;
    bool inDialogue;
    //string nameOfTextToLoad;
    //TMP_Text textBoxToDisplay;
    //public string lineIDToDisplay;
    public Dictionary<string,TextLine> textlines=new Dictionary<string, TextLine>();
    public static LoadTextManager instance;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {  
        instance = this;
        TextWindowLoader.loadTextManager = this;
        TextBox.loadTextManager = this;
        textBoxText = textBoxObj.GetComponentInChildren<TMP_Text>();
        playerInteract = GameObject.FindFirstObjectByType<PlayerInteract>().GetComponent<PlayerInteract>();
        playerMovementAdvanced = GameObject.FindFirstObjectByType<PlayerMovement3d>().GetComponent<PlayerMovement3d>();
        textBoxObj.SetActive(false);
        //textBoxToDisplay = gameObject.GetComponent<TMP_Text>();
        LoadDialogue();
        //SetText();
    }

    /*void SetText()
    {
        textBoxToDisplay.text = weblines[lineIDToDisplay].textToDisplay;
    }*/

    public void StartText(string lineIdToDisplay)
    {
        textBoxObj.GetComponent<TextBox>().inDialogue = true;
        playerMovementAdvanced.frozen = true;
        playerInteract.frozen = true;
        textBoxObj.SetActive(true);
        if(textlines[lineIdToDisplay].type == "oneline")
        {
            SetText(textBoxText,lineIdToDisplay);
        }
        if(textlines[lineIdToDisplay].type == "dialogue")
        {
            DialogueSetup(lineIdToDisplay);
        }
    }

    public void DialogueSetup(string lineIdToDisplay)
    {
        //Debug.Log("test");
        inDialogue = true;
        dialogueList = textlines[lineIdToDisplay].textToDisplay.Split(";").ToList();
        DisplayNextDialogue();
        textBoxObj.GetComponent<TextBox>().inDialogue = true;
    }

    public void DisplayNextDialogue()
    {
        if (inDialogue)
        {
            Debug.Log(dialogueList.Count());
            if(dialogueList.Count() == 0)
            {
                textBoxObj.GetComponent<TextBox>().inDialogue = false;
                textBoxObj.SetActive(false);
                playerMovementAdvanced.frozen = false;
                playerInteract.frozen = false;
                inDialogue = false;
                return;
            }
            if (dialogueList[0] == "o")
            {
                dialogueList.RemoveAt(0);
                textBoxText.text = dialogueList[0];
                dialogueList.RemoveAt(0);
                return;

            }
            else if(dialogueList[0] == "p")
            {
                dialogueList.RemoveAt(0);
                textBoxText.text = dialogueList[0];
                dialogueList.RemoveAt(0);
            }
        }
        else
        {
            textBoxObj.SetActive(false);
            playerMovementAdvanced.frozen = false;
            playerInteract.frozen = false;
            textBoxObj.GetComponent<TextBox>().inDialogue = false;
        }
        
    }

    public void SetText(TMP_Text text,string lineIdToDisplay)
    {
        text.text = textlines[lineIdToDisplay].textToDisplay;
    }

    private void LoadDialogue()
    {
        StringReader sr=new StringReader(txt.text);
        sr.ReadLine();
        while (true)
        {
            
            string line=sr.ReadLine();
            if(line == null)
            {
                break;
            }
            string[] data= line.Split("\t");
            if(data[0] == "")
            {
                continue;
            }
            TextLine newLine = new TextLine
            {
                id = data[0],
                type = data[1] == ""?"default":data[1],
                textToDisplay = data[2] == ""?"default":data[2]
            ,
            };
            textlines.Add(data[0],newLine);
        }
        

        
    }

}
