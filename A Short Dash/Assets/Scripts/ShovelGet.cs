using UnityEngine;

public class ShovelGet : TextWindowLoader
{
    public override void StartText()
    {
        if (VariableManager.instance.CheckVariable("finishedShovelQuest") == 1)
        {
            base.StartText();
            VariableManager.instance.UpdateVariable("hasShovel",1);
        }
        

    }
}
