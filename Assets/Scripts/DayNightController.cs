using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static Assets.Scripts.Enums;

namespace Assets.Scripts
{
    public class DayNightController : MonoBehaviour
    {
        GameObject[] ceilingLights;
        GameObject player;

        List<BoxCollider> activeSafeZones;
        private EntityState _entityState;
        public EntityState CurrentState => _entityState;

        private DayNightState _dayNightState;

        public DayNightState CurrentDayNightState => _dayNightState;

        public float dayDuration = 2f; // 5 minutes in seconds
        public float nightDuration = 1f; // 1 minute in seconds

        private float triggerTimer;
        private float triggerInterval = 5f;

        private Coroutine dayNightCycleCoroutine;
        //private string dayNightCycle;
        public TMP_Text dayNightText;

        public EntitySoundGenerator entitySoundGenerator; 

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            activeSafeZones = new List<BoxCollider>();

            StartCoroutine(StupidCoroutine());
            //StartDayNightCycle(); 
        }

        void Update()
        {
            /*if (Input.GetKeyDown(KeyCode.Y))
            {
                ChooseRandomCeilingLights();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                SwitchToNight();
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                SwitchToDay();
            }*/

            dayNightText.text = _dayNightState.ToString();

            TriggerPer5Secs();
        }

        IEnumerator StupidCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            if(activeSafeZones == null)
            {
                StartCoroutine(StupidCoroutine());
            }
            else
            {
                StartDayNightCycle();
            }
        }

        void StartDayNightCycle()
        {
            if (dayNightCycleCoroutine != null)
            {
                StopCoroutine(dayNightCycleCoroutine);
            }
            dayNightCycleCoroutine = StartCoroutine(DayNightCycle());
        }

        IEnumerator DayNightCycle()
        {
            while (true)
            {
                if (_dayNightState == DayNightState.STATE_DAY)
                {
                    Debug.Log("Switching to Day");
                    SwitchToDay();
                    //dayNightCycle = _dayNightState.ToString();
                    yield return new WaitForSeconds(dayDuration);
                    _dayNightState = DayNightState.STATE_NIGHT;
                    _entityState = EntityState.STATE_CHASING;
                }
                else if (_dayNightState == DayNightState.STATE_NIGHT)
                {
                    Debug.Log("Switching to Night");
                    SwitchToNight();
                    //dayNightCycle = _dayNightState.ToString();
                    yield return new WaitForSeconds(nightDuration);
                    _dayNightState = DayNightState.STATE_DAY;
                    _entityState = EntityState.STATE_ROAMING;
                }
            }
        }

        public void SwitchState()
        {

        }

        public void SwitchToNight()
        {
            ceilingLights = GameObject.FindGameObjectsWithTag("CeilingLight");
            foreach (GameObject ceilingLight in ceilingLights)
            {
                CeilingLightController lightController = ceilingLight.GetComponentInChildren<CeilingLightController>();

                lightController.ToggleLightOff();
                //CreateSafeZone(lightController);
            }
            //ChooseRandomCeilingLights();

            // play growl sound
            //entitySoundGenerator.PlayGrowl();
        }

        public void SwitchToDay()
        {
            ceilingLights = GameObject.FindGameObjectsWithTag("CeilingLight");
            foreach (GameObject ceilingLight in ceilingLights)
            {
                CeilingLightController lightController = ceilingLight.GetComponentInChildren<CeilingLightController>();

                lightController.ToggleLightOn();
                //CreateSafeZone(lightController);
            }

            if (activeSafeZones.Count > 0)
            {
                DisableAllSafeZones();
            }
        }

        //hehe code ngu vcl
        void CreateSafeZone(CeilingLightController lightController)
        {
            Transform parentTransform = lightController.transform.parent;
            if (parentTransform != null)
            {
                Transform safeZoneTransform = parentTransform.Find("SafeZone");
                if (safeZoneTransform != null)
                {
                    BoxCollider safeZoneCollider = safeZoneTransform.GetComponent<BoxCollider>();
                    NavMeshObstacle safeZoneObstacle = safeZoneTransform.GetComponent<NavMeshObstacle>();
                    if (safeZoneCollider != null)
                    {
                        if (!safeZoneCollider.enabled) 
                        {                           
                            safeZoneCollider.enabled = true;
                            safeZoneObstacle.enabled = true;
                            activeSafeZones.Add(safeZoneCollider);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("SafeZone found, but no BoxCollider attached.");
                    }
                }
                else
                {
                    Debug.LogWarning("SafeZone not found as a sibling.");
                }
            }
        }

        void DisableSafeZone(CeilingLightController lightController)
        {
            Transform parentTransform = lightController.transform.parent;
            if (parentTransform != null)
            {
                Transform safeZoneTransform = parentTransform.Find("SafeZone");
                if (safeZoneTransform != null)
                {
                    BoxCollider safeZoneCollider = safeZoneTransform.GetComponent<BoxCollider>();
                    NavMeshObstacle safeZoneObstacle = safeZoneTransform.GetComponent<NavMeshObstacle>();
                    if (safeZoneCollider != null)
                    {
                        if (safeZoneCollider.enabled)
                        {                            
                            safeZoneCollider.enabled = false;
                            safeZoneObstacle.enabled = false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("SafeZone found, but no BoxCollider attached.");
                    }
                }
                else
                {
                    Debug.LogWarning("SafeZone not found as a sibling.");
                }
            }
        }

        void DisableAllSafeZones()
        {
            foreach(BoxCollider safeZoneCollider in activeSafeZones)
            {
                //NavMeshObstacle safeZoneObstacle = GetComponent<NavMeshObstacle>();
                if (safeZoneCollider.enabled)
                {
                    safeZoneCollider.enabled = false;
                    //safeZoneObstacle.enabled = false;
                }
            }

            activeSafeZones.Clear();
        }

        public void ChooseRandomCeilingLights()
        {
            Collider[] colliders = Physics.OverlapSphere(player.transform.position, 40f);
            List<CeilingLightController> nearbyLights = new List<CeilingLightController>();

            foreach (Collider col in colliders)
            {
                CeilingLightController lightController = col.GetComponentInParent<CeilingLightController>();
                if (lightController != null)
                {
                    nearbyLights.Add(lightController);
                }
            }

            // Step 2: Select two random lights
            if (nearbyLights.Count >= 2)
            {
                // Select two random lights
                int firstIndex = Random.Range(0, nearbyLights.Count);
                int secondIndex;

                do
                {
                    secondIndex = Random.Range(0, nearbyLights.Count);
                } while (secondIndex == firstIndex);

                CeilingLightController firstLight = nearbyLights[firstIndex];
                CeilingLightController secondLight = nearbyLights[secondIndex];               

                if (firstLight != null && secondLight != null)
                {
                    // Toggle the selected lights                                     
                    if (firstLight.isLightOn == false) 
                    {
                        firstLight.ToggleLight();
                        CreateSafeZone(firstLight);
                    }
                    else
                    {
                        firstLight.ToggleLight();
                        DisableSafeZone(firstLight);
                    }
                    
                    if (secondLight.isLightOn == false)
                    {
                        secondLight.ToggleLight();
                        CreateSafeZone(secondLight);
                    }
                    else
                    {
                        secondLight.ToggleLight();
                        DisableSafeZone(secondLight);
                    }

                    Debug.Log($"Toggled lights: {firstLight.gameObject.name} and {secondLight.gameObject.name}");
                }            
            }
            else
            {
                Debug.LogWarning("Not enough lights within the radius!");
            }
        }       

        void TriggerPer5Secs()
        {
            if(_dayNightState == DayNightState.STATE_NIGHT)
            {
                triggerTimer += Time.deltaTime; // Increment timer with time passed since last frame       

                if (triggerTimer >= triggerInterval)
                {
                    ChooseRandomCeilingLights();

                    // Reset the timer
                    triggerTimer = 0f;
                }
            }            
        }
    }
}