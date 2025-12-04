using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    GameObject interactableObject;
    bool canInteract = false;
    public bool frozen = false;
    bool collectingItem = false;

    void Start()
    {
        
    }


    void Update()
    {
        if(!frozen && canInteract && Input.GetKeyDown(KeyCode.Space))
        {
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
