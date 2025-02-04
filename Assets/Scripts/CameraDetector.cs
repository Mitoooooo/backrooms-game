using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDetector : MonoBehaviour
{
    GameObject[] walls;
    GameObject[] wallGrids;
    GameObject entity;
    public List<GameObject> detectedWalls;
    List<GameObject> objectsToDetect;

    public float minDetectionDistance = 50f;
    public float maxDetectionDistance = 80f;

    public bool entityDetection = true;

    public Camera cam;

    void Start()
    {
        detectedWalls = new List<GameObject>();
        walls = GameObject.FindGameObjectsWithTag("Wall");
        wallGrids = GameObject.FindGameObjectsWithTag("WallGrid");
        //objectsToDetect = new List<GameObject> { walls };
        cam = Camera.main; // Assuming you're using the main camera
        entity = GameObject.FindGameObjectWithTag("Entity");
    }

    void Update()
    {
        FindEntityInView(entity);

        /*if (Input.GetKeyUp(KeyCode.P))
        {
            DetectInCameraView();
        }*/
    }

    // Detects anything in view
    //public void DetectInCameraView()
    //{
    //    Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

    //    detectedWalls.Clear();

    //    // Detects objects in view
    //    foreach (var wall in walls)
    //    {
    //        FindObjectInView(planes, wall);
    //    }
    //}

    //void FindObjectInView(Plane[] planes, GameObject obj)
    //{
    //    // Get the object's bounding box (Collider bounds can be used)
    //    Collider objectCollider = obj.GetComponent<Collider>();

    //    if (GeometryUtility.TestPlanesAABB(planes, objectCollider.bounds))
    //    {
    //        float distanceToWall = Vector3.Distance(Camera.main.transform.position, obj.transform.position);

    //        // Check if the wall is within the specified distance range
    //        if (distanceToWall > minDetectionDistance && distanceToWall < maxDetectionDistance)
    //        {
    //            Debug.Log($"Object in view and within distance: {obj.name}, Distance: {distanceToWall}");
    //            detectedWalls.Add(obj);
    //        }
    //    }
    //}

    public void DetectInCameraView()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        detectedWalls.Clear();

        // Detect objects in view
        foreach (var wall in walls)
        {
            FindObjectInView(planes, wall);
        }       
    }

    public List<GameObject> DetectInCameraView2()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        detectedWalls.Clear();

        // Detect objects in view
        foreach (var wall in walls)
        {
            FindObjectInView(planes, wall);
        }

        return detectedWalls;
    }

    void FindObjectInView(Plane[] planes, GameObject obj)
    {
        Collider objectCollider = obj.GetComponent<Collider>();

        // Check if the object is inside the camera frustum
        if (GeometryUtility.TestPlanesAABB(planes, objectCollider.bounds))
        {
            // Check multiple points on the collider for visibility
            Vector3[] checkPoints = GetColliderPoints(objectCollider);

            foreach (Vector3 point in checkPoints)
            {
                if (IsPointVisible(point, obj))
                {
                    detectedWalls.Add(obj);
                    //Debug.Log($"Wall in view and not blocked: {obj.name}");
                    break; // Add the wall once and stop checking further points
                }
            }
        }
    }

    bool IsPointVisible(Vector3 point, GameObject obj)
    {
        Vector3 directionToPoint = (point - Camera.main.transform.position).normalized;
        Ray ray = new Ray(Camera.main.transform.position, directionToPoint);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDetectionDistance))
        {
            return hit.collider.gameObject == obj; // True if this point is not blocked
        }

        return false;
    }

    Vector3[] GetColliderPoints(Collider collider)
    {
        Bounds bounds = collider.bounds;

        // Select key points on the collider
        return new Vector3[]
        {
        bounds.center,                          // Center
        bounds.min,                             // Bottom-left-back
        bounds.max,                             // Top-right-front
        new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), // Top-left-back
        new Vector3(bounds.max.x, bounds.min.y, bounds.max.z)  // Bottom-right-front
        };
    }

    public void FindEntityInView(GameObject entity)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        Collider entityCollider = entity.GetComponent<Collider>();
        if (entityCollider == null) return;

        // Check if the entity's bounding box is within the camera's frustum
        if (GeometryUtility.TestPlanesAABB(planes, entityCollider.bounds))
        {
            Vector3[] checkPoints = GetColliderPoints(entityCollider);

            foreach (Vector3 point in checkPoints)
            {
                if (IsPointVisible(point, entity))
                {
                    Debug.Log($"Entity detected: {entity.name}");
                    entityDetection = true;
                    break;
                }
            }
        }
    }
}
