using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interactor : MonoBehaviour
{
    public Camera cam;

    public GameObject interactOverlay;

    private void Update()
    {
        InteractWithFrontObject();
    }

    void InteractWithFrontObject()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var firstInteractable, 2f))
        {
            //print(firstInteractable.collider.tag);
            IInteractable interactable = firstInteractable.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactOverlay.SetActive(true);
                Text text = interactOverlay.GetComponent<Text>();
                text.text = interactable.GetText();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
            }
        }
        else
        {
            interactOverlay.SetActive(false);           
        }
    }
}
