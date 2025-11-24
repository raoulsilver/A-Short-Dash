using UnityEngine;

public class BeatMarkerTapper : MonoBehaviour
{
    public Transform player;
    public KeyCode beatKey = KeyCode.B;

    public static System.Action<Vector3> OnBeat;

    void Update()
    {
        if (Application.isPlaying && Input.GetKeyDown(beatKey))
        {
            Vector3 pos = player.position;
            OnBeat?.Invoke(pos);
            Debug.Log("Beat tapped at " + pos);
        }
    }
}