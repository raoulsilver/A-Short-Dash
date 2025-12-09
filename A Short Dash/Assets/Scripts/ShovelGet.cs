using UnityEngine;

public class ShovelGet : TextWindowLoader
{
    public override void StartText()
    {
        if (PlayerPrefs.GetInt("finishedShovelQuest") == 1)
        {
            base.StartText();
            PlayerPrefs.SetInt("hasShovel",1);
        }
    }
}
