using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoad : MonoBehaviour
{
    [SerializeField] private AudioClip levelCompleteClip;
    [SerializeField, Range(0f,1f)] private float levelCompleteVolume = 0.9f;
    private AudioSource levelCompleteSource;

    [SerializeField]
    string sceneToLoadNext;

    [SerializeField]
    GameObject canvas;
    [SerializeField]
    GameObject itemGotBox;
    [SerializeField]
    TMP_Text objectText;
    [SerializeField]
    PlayerMovement2d playerMovement2D;
    [SerializeField]
    string levelName;

    bool ending = false;
    //public Dictionary<string,string> levelObjects=new Dictionary<string, string>();
    public List<GameObject> collectableList= new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collectableList.Clear();
        canvas.SetActive(false);
        itemGotBox.SetActive(false);
        levelCompleteSource = gameObject.AddComponent<AudioSource>();
        levelCompleteSource.loop = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(ending == true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("Mountain Base Scene");
            }
        }
    }

    void UpdateCollectables()
    {
        foreach(GameObject obj in collectableList){
            if (obj.GetComponent<Coin>())
            {
                PlayerPrefs.SetInt("Coins",PlayerPrefs.GetInt("Coins")+1);
                PlayerPrefs.SetInt(obj.GetComponent<Coin>().coinName,1);
                objectText.text = objectText.text+("\n")+("Coin");
            }
            if (obj.GetComponent<BookCollectable>())
            {
                PlayerPrefs.SetInt("bookCount",PlayerPrefs.GetInt("bookCount")+1);
                PlayerPrefs.SetInt(obj.GetComponent<BookCollectable>().bookName,1);
                objectText.text = objectText.text+("\n")+("Book");
            }
            if (obj.GetComponent<KeyCollectable>())
            {
                PlayerPrefs.SetInt("hasKey",1);
                PlayerPrefs.SetInt(obj.GetComponent<KeyCollectable>().keyName,1);
                objectText.text = objectText.text+("\n")+("Key");
            }
        }
        if (collectableList.Count != 0)
        {
            
            itemGotBox.SetActive(true);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if(levelName == "first")
        {
        }
        if (other.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoadNext);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (levelCompleteClip != null)
            {
                levelCompleteSource.PlayOneShot(levelCompleteClip, levelCompleteVolume);
            }
            ending = true;
            canvas.SetActive(true);
            UpdateCollectables();
            if(SceneManager.GetActiveScene().name=="Level 1")
            {
                PlayerPrefs.SetInt("FinishedFirstLevel",1);
            }
            if(SceneManager.GetActiveScene().name=="Level 2")
            {
                PlayerPrefs.SetInt("FinishedSecondLevel",1);
            }

            //SceneManager.LoadScene(sceneToLoadNext);
        }
    }
}
