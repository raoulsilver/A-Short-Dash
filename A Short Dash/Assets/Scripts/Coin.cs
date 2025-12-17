using UnityEngine;

public class Coin : MonoBehaviour
{

    public string coinName;
    [SerializeField] private AudioClip collectClip;
    [SerializeField, Range(0f,1f)] private float collectVolume = 0.9f;
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
            if (collectClip != null)
            {
                GameObject temp = new GameObject("CollectSound");
                AudioSource src = temp.AddComponent<AudioSource>();
                src.clip = collectClip;
                src.volume = collectVolume;
                src.spatialBlend = 0f;
                src.Play();
                Destroy(temp, collectClip.length);
            }
            levelLoad.collectableList.Add(gameObject);
            gameObject.SetActive(false);
        }
    }
}
