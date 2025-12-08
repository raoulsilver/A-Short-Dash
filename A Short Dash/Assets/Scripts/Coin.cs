using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField]
    string coinName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(VariableManager.instance.CheckVariable(coinName) == 1)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            VariableManager.instance.UpdateVariable(coinName,1);
            VariableManager.instance.UpdateVariable("Coins",VariableManager.instance.CheckVariable("Coins")+1);
            gameObject.SetActive(false);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            VariableManager.instance.UpdateVariable(coinName,1);
            VariableManager.instance.UpdateVariable("Coins",VariableManager.instance.CheckVariable("Coins")+1);
            gameObject.SetActive(false);
        }
    }
}
