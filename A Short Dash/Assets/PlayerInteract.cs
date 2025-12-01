using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    GameObject interactableObject;
    bool canInteract = false;
    public bool frozen = false;

    void Start()
    {
        
    }


    void Update()
    {
        if(!frozen && canInteract && Input.GetKeyDown(KeyCode.Space))
        {
            interactableObject.GetComponent<TextWindowLoader>().StartText();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("interactable"))
        {
            interactableObject = other.gameObject;
            canInteract = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("interactable"))
        {
            interactableObject = null;
            canInteract = false;
        }
    }
}
