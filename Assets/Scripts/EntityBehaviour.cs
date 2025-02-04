using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using WebSocketSharp;
using static Assets.Scripts.Enums;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class EntityBehaviour : MonoBehaviour
{
    public GameObject spawnWall;
    public GameObject targetWall;
    private NavMeshAgent agent;
    private CameraDetector cameraDetector;
    //Vector3 spawnPosition;
    //Vector3 targetPosition;
    GameObject player;
    Transform playerTransform;
    public float minRangeSphere = 10f; // Minimum range to scan for walls
    public float maxRangeSphere = 20f; // Maximum range to scan for walls
    public float caughtDistance = 10f;
    public string wallTag = "Wall"; // Tag for the wall objects
    private float changeToRoamingInterval;
    private float changeToChasingInterval;
    public Vector3 outOfSightPosition;

    private float moveTimer;
    private float moveInterval = 0.5f;

    private float triggerTimer;
    private float triggerInterval = 4f;

    private float moveTimer2;
    private float moveCooldown = 45f;

    private float spotTimer;
    private float spotDuration;

    private Coroutine runningCoroutine = null;

    private DayNightController dayNightController;
    private EntityState _state;

    LayerMask wallMask;
    GameObject[] walls;

    bool movement = false;

    bool switchMode = false;

    public Material originalMaterial;
    public Material highlightMaterial;

    public TMP_Text entityStateText;
    private bool isRunning = false;
    private bool isActive = false;

    private bool isMoving = false;
    private bool isFootstepCooldownActive = false;
    private EntitySoundGenerator entitySoundGenerator;
    private GameOverCutscene gameOverCutscene;

    //private bool entityDetection;

    void Start()
    {
        // Initialize NavMeshAgent
        agent = GetComponent<NavMeshAgent>();

        // Find the CameraDetector script on the main camera or another GameObject
        cameraDetector = Camera.main.GetComponent<CameraDetector>();

        // Get Player Transform
        player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player.GetComponent<Transform>();

        //StartCoroutine(TriggerEveryHalfSecondCoroutine());

        //mapGenerator = FindObjectOfType<MapGenerator>();
        wallMask = LayerMask.GetMask("Walls");
        walls = GameObject.FindGameObjectsWithTag("Wall");
        dayNightController = FindFirstObjectByType<DayNightController>();
        entitySoundGenerator = GetComponent<EntitySoundGenerator>();

        gameOverCutscene = player.GetComponent<GameOverCutscene>();
        //entityDetection = cameraDetector.entityDetection;
    }

    private void Update()
    {
        //SpawnEntity();

        //TriggerSpawnAndMoveEntityPerSecond();

        //TriggerPer2Secs(); 

        StateSwitch();

        entityStateText.text = _state.ToString();

        EntityMovement();

        CheckMoving();

        PlayFootSound();

        if (cameraDetector.entityDetection == true)
        {
            EntityRoamCooldown();
            //EntitySpotted();
        }

        TeleportEntityOutOfSight2();      
    }

    void TeleportEntityOutOfSight2()
    {
        if (!agent.hasPath && cameraDetector.entityDetection == true && transform.position != Vector3.zero)
        {
            transform.position = Vector3.zero;
            targetWall = null;
            agent.ResetPath();
        }
    }

    void StateSwitch()
    {
        if(dayNightController.CurrentDayNightState == DayNightState.STATE_DAY)
        {
            _state = EntityState.STATE_ROAMING;
        }

        if (dayNightController.CurrentDayNightState == DayNightState.STATE_NIGHT)
        {
            _state = EntityState.STATE_CHASING;
        }
    }

    void TriggerPer2Secs()
    {
        triggerTimer += Time.deltaTime; // Increment timer with time passed since last frame       

        if (triggerTimer >= triggerInterval)
        {
            //TeleportEntityOutOfSight2();

            // Reset the timer
            triggerTimer = 0f;
        }
    }

    void TriggerSpawnAndMoveEntityPerSecond()
    {
        moveTimer += Time.deltaTime; // Increment timer with time passed since last frame       

        if (moveTimer >= moveInterval /*&& targetWall == null*/)
        {
            // Trigger the desired action every 0.5 seconds
            SpawnAndMoveEntity();

            // Reset the timer
            moveTimer = 0f;
        }
    }

    void EntityRoamCooldown()
    {
        moveTimer2 += Time.deltaTime; // Increment timer with time passed since last frame       

        if (moveTimer2 >= moveCooldown)
        {
            cameraDetector.entityDetection = false;

            // Reset the timer
            moveTimer2 = 0f;
        }
    }

    void SpawnEntity()
    {
        if (Input.GetKeyUp(KeyCode.P) && !agent.hasPath)
        {
            // If any 2 walls are detected in camera view
            if (cameraDetector.detectedWalls.Count > 1)
            {
                SpawnAndMoveEntity(); 
            }
            else
            {
                Debug.LogWarning("No walls detected in camera view.");
            }
        }

        if (Input.GetKeyUp(KeyCode.O) && !agent.hasPath)
        {
            if (switchMode == false)
            {
                switchMode = true;
            }
            else
            {
                switchMode = false;
            }
        }

        //if (Input.GetKeyUp(KeyCode.I))
        //{
        //    Debug.Log("movement: " + movement);
        //    _state = EntityState.STATE_ROAMING;
        //    if (movement == false)
        //    {
        //        movement = true;
        //    }
        //    else
        //    {
        //        movement = false;
        //    }
        //}

        //if (movement == true)
        //{
        //    EntityMovement();
        //}
    }

    // Find spawn point behind the wall and move entity
    public void SpawnAndMoveEntity()
    {
        if (isRunning) return; // Exit if already running
        isRunning = true;
        try
        {
            List<GameObject> detectedWalls = cameraDetector.DetectInCameraView2();
            // Determine a random spawn wall
            GameObject spawnWall = detectedWalls[Random.Range(0, cameraDetector.detectedWalls.Count)];

            Debug.Log("Spawn wall: " + spawnWall.name + "Position :" + spawnWall.transform.position);

            // Determine spawn point behind the wall
            Vector3 spawnPosition = GetPositionBehindWall(spawnWall);
            Debug.Log(spawnPosition);
            GetComponent<NavMeshAgent>().enabled = false;
            transform.position = spawnPosition;
            GetComponent<NavMeshAgent>().enabled = true;
            Debug.Log(transform.position);
            Debug.Log(transform.localPosition);

            // highlight spawn wall for debugging
            //HighlightObject(spawnWall);

            List<GameObject> targetWalls = ScanSurroundingWalls(spawnWall, 40, wallMask); 

            // Find target wall and check for a straight path
            StartCoroutine(ChooseTargetWallAndCheckPath(targetWalls, spawnWall, spawnPosition));
        }
        finally
        {
            isRunning = false; // Reset the flag even if an exception occurs
        }
        //RemoveHighlight(spawnWall);
        // Start checking for reaching the destination and call Teleport when done
        //StartCoroutine(CheckDestinationFinished(TeleportEntityOutOfSight));
    }

    // Sphere scan to find walls and then move the entity
    public void SpawnAndMoveEntitySphere()
    {
        // Maybe let it move straight instead of moving between walls so it moves even more frequently?

        // Scan for walls withing the specified range       
        List<GameObject> wallsInRange = ScanForWallsInRange(minRangeSphere, maxRangeSphere);
        if(wallsInRange.Count > 0)
        {
            GameObject spawnWall = wallsInRange[Random.Range(0, wallsInRange.Count)];

            // Spawn entity
            Vector3 spawnPosition = GetPositionBehindWall(spawnWall);
            transform.position = spawnPosition;

            List<GameObject> targetWalls = ScanSurroundingWalls(spawnWall, 50, wallMask);

            StartCoroutine(ChooseTargetWallAndCheckPath(targetWalls, spawnWall, spawnPosition));
            //yield return true;
            //yield return StartCoroutine(CheckDestinationFinished(TeleportEntityOutOfSight));
        }       
    }

    public void SpawnAndMoveEntitySphere2()
    {
        // Maybe let it move straight instead of moving between walls so it moves even more frequently?

        // Scan for walls within the specified range       
        List<GameObject> wallsInRange = ScanForWallsInRange(minRangeSphere, maxRangeSphere);
        if (wallsInRange.Count > 0)
        {
            // Select a random wall from the detected walls
            GameObject spawnWall = wallsInRange[Random.Range(0, wallsInRange.Count)];

            // Spawn entity at a position behind the selected wall
            Vector3 spawnPosition = GetPositionBehindWall(spawnWall);
            transform.position = spawnPosition;

            // Calculate the movement direction to avoid running straight at the player
            Vector3 moveDirection = CalculateAvoidPlayerDirection(spawnPosition, player.transform.position);

            // Move the entity in the calculated direction
            float moveSpeed = 15f;
            float maxTravelDistance = 40f;
            StartCoroutine(MoveEntityStraight(transform, moveDirection, moveSpeed, maxTravelDistance));            
            //yield return StartCoroutine(CheckDestinationFinished(TeleportEntityOutOfSight));
        }
    }

    public void SpawnEntityChase()
    {
        // Scan for walls within the specified range       
        List<GameObject> wallsInRange = ScanForWallsInRange(minRangeSphere, maxRangeSphere);
        if (wallsInRange.Count > 0)
        {
            // Select a random wall from the detected walls
            GameObject spawnWall = wallsInRange[Random.Range(0, wallsInRange.Count)];

            // Spawn entity at a position behind the selected wall
            Vector3 spawnPosition = GetPositionBehindWall(spawnWall);
            transform.position = spawnPosition;
        }
    }

    public void MoveEntityChase()
    {
        var destination = playerTransform.position;

        if (IsDestinationValid(destination))
        {
            agent.SetDestination(playerTransform.position);
            isActive = true;
        }
        else
        {
            if (isActive == true)
            {
                TeleportEntityOutOfSight();
                isActive = false;
            }
        }
    }

    public bool IsDestinationValid(Vector3 destination)
    {
        NavMeshHit hit;
        bool isValid = NavMesh.SamplePosition(destination, out hit, 1.0f, NavMesh.AllAreas);
        return isValid;
    }

    private Vector3 CalculateAvoidPlayerDirection(Vector3 spawnPosition, Vector3 playerPosition)
    {
        // Vector from the player to the spawn point
        Vector3 toSpawn = spawnPosition - playerPosition;

        // Normalize the vector to keep consistent direction
        toSpawn.Normalize();

        // Calculate a perpendicular vector to ensure the entity doesn't run directly toward the player
        Vector3 perpendicularDirection = Vector3.Cross(toSpawn, Vector3.up); // Assuming Y is the vertical axis

        // Randomize left or right direction
        if (Random.value > 0.5f)
        {
            perpendicularDirection = -perpendicularDirection;
        }

        return perpendicularDirection.normalized;
    }

    private IEnumerator MoveEntityStraight(Transform entity, Vector3 direction, float speed, float maxTravelDistance)
    {
        float traveledDistance = 0f; // Track how far the entity has traveled

        while (traveledDistance < maxTravelDistance) // Stop after reaching max distance
        {
            // Calculate movement for this frame
            Vector3 movement = direction * speed * Time.deltaTime;

            // Move the entity
            entity.position += movement;

            // Update traveled distance
            traveledDistance += movement.magnitude;

            yield return null; // Wait for the next frame
        }
    }

    // Calculate a spawn point behind the wall
    Vector3 GetPositionBehindWall(GameObject wall)
    {
        Camera cam = Camera.main;
        Vector3 wallPosition = wall.transform.position;
        Vector3 directionToWall = wallPosition - cam.transform.position;
        Vector3 spawnPosition = wallPosition + directionToWall.normalized * 2f;
        spawnPosition.y = 2f;
        return spawnPosition;
    }

    //Choose a target wall and ensure a straight path exists
    IEnumerator ChooseTargetWallAndCheckPath(List<GameObject> detectedWalls, GameObject spawnWall, Vector3 spawnPosition)
    {
        List<GameObject> validWalls = new List<GameObject>();
        List<NavMeshPath> validPaths = new List<NavMeshPath>();
        //GameObject newTargetWall;

        if (detectedWalls.Count <= 1)
        {
            Debug.LogWarning("Only one wall detected, unable to choose a different target.");
            yield return false;
        }

        foreach(GameObject targetWall in detectedWalls)
        {
            //If the new target wall is not the same as the spawn wall or within the same grid, exit the loop
            if (
                targetWall != spawnWall &&
                targetWall.transform.parent != spawnWall.transform.parent &&
                targetWall.transform.parent != null &&
                spawnWall.transform.parent != null)
            {
                validWalls.Add(targetWall);
            }
        }   

        if (validWalls.Count > 0)
        {
            foreach (GameObject newTargetWall in validWalls)
            {
                targetWall = newTargetWall;
                Vector3 targetPosition = GetPositionBehindWall(newTargetWall);
                //Debug.Log("Target wall: " + targetWall.name + "Position :" + targetWall.transform.position);

                //Use NavMesh to check for a straight path
                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(spawnPosition, targetPosition, NavMesh.AllAreas, path);

                //Check if the path is valid and straight
                if (path.status == NavMeshPathStatus.PathComplete && IsPathStraight(path))
                {
                    validPaths.Add(path);
                }
            }
        }

        if(validPaths.Count > 0)
        {
            Debug.Log("test");
            StartCoroutine(EntityMoveToDestination(validPaths, spawnPosition));
            //NavMeshPath newTargetPath = validPaths[Random.Range(0, validPaths.Count)];
            //transform.position = spawnPosition;
            //agent.SetPath(newTargetPath);
            //MoveToTarget(targetWall);

            // (!) this will override and mess up the transform.position of the entity, resulting in mismatch in spawnPosition and actual entity's transform.position
            //yield return StartCoroutine(CheckDestinationFinished(TeleportEntityOutOfSight));
        }
        else
        {
            Debug.Log("No straight path available, canceling movement.");

            targetWall = null;
            yield return false;
        }
    }

    IEnumerator EntityMoveToDestination(List<NavMeshPath> validPaths, Vector3 spawnPosition)
    {
        NavMeshPath newTargetPath = validPaths[Random.Range(0, validPaths.Count)];
        //transform.position = spawnPosition;
        yield return new WaitForSeconds(0.1f);
        agent.SetPath(newTargetPath);
    }

    // Check if the path generated by NavMesh is straight
    bool IsPathStraight(NavMeshPath path)
    {
        return path.corners.Length <= 3; // 2 cung k dc ma 3 cung k xong??? // p/s: fk unity's pathfinding
    }

    IEnumerator CheckDestinationFinished(System.Action onDestinationFinished)
    {
        while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null; // Wait until the next frame
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                //Debug.Log("Destination reached");
                onDestinationFinished?.Invoke();
            }
        }
    }

    void EntityMovement()
    {     
        playerTransform = player.GetComponent<Transform>();
        //_state = dayNightController.CurrentState;
        switch (_state)
        {
            case EntityState.STATE_ROAMING:
                // Set speed
                GetComponent<NavMeshAgent>().speed = 40f;

                if(cameraDetector.entityDetection == false)
                {
                    TriggerSpawnAndMoveEntityPerSecond();
                }                                          
                break;

            case EntityState.STATE_CHASING:
                // Set Entity's speed, is it enough (?)
                GetComponent<NavMeshAgent>().speed = 10f;                
                if (!IsAgentMoving(agent))
                {
                    SpawnEntityChase();
                }

                MoveEntityChase();

                // Player caught
                if (Vector3.Distance(playerTransform.position, transform.position) < caughtDistance)
                {
                    gameOverCutscene.TriggerCaughtCutscene();
                }

                break;
        }
    }

    public void Provoke()
    {
        _state = EntityState.STATE_PROVOKED;
        GetComponent<NavMeshAgent>().speed = Random.Range(5f, 10f);
        agent.SetDestination(playerTransform.position);
    }

    IEnumerator TriggerRunningAtRandomIntervals()
    {
        while (true) // Infinite loop to keep triggering
        {
            // Wait for a random time between 5 and 7 seconds
            float waitTime = Random.Range(5f, 7f);
            yield return new WaitForSeconds(waitTime);

            // Trigger entity spawning
            //SpawnAndMoveEntitySphere();
        }
    }

    // Scan for walls within a specified range of player using OverlapSphere
    List<GameObject> ScanForWallsInRange(float minRange, float maxRange)
    {
        List<GameObject> wallsInRange = new List<GameObject>();

        Collider[] hits = Physics.OverlapSphere(player.transform.position, maxRange);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Wall"))
            {
                float distanceToWall = Vector3.Distance(player.transform.position, hit.transform.position);

                if (distanceToWall >= minRange && distanceToWall <= maxRange)
                {
                    wallsInRange.Add(hit.gameObject);
                }
            }
        }

        return wallsInRange;
    }

    // Scan for walls within a specified range of another wall using OverlapSphere
    List<GameObject> ScanSurroundingWalls(GameObject inputWall, float radius, LayerMask layerMask)
    {
        // Get the root GameObject of the inputWall's prefab
        Transform inputWallRoot = inputWall.transform.root;

        // Get the position of the input wall
        Vector3 position = inputWall.transform.position;

        // Find all colliders within the radius
        Collider[] colliders = Physics.OverlapSphere(position, radius, layerMask);

        // List to store nearby walls
        List<GameObject> nearbyWalls = new List<GameObject>();

        foreach (Collider collider in colliders)
        {
            // Get the root GameObject of the detected wall
            Transform detectedWallRoot = collider.transform.root;

            // Ensure the detected wall is not the input wall itself and is not from the same prefab
            if (collider.gameObject != inputWall && detectedWallRoot != inputWallRoot)
            {
                nearbyWalls.Add(collider.gameObject);
            }
        }

        return nearbyWalls;
    }

    void TeleportEntityOutOfSight()
    {
        transform.position = outOfSightPosition;   
        targetWall = null;
        agent.ResetPath();       
    }

    void HighlightObject(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            //originalMaterial = renderer.material; // Save the original material
            renderer.material = highlightMaterial; // Set the highlight material
        }
        //return null;
    }

    void RemoveHighlight(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && originalMaterial != null)
        {
            renderer.material = originalMaterial; // Restore the original material
            targetWall = null;
        }
        //return null; 
    }

    bool IsAgentMoving(NavMeshAgent agent)
    {
        // Check if the agent has a path, the path is not pending, the distance to the destination is significant, and the agent is not stopped.
        return agent.hasPath && !agent.pathPending && agent.remainingDistance > 0.1f && !agent.isStopped;
    }

    void CheckMoving()
    {
        if (IsAgentMoving(agent))
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    void PlayFootSound()
    {
        if (isMoving && !isFootstepCooldownActive)
        {
            var randomIndex = Random.Range(0.15f, 0.4f);
            StartCoroutine(PlayStepSound(0.3f)); // Start sound with a cooldown
        }
    }

    IEnumerator PlayStepSound(float cooldown)
    {
        isFootstepCooldownActive = true; // Activate the cooldown

        var clipCount = entitySoundGenerator.entityFootsteps.Count;
        var randomIndex = Random.Range(0, clipCount);
        entitySoundGenerator.audioSource.clip = entitySoundGenerator.entityFootsteps[randomIndex];

        entitySoundGenerator.audioSource.Play(); // Play the footstep sound

        yield return new WaitForSeconds(cooldown); // Wait for the cooldown duration

        isFootstepCooldownActive = false; // Reset the cooldown
    }
} 