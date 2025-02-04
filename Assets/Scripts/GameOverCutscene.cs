using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverCutscene : MonoBehaviour
{
    public Camera playerCamera;    // Reference to the player's camera
    public Camera cutsceneCamera;
    public GameObject flashlight;
    public GameObject mainFlashlight;
    private float dropSpeed = 4f;      // Speed of the camera dropping
    private float dropDistance = 0.75f;  // How far the camera drops
    private float rotationSpeed = 2f; // Speed of rotation

    private bool isCaught = false;    // Flag to check if the player is caught
    private Vector3 originalPosition; // Camera's original position
    private Quaternion originalRotation; // Camera's original rotation

    //public GameObject player;
    private PlayerMove playerMoveScript;

    public Animator endTransition;

    private List<GameObject> listGameObjects = new List<GameObject>();
    private GameObject entity;
    private GameObject gameManager;
    private LoadingScene loadingScene;
    private Rigidbody rb;

    private void Start()
    {
        /*// Save the original position and rotation of the camera
        originalPosition = cutsceneCamera.transform.localPosition;
        originalRotation = cutsceneCamera.transform.localRotation;*/
        
        cutsceneCamera.enabled = false;
        flashlight.SetActive(false);

        playerMoveScript = GetComponent<PlayerMove>();

        // Find GameManager dynamically by tag, name, or type
        gameManager = GameObject.Find("GameManager");
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene!");
        }

        loadingScene = gameManager.GetComponentInChildren<LoadingScene>();

        listGameObjects.Add(gameManager);

        // Find Entity dynamically by tag, name, or type
        entity = GameObject.FindWithTag("Entity");
        if (entity == null)
        {
            Debug.LogError("Entity not found in the scene!");
        }

        listGameObjects.Add(entity);
        
        rb = GetComponent<Rigidbody>();
    }

    public void TriggerCaughtCutscene()
    {
        cutsceneCamera.enabled = true;
        flashlight.SetActive(true);
        mainFlashlight.SetActive(false);
        isCaught = true;
        rb.isKinematic = true;

        playerMoveScript.enabled = false;
        DisableGameObjects();
    }

    private void Update()
    {
        if (isCaught)
        {
            PlayCutscene();
        }

        /*if (Input.GetKeyDown(KeyCode.C))
        {
            TriggerCaughtCutscene();
        }*/
    }

    void DisableGameObjects()
    {
        foreach (GameObject gameObject in listGameObjects)
        {
            gameObject.SetActive(false);
        }
    }

    private void PlayCutscene()
    {
        // Move the camera down
        Vector3 targetPosition = originalPosition + Vector3.down * dropDistance;
        cutsceneCamera.transform.localPosition = Vector3.Lerp(cutsceneCamera.transform.localPosition, targetPosition, dropSpeed * Time.deltaTime);

        // Rotate the camera
        Quaternion targetRotation = Quaternion.Euler(0f, cutsceneCamera.transform.localRotation.eulerAngles.y, 50f);
        cutsceneCamera.transform.localRotation = Quaternion.Slerp(cutsceneCamera.transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Optionally, you can add a condition to stop the cutscene or trigger another event
        if (Vector3.Distance(cutsceneCamera.transform.localPosition, targetPosition) < 0.01f)
        {
            // Cutscene complete
            Debug.Log("Cutscene complete");
            StartCoroutine(loadingScene.LoadLevel(0));
            isCaught = false; // Stop the cutscene
        }
    }   
}
