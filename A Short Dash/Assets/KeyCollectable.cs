using UnityEngine;

public class KeyCollectable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public string keyName;
    LevelLoad levelLoad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (PlayerPrefs.GetInt(keyName) == 1)
        {
            gameObject.SetActive(false);
        }
        levelLoad = FindFirstObjectByType<LevelLoad>().GetComponent<LevelLoad>();
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
