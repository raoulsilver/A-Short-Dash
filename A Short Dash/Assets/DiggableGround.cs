using UnityEngine;

public class DiggableGround : TextWindowLoader
{
    public string digSpotName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayerPrefs.GetInt(digSpotName) == 1)
        {
            gameObject.SetActive(false);
        }
    }

    public override void StartText()
    {
        Debug.Log("test");
        if (PlayerPrefs.GetInt("hasShovel")== 1)
        {
            lineIdToDisplay = "digText";
            base.StartText();
            PlayerPrefs.SetInt(digSpotName,1);
            PlayerPrefs.SetInt("Coins",PlayerPrefs.GetInt("Coins")+1);
            PlayerInteract.instance.canInteract=false;
            gameObject.SetActive(false);

        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
