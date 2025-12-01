using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    AudioSource audioSource;
    FollowCam followCam;
    GameObject player;


    List<GameObject> feathers = new List<GameObject>();

    public AudioClip song1;
    public AudioClip song2;
    // ======= ADDED SETTINGS =======
    [Header("Music Sync Settings")]
    public Transform levelStartPoint;     // assign empty object at level start
    public float moveSpeed = 12f;         // MUST match PlayerMovement.moveSpeed
    // =================================


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


        audioSource.Play(); // optional auto-start, won't break your old logic
    }

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


        // ======= OPTIONAL TEST KEY (REMOVE IF YOU WANT) =======
        if (Input.GetKeyDown(KeyCode.M))
            SyncMusicToPlayerPos(); // press M to sync manually anytime
        // ======================================================
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
        // If reset respawns player → sync music to new position automatically
        SyncMusicToPlayerPos();
    }

    public void SetNewSong(AudioClip newClip)
    {
        audioSource.clip = newClip;
    }

    // ======= NEW FUNCTION ADDED — SAFE, NON-DESTRUCTIVE =======
    public void SyncMusicToPlayerPos()
    {
        if (player == null || levelStartPoint == null) return;

        float dist = player.transform.position.x - levelStartPoint.position.x;
        float targetTime = dist / moveSpeed;

        targetTime = Mathf.Clamp(targetTime, 0f, audioSource.clip.length);
        audioSource.time = targetTime;
    }
}