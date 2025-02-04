using Assets.Scripts;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject light;
    private CeilingLightController ceilingLightController;

    public GameObject entity;
    private NavMeshAgent agent;

    private Vector3 entityPos1 = new Vector3(1, 1, 42);
    private Vector3 entityPos2 = new Vector3(16, 1, 42);

    private float lightTriggerTimer;
    private float entityTriggerTimer;

    private float lightTriggerInterval;
    private float entityTriggerInterval;
    private GameObject levelLoader;
    private LoadingScene loadingScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ceilingLightController = light.GetComponent<CeilingLightController>();
        agent = entity.GetComponent<NavMeshAgent>();
        lightTriggerInterval = Random.Range(10f, 15f);
        entityTriggerInterval = Random.Range(20f, 25f);

        levelLoader = GameObject.Find("LevelLoader");
        loadingScene = levelLoader.GetComponent<LoadingScene>();

        UnlockCursor();
    }

    // Update is called once per frame
    void Update()
    {
        TriggerLight();

        TriggerEntity();
    }

    void TriggerLight()
    {
        lightTriggerTimer += Time.deltaTime; // Increment timer with time passed since last frame       

        if (lightTriggerTimer >= lightTriggerInterval)
        {
            StartCoroutine (AnimateLight());

            // Reset the timer
            lightTriggerTimer = 0f;
            lightTriggerInterval = Random.Range(10f, 15f);
        }
    }

    public void Test()
    {
        Debug.Log("test");
    }

    void TriggerEntity()
    {
        entityTriggerTimer += Time.deltaTime; // Increment timer with time passed since last frame       

        if (entityTriggerTimer >= entityTriggerInterval)
        {
            MoveEntityRandom();

            // Reset the timer
            entityTriggerTimer = 0f;
            entityTriggerInterval = Random.Range(20f, 25f);
        }
    }

    // animating with idiocy
    IEnumerator AnimateLight()
    {
        ceilingLightController.ToggleLight();
        yield return new WaitForSeconds(0.01f);
        ceilingLightController.ToggleLight();

        yield return new WaitForSeconds(1f);

        ceilingLightController.ToggleLight();
        yield return new WaitForSeconds(0.01f);
        ceilingLightController.ToggleLight();

        yield return new WaitForSeconds(1f);
        ceilingLightController.ToggleLight();
    }

    void MoveEntityRandom()
    {
        int random = Random.Range(0, 2);
        if (random == 0)
        {
            entity.transform.position = entityPos1;
            agent.SetDestination(entityPos2);
        }
        else
        {
            entity.transform.position = entityPos2;
            agent.SetDestination(entityPos1);
        }
    }

    public void OnPlayPressed()
    {
        StartCoroutine(loadingScene.LoadLevel(1));
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }

    void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
