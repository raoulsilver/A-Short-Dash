using UnityEngine;
using TMPro;

public class TextBox : MonoBehaviour
{
    public static LoadTextManager loadTextManager;
    public bool inDialogue;
    private bool wasInDialogue = false;
    [Header("Dialogue Audio")]
    [SerializeField] private AudioClip voiceClipA;
    [SerializeField] private AudioClip voiceClipB;
    [SerializeField, Range(0f,1f)] private float voiceVolume = 0.8f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TextWindowLoader.textBoxToDisplay = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (inDialogue && !wasInDialogue)
        {
            PlayDialogueVoice();
        }

        if(inDialogue && Input.GetKeyDown(KeyCode.Space))
        {
            loadTextManager.DisplayNextDialogue();
            PlayDialogueVoice();
        }

        wasInDialogue = inDialogue;
    }

    void PlayDialogueVoice()
    {
        AudioClip clipToPlay = Random.value < 0.5f ? voiceClipA : voiceClipB;
        if (clipToPlay == null) return;

        GameObject temp = new GameObject("DialogueVoice");
        AudioSource src = temp.AddComponent<AudioSource>();
        src.clip = clipToPlay;
        src.volume = voiceVolume;
        src.spatialBlend = 0f;
        src.Play();
        Destroy(temp, clipToPlay.length);
    }

    public void SendText(string lineID)
    {
        //LoadTextManager.instance.SetText(textBoxToDisplay,lineIdToDisplay);
    }
}
