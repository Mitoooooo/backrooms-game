using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Ragdoll : MonoBehaviour, IInteractable
{
    public Rigidbody[] rigidbodies;
    public Collider[] colliders;

    public GameObject ragdollObject;

    public GameObject ragdollObject2;

    private GameObject newRagdollObject;

    private GameObject newRagdollObject2;

    private bool ragdollState = true;

    private void Awake()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        ChangeRagdollState(true);     
    }

    public string GetText()
    {
        return null;
    }

    public void ChangeRagdollState(bool kinematic)
    {
        Debug.Log("ragdoll state" + kinematic);
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = kinematic;
        }
    }

    public void Interact()
    {
        if (newRagdollObject != null)
        {
            if (ragdollState == true)
            {
                ragdollState = !ragdollState;
                newRagdollObject.GetComponent<Ragdoll>().ChangeRagdollState(ragdollState);
                newRagdollObject2.GetComponent<Ragdoll>().ChangeRagdollState(ragdollState);
            }
            else
            {
                //Destroy and Create a prefab
                ragdollState = !ragdollState;
                newRagdollObject.GetComponent<Ragdoll>().ChangeRagdollState(ragdollState);
                newRagdollObject2.GetComponent<Ragdoll>().ChangeRagdollState(ragdollState);
                //ragdollObject.GetComponent<Ragdoll>().ChangeRagdollState(ragdollState);

                //Debug.Log(originalPosition);
                Destroy(newRagdollObject);
                Destroy(newRagdollObject2);

                newRagdollObject = Instantiate(ragdollObject, new Vector3(-6.5f, 0f, -16f), Quaternion.identity);
                newRagdollObject2 = Instantiate(ragdollObject2, new Vector3(-9.5f, 0f, -16f), Quaternion.identity);
                //Debug.Log(newRagdollObject.transform.position);
                //Destroy(newRagdollObject);
            }
        }
        else
        {
            //originalPosition = ragdollObject.transform.position;
            newRagdollObject = Instantiate(ragdollObject, new Vector3(-6.5f, 0f, -16f), Quaternion.identity);
            newRagdollObject2 = Instantiate(ragdollObject2, new Vector3(-9.5f, 0f, -16f), Quaternion.identity);
        }
    }
}
