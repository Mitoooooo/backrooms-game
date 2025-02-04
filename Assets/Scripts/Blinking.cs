using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinking : MonoBehaviour {

    private float playerHeight = 2f;
    private float playerRadius = 0.5f;
    //public int gunDamage = 1;                                            // Set the number of hitpoints that this gun will take away from shot objects with a health script
    public float fireRate = 0.25f;                                        // Number in seconds which controls how often the player can fire
    public float blinkRange = 50f;                                        // Distance in Unity units over which the player can fire
    //public float hitForce = 100f;                                        // Amount of force which will be added to objects with a rigidbody shot by the player
    //public Transform gunEnd;                                            // Holds a reference to the gun end object, marking the muzzle location of the gun

    public Camera fpsCam;                                                // Holds a reference to the first person camera
    //private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);    // WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible
    //private AudioSource gunAudio;                                        // Reference to the audio source which will play our shooting sound effect
    private Transform laserOrigin;
    private LineRenderer laserLine;                                        // Reference to the LineRenderer component which will display our laserline
    private float nextFire;                                                // Float to store the time the player will be allowed to fire again, after firing
    private RaycastHit hit;                     // Declare a raycast hit to store information about what our raycast has hit

    void Start () 
    {
        // Get and store a reference to our LineRenderer component
        laserLine = GetComponent<LineRenderer>();
        laserOrigin = GetComponent<Transform>();
        laserLine.enabled = false;
    }

    void Update () 
    {
        PlayerBlink();
    }
    
    void PlayerBlink()
    {
        laserOrigin = laserOrigin.transform;
        laserLine.SetPosition(0, laserOrigin.position);

        // Create a vector at the center of our camera's viewport
        Vector3 rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
        // Check if the player has pressed the fire button and if enough time has elapsed since they last fired

        if (Input.GetKeyUp(KeyCode.F) && Time.time > nextFire)
        {
            laserLine.enabled = false;
            // Update the time when our player can fire next
            nextFire = Time.time + fireRate;

            // Check if our raycast has hit anything
            if (Physics.Raycast(rayOrigin, fpsCam.transform.forward, out hit, blinkRange))
            {
                StartCoroutine(LerpBlink(hit.point, 0.1f));
            }
            else
            {
                // If we did not hit anything, set the end of the line to a position directly in front of the camera at the distance of weaponRange
                laserLine.SetPosition(1, rayOrigin + (fpsCam.transform.forward * blinkRange));
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.F))
            {
                if (Physics.Raycast(rayOrigin, fpsCam.transform.forward, out hit, blinkRange))
                {
                    // Set the end position for our laser line 
                    laserLine.enabled = true;
                    laserLine.SetPosition(1, hit.point);
                }
                else
                {
                    // If we did not hit anything, set the end of the line to a position directly in front of the camera at the distance of weaponRange
                    laserLine.SetPosition(1, rayOrigin + (fpsCam.transform.forward * blinkRange));
                }
            }
        }
    }

    IEnumerator LerpBlink(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;
        Vector3 temp = transform.position;
        temp.y = targetPosition.y + playerHeight;
        temp.x = targetPosition.x;
        temp.z = targetPosition.z;
        while (time < duration)
        {
            //transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            transform.position = Vector3.Lerp(startPosition, temp, time / duration);   
            time += Time.deltaTime;
            yield return null;
        }
        //transform.position = targetPosition;
        transform.position = temp;
    }

    IEnumerator LerpVault(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }

    // private IEnumerator ShotEffect()
    // {
    //     // Play the shooting sound effect
    //     gunAudio.Play ();

    //     // Turn on our line renderer
    //     laserLine.enabled = true;

    //     //Wait for .07 seconds
    //     yield return shotDuration;

    //     // Deactivate our line renderer after waiting
    //     laserLine.enabled = false;
    // }
}