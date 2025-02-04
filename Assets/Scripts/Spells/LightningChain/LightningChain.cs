using Assets.Scripts.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningChain : Spell
{
    private LineRenderer laserLine;

    private GameObject player;
    private Camera playerCam;

    public Transform gunEnd;
    public float weaponRange = 50f;
    private int damageableLayer;
    public int maxChainCount;
    public float sphereRadius;
    private int currentPositionCount;

    // store informations of chained objects
    private HashSet<GameObject> chainedObjects = new HashSet<GameObject>();

    private void Start()
    {
        // Get and store a reference to our LineRenderer component
        laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = false;

        player = GameObject.Find("Player");
        playerCam = player.GetComponentInChildren<Camera>();

        damageableLayer = LayerMask.GetMask("DamageableEntity");
    }

    public override void Cast()
    {
        chainedObjects.Clear();
        // Create a vector at the center of our camera's viewport
        Vector3 rayOrigin = playerCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));       

        // Declare a raycast hit to store information about what our raycast has hit
        RaycastHit hit;

        RaycastHit hit2;

        if (Physics.SphereCast(rayOrigin, 1f, playerCam.transform.forward, out hit, weaponRange, damageableLayer))
        {
            // Start our ShotEffect coroutine to turn our laser line on and off
            StartCoroutine(ShotEffect());

            // Set the start position for our visual effect for our laser to the position of gunEnd
            laserLine.SetPosition(0, gunEnd.position);

            // Set the end position for our laser line 
            laserLine.SetPosition(1, hit.collider.bounds.center/*hit.point*/);
          
            // Get a reference to a health script attached to the collider we hit
            Damageable health = hit.collider.GetComponent<Damageable>();

            ChainLightning(hit.collider.gameObject, 1);
            /*// If there was a health script attached
            if (health != null)
            {
                // Call the damage function of that script, passing in our gunDamage variable
                health.Damage(gunDamage);
            }*/

            /*// Check if the object we hit has a rigidbody attached
            if (hit.rigidbody != null)
            {
                // Add force to the rigidbody we hit, in the direction from which it was hit
                hit.rigidbody.AddForce(-hit.normal * hitForce);
            }*/

            /*       else
                   {
                       // If we did not hit anything, set the end of the line to a position directly in front of the camera at the distance of weaponRange
                       laserLine.SetPosition(1, rayOrigin + (playerCam.transform.forward * weaponRange));
                   }*/
        }
        else
        {
            Debug.Log("Not hit");
        }
    }

    private void ChainLightning(GameObject currentEnemy, int currentChainCount)
    {       
        if (currentChainCount <= maxChainCount && !chainedObjects.Contains(currentEnemy))
        {         
            chainedObjects.Add(currentEnemy);

            Collider targetCollider = currentEnemy.GetComponent<Collider>();

            Collider[] colliders = Physics.OverlapSphere(targetCollider.bounds.center/*currentEnemy.transform.position*/, sphereRadius, damageableLayer);
 
            Collider closestCollider = GetClosestCollider(targetCollider.bounds.center, colliders, targetCollider, chainedObjects);

            if (closestCollider != null && !chainedObjects.Contains(closestCollider.gameObject))
            {
                currentPositionCount = laserLine.positionCount;
                laserLine.positionCount = currentPositionCount + 1;

                // Set the new position for the next segment
                laserLine.SetPosition(currentPositionCount, closestCollider.bounds.center);

                // Add the object to the set of chained objects
                //chainedObjects.Add(closestCollider.gameObject);

                // Chain lightning to the next enemy    
                ChainLightning(closestCollider.gameObject, currentChainCount + 1);
                //currentPositionCount++;
            }
        }
    }

    private Collider GetClosestCollider(Vector3 point, Collider[] colliders, Collider excludedCollider, HashSet<GameObject> exclusionSet)
    {
        Collider closestCollider = null;
        float closestDistance = float.MaxValue;

        foreach (var collider in colliders)
        {
            if (collider != excludedCollider && !exclusionSet.Contains(collider.gameObject))
            {
                float distance = Vector3.Distance(point, collider.bounds.center);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCollider = collider;
                }
            }
        }

        return closestCollider;
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
}
