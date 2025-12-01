using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    AudioSource audioSource;
    FollowCam followCam;
    GameObject player;
    List<GameObject> feathers = new List<GameObject>();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        followCam = Camera.main.GetComponent<FollowCam>();
        player = GameObject.FindGameObjectWithTag("Player").gameObject;
        GameObject.FindGameObjectsWithTag("feather",feathers);
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying)
        {
            Debug.Log(player.transform.position.x);
        }
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
}
