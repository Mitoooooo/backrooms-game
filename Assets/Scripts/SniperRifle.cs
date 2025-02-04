using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperRifle : MonoBehaviour, IShootable
{
    public Animator animator;
    //public Animation anim;

    public bool isScoped = false;
    public bool scoping = false;

    //public float playbackTime;
    public float playbackTime = Animator.StringToHash("ScopeFloat");
    public float maxTime = 1f;
    public float minTime = 0;
    public float timeAccelerationPerSecond = 1f;

    //weapon stats
    public int gunDamage = 1;                                           
    public float fireRate = 0.25f;                                        
    public float weaponRange = 50f;
    public float hitForce = 100f;

    public GameObject scopeOverlay;

    public GameObject weaponCamera;

    private Camera fpsCam;

    private AudioSource gunAudio;

    private LineRenderer laserLine;

    private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);            // WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible
    
    private float nextFire;

    public Transform gunEnd;

    private void Start()
    {
        //anim["ScopeIn"].normalizedTime = 0.5f;
        scopeOverlay.SetActive(false);

        // Get and store a reference to our LineRenderer component
        laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = false;

        // Get and store a reference to our AudioSource component
        gunAudio = GetComponent<AudioSource>();

        // Get and store a reference to our Camera by searching this GameObject and its parents
        fpsCam = GetComponentInParent<Camera>();
    }

    void Update()
    {
        //Shoot();

        Scope();
    }

    public void Shoot()
    {
        // Check if the player has pressed the fire button and if enough time has elapsed since they last fired
        if (Input.GetButtonDown("Fire1") && Time.time > nextFire)
        {
            // Update the time when our player can fire next
            nextFire = Time.time + fireRate;

            // Start our ShotEffect coroutine to turn our laser line on and off
            StartCoroutine(ShotEffect());

            // Create a vector at the center of our camera's viewport
            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

            // Declare a raycast hit to store information about what our raycast has hit
            RaycastHit hit;

            // Set the start position for our visual effect for our laser to the position of gunEnd
            laserLine.SetPosition(0, gunEnd.position);

            // Check if our raycast has hit anything
            if (Physics.Raycast(rayOrigin, fpsCam.transform.forward, out hit, weaponRange))
            {
                // Set the end position for our laser line 
                laserLine.SetPosition(1, hit.point);

                // Get a reference to a health script attached to the collider we hit
                Damageable health = hit.collider.GetComponent<Damageable>();

                // If there was a health script attached
                if (health != null)
                {
                    // Call the damage function of that script, passing in our gunDamage variable
                    health.Damage(gunDamage);
                }

                // Check if the object we hit has a rigidbody attached
                if (hit.rigidbody != null)
                {
                    // Add force to the rigidbody we hit, in the direction from which it was hit
                    hit.rigidbody.AddForce(-hit.normal * hitForce);
                }
            }
            else
            {
                // If we did not hit anything, set the end of the line to a position directly in front of the camera at the distance of weaponRange
                laserLine.SetPosition(1, rayOrigin + (fpsCam.transform.forward * weaponRange));
            }
        }
    }

    public void Scope()
    {
        if (Input.GetButton("Fire2"))
        {
            if (playbackTime < maxTime)
            {
                playbackTime += timeAccelerationPerSecond * Time.deltaTime;
                animator.SetFloat("ScopeFloat", playbackTime);
                //print("Playback time: " + playbackTime);
            }
            else if (scopeOverlay.active == false)
            {
                playbackTime = maxTime;
                animator.SetFloat("ScopeFloat", 0.9999999f);
                StartCoroutine(OnScoped());
                isScoped = true;
            }
        }
        else
        {
            if (scopeOverlay.active == true)
            {
                OnUnscoped();
            }

            if (playbackTime > 0)
            {
                playbackTime -= timeAccelerationPerSecond * Time.deltaTime; //possibly speedDeaccelerationPerSecond depending on your needs
                animator.SetFloat("ScopeFloat", playbackTime);
                //print("Playback time: " + playbackTime);
            }
            else
            {
                playbackTime = minTime;
                animator.SetFloat("ScopeFloat", 0.0001f);
            }
        }
    }

    private IEnumerator ShotEffect()
    {
        // Play the shooting sound effect
        //gunAudio.Play();

        // Turn on our line renderer
        laserLine.enabled = true;

        //Wait for .07 seconds
        yield return new WaitForSeconds(0.07f); //shotDuration;

        // Deactivate our line renderer after waiting
        laserLine.enabled = false;
    }

    IEnumerator OnScoped()
    {
        yield return new WaitForSeconds(0.15f);

        scopeOverlay.SetActive(true);

        weaponCamera.SetActive(false);
    }

    void OnUnscoped()
    {
        scopeOverlay.SetActive(false);

        weaponCamera.SetActive(true);
    }
}
