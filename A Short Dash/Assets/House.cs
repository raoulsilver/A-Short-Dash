using UnityEngine;
using UnityEngine.SceneManagement;

public class House : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayerPrefs.GetInt("FinishedSecondLevel") == 0)
        {
            GetComponent<SphereCollider>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Interact()
    {
        SceneManager.LoadScene("End Screen");
    }
}
