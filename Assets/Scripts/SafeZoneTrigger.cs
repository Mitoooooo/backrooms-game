using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class SafeZoneTrigger : MonoBehaviour
    {
        private CeilingLightController ceilingLightController;
        private BoxCollider safeZoneCollider;
        private DayNightController dayNightController;
        private GameObject gameManager;
        private NavMeshObstacle navMeshObstacle;

        private void Start()
        {
            //safeZoneCollider = GetComponent<BoxCollider>();
            navMeshObstacle = GetComponent<NavMeshObstacle>();

            Transform parentTransform = transform.parent;

            safeZoneCollider = transform.parent.GetComponent<BoxCollider>();
            if (parentTransform != null)
            {
                ceilingLightController = parentTransform.GetComponentInChildren<CeilingLightController>();
            }

            if (ceilingLightController == null)
            {
                Debug.LogError("SafeZoneTrigger: CeilingLightController not found in sibling.");
            }

            //dayNightController = GameObject.FindGameObjectWithTag("GameManager").GetComponent<DayNightController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player entered the SafeZone!");

                StartCoroutine(ToggleLightOffAndDisableSafeZone(5f));               
            }
        }

        private void OnTriggerExit(Collider other)
        {

        }

        IEnumerator ToggleLightOffAndDisableSafeZone(float delay)
        {
            yield return new WaitForSeconds(delay);
            ceilingLightController?.ToggleLightOff();
            safeZoneCollider.enabled = false;
            navMeshObstacle.enabled = false;
            //dayNightController.ChooseRandomCeilingLights();
        }
    }
}
