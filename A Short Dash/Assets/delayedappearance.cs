using System.Collections;
using TMPro;
using UnityEngine;

public class delayedappearance : MonoBehaviour
{
    [SerializeField]
    float startDelay;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<TMP_Text>().enabled = false;
        StartCoroutine(DelayStart());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(startDelay);
        GetComponent<TMP_Text>().enabled = true;
        yield break;
    }
}
