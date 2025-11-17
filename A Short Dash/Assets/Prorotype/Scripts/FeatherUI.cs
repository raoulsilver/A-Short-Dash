using UnityEngine;
using UnityEngine.UI;

public class FeatherUI : MonoBehaviour
{
    public Image[] feathers;

    public void UpdateFeathers(int current, int max)
    {
        for (int i = 0; i < feathers.Length; i++)
        {
            feathers[i].enabled = i < current;
        }
    }
}