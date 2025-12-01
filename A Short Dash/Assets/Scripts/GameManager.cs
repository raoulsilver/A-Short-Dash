using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    AudioSource audioSource;
    FollowCam followCam;
    GameObject player;
    List<GameObject> feathers = new List<GameObject>();

    public AudioClip song1;
    public AudioClip song2;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        followCam = Camera.main.GetComponent<FollowCam>();
        player = GameObject.FindGameObjectWithTag("Player").gameObject;
        GameObject.FindGameObjectsWithTag("feather",feathers);
        if(SceneManager.GetActiveScene().name == "First Zone Scene")
        {
            PlayerPrefs.SetInt("FinishedFirstLevel",1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying)
        {
            //Debug.Log(player.transform.position.x);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SuperReset();
        }
    }

    private void SuperReset()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Mountain Base Scene");
    }
    public void Reset()
    {
        audioSource.Stop();
        audioSource.Play();
        followCam.Reset();
        foreach(GameObject feather in feathers)
        {
            feather.SetActive(true);
        }
    }

    public void SetNewSong(AudioClip newClip)
    {
        audioSource.clip = newClip;
    }


}
