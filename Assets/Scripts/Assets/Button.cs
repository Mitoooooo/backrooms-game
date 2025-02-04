using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{
    private Vector3 originalPosition;

    public GameObject otherObject;

    public string GetText()
    {
        return "test";
    }

    public void Interact()
    {       
        IInteractable interactable = otherObject.GetComponent<IInteractable>();
        interactable.Interact();
    }

    public void InteractWithAnotherObject(GameObject otherObject)
    {
        //button animator       
        //object to interact
        IInteractable interactable = otherObject.GetComponent<IInteractable>();
        interactable.Interact();
    }
}
