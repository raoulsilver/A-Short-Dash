using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    GameObject interactableObject;
    public bool canInteract = false;
    public bool frozen = false;
    bool collectingItem = false;
    GameObject otherCanvas;
    public static PlayerInteract instance;

    void Start()
    {
        instance = this;
    }


    void Update()
    {
        if(!frozen && canInteract && Input.GetKeyDown(KeyCode.Space))
        {
            if (interactableObject.GetComponent<House>())
            {
                interactableObject.GetComponent<House>().Interact();
            }
            interactableObject.GetComponent<TextWindowLoader>().StartText();
        }
        if (!frozen)
        {
            collectingItem = false;
        }
    }

    /*public void GetItem(string itemLineIdToDisplay,GameObject modelToDisplay)
    {
        LoadTextManager.instance.StartText(itemLineIdToDisplay);
        collectingItem = true;
        //VariableManager.instance.UpdateVariable(varToUpdate,1);
    }*/

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("interactable"))
        {
            interactableObject = other.gameObject;
            canInteract = true;
            Debug.Log(other.transform.parent);
            otherCanvas=other.transform.parent.GetComponentInChildren<Canvas>(true).gameObject;
            otherCanvas.SetActive(true);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("interactable"))
        {
            otherCanvas.SetActive(false);
            otherCanvas = null;
            interactableObject = null;
            canInteract = false;
        }
    }
}
