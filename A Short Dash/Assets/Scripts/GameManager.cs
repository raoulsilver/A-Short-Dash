using UnityEngine;

public class GameManager : MonoBehaviour
{

    AudioSource audioSource;
    FollowCam followCam;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        followCam = Camera.main.GetComponent<FollowCam>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Reset()
    {
        audioSource.Stop();
        audioSource.Play();
        followCam.Reset();
    }
}
