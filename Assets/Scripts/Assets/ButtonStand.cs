using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonStand : MonoBehaviour
{
    // This script is not needed, can be deleted (replaced with Interactor)





    public Camera cam;

    public GameObject interactOverlay;

    private Collider test;

    private bool isInteractable = true;

    // Start is called before the first frame update
    void Start()
    {
        interactOverlay.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //MessageShow();

        ButtonPress();
    } 

    void ButtonPress()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var firstInteractable, 2f))
        {
            //print(firstInteractable.collider.tag);
            if (firstInteractable.collider.tag == "Ragdoll Button")
            {
                interactOverlay.SetActive(true);
                IInteractable interactable = firstInteractable.collider.GetComponent<IInteractable>();
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
