using UnityEngine;

public class Coin : MonoBehaviour
{

    public string coinName;
    LevelLoad levelLoad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(PlayerPrefs.GetInt(coinName) == 1)
        {
            gameObject.SetActive(false);
        }
        levelLoad = FindFirstObjectByType<LevelLoad>().GetComponent<LevelLoad>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerPrefs.SetInt(coinName,1);
            PlayerPrefs.SetInt("Coins",PlayerPrefs.GetInt("Coins")+1);
            gameObject.SetActive(false);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            levelLoad.collectableList.Add(gameObject);
            gameObject.SetActive(false);
        }
    }
}
