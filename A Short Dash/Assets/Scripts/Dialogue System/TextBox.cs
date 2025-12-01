using UnityEngine;
using TMPro;

public class TextBox : MonoBehaviour
{
    public static LoadTextManager loadTextManager;
    public bool inDialogue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TextWindowLoader.textBoxToDisplay = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(inDialogue && Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("test");
            loadTextManager.DisplayNextDialogue();
        }
    }

    public void SendText(string lineID)
    {
        //LoadTextManager.instance.SetText(textBoxToDisplay,lineIdToDisplay);
    }
}
