using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField]
    string coinPref;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(PlayerPrefs.GetInt(coinPref) == 1)
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
            PlayerPrefs.SetInt(coinPref,1);
            PlayerPrefs.SetInt("Coins",PlayerPrefs.GetInt("Coins")+1);
            gameObject.SetActive(false);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerPrefs.SetInt(coinPref,1);
            PlayerPrefs.SetInt("Coins",PlayerPrefs.GetInt("Coins")+1);
            gameObject.SetActive(false);
        }
    }
}
