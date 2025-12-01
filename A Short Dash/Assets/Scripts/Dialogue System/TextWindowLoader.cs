using UnityEngine;
using TMPro;

public class TextWindowLoader : MonoBehaviour
{
    public static LoadTextManager loadTextManager;
    [SerializeField]
    public static TextBox textBoxToDisplay;
    [SerializeField]
    public string lineIdToDisplay;

    void Start()
    {
        //LoadTextManager.instance.SetText(textBoxToDisplay,lineIdToDisplay);
        //textBoxToDisplay.SendText(lineIdToDisplay);
    }


    public virtual void StartText()
    {
        loadTextManager.StartText(lineIdToDisplay);
    }

    /*void SetText()
    {
        
    }*/
}
