using UnityEngine;

public class KeyCollectable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public string keyName;
    [SerializeField] private AudioClip collectClip;
    [SerializeField, Range(0f,1f)] private float collectVolume = 0.9f;
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
