using UnityEngine;

public class BookCollectable : MonoBehaviour
{
    public string bookName;
    LevelLoad levelLoad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (PlayerPrefs.GetInt(bookName) == 1)
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
    // Update is called once per frame
    void Update()
    {
        
    }
}
