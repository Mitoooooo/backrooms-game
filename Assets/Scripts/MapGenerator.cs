/*using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
 
namespace Assets.Scripts
{
    public class MapGenerator : MonoBehaviour
    {
        public int width = 10;    // Width of the map grid
        public int height = 10;   // Height of the map grid
        public float noiseScale = 0.1f; // Controls the "frequency" of the noise
        public GameObject[] roomPrefabs; // Array of 12 room prefabs
        public NavMeshSurface navMeshSurface;
        void Start()
        {
            StartCoroutine(GenerateAndBakeMap());
            //BakeNavMesh();
        }

        IEnumerator GenerateAndBakeMap()
        {
            // Wait for GenerateMap to finish
            yield return StartCoroutine(GenerateMap());

            // Then call BakeNavMesh
            BakeNavMesh();
        }

        IEnumerator GenerateMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Generate a noise value for the (x, y) position
                    float noiseValue = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);

                    // Map the noise value to an integer index for room configurations
                    int roomIndex = Mathf.FloorToInt(noiseValue * roomPrefabs.Length);
                    roomIndex = Mathf.Clamp(roomIndex, 0, roomPrefabs.Length - 1);

                    // Calculate the position based on grid coordinates
                    int randomRoomPos = Random.Range(1, 3);                    
                    Vector3 roomPosition = new Vector3((x * 2 + randomRoomPos) * 10, 0, (y * 2 + randomRoomPos) * 10); // Cell size 10 units, multiply with randomRoomPos so the room placement seems more random, 4 cells (x*2, y*2)

                    // Instantiate the corresponding room prefab at the calculated grid position
                    GameObject room = Instantiate(roomPrefabs[roomIndex], roomPosition, Quaternion.identity);
                }
            }

            yield return null;
        }

        void BakeNavMesh()
        {
            // Build NavMesh on the single NavMeshSurface
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh has been baked on the main surface.");
        }
    }
}
  */

using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts
{
    public class MapGenerator : MonoBehaviour
    {
        public int width = 10;    // Width of the map grid
        public int height = 10;   // Height of the map grid
        public float noiseScale = 0.1f; // Controls the "frequency" of the noise
        public GameObject[] roomPrefabs; // Array of 12 room prefabs
        public GameObject ceilingLightPrefab;
        public GameObject ceilingPrefab;
        public NavMeshSurface navMeshSurface;

        private int seed; // Seed for procedural generation
        public bool generateCeilingLights;
        public bool generateCeiling;
        public bool generateWalls;

        private GameObject[,] bigGrid; // Store generated rooms for big grid reference

        public GameObject testWall;

        public GameObject exitPrefab;

        public GameObject player;

        void Awake()
        {
            seed = Random.Range(1, 200);         
        }

        private void Start()
        {
            bigGrid = new GameObject[width, height];
            StartCoroutine(GenerateAndBakeMap());
        }

        private void Update()
        {
            if (testWall != null)
            {
                List<GameObject> surroundingWalls = GetSurroundingWalls(testWall, 10);
                Debug.Log("Surrounding walls: " + surroundingWalls.Count);
            }
        }

        IEnumerator GenerateAndBakeMap()
        {
            if (generateWalls == true)
            {
                yield return StartCoroutine(GenerateMap());
            }

            if (generateCeilingLights == true) 
            {
                yield return StartCoroutine(GenerateCeilingLights());
            }

            if (generateCeiling == true)
            {
                yield return StartCoroutine(GenerateCeiling());
            }

            BakeNavMesh();

            var detector = player.GetComponentInChildren<CameraDetector>();
            detector.enabled = true;
        }

        IEnumerator GenerateMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Use Perlin noise to determine the room type based on the seed
                    float noiseValue = Mathf.PerlinNoise((x + seed) * noiseScale, (y + seed) * noiseScale);
                    int roomIndex = Mathf.FloorToInt(noiseValue * roomPrefabs.Length);
                    roomIndex = Mathf.Clamp(roomIndex, 0, roomPrefabs.Length - 1);

                    // Calculate a seed-based offset for `randomRoomPos` to ensure repeatable randomness
                    int randomRoomPos = Mathf.Abs((seed + x * 101 + y * 103) % 3) + 1;

                    // Determine room position within the big grid cell using this offset
                    Vector3 roomPosition = new Vector3((x * 2 + randomRoomPos) * 10, 0, (y * 2 + randomRoomPos) * 10);

                    // Instantiate the room at the calculated position
                    GameObject room = Instantiate(roomPrefabs[roomIndex], roomPosition, Quaternion.identity);
                    bigGrid[x, y] = room; // Store the reference to this room in the big grid array
                }
            }

            // Replace a random WallGrid with the specific prefab
            ReplaceRandomWallGrid();

            yield return null;
        }

        void ReplaceRandomWallGrid()
        {
            // Find all WallGrid objects in the scene
            GameObject[] wallGrids = GameObject.FindGameObjectsWithTag("WallGrid");

            // Check if there are any WallGrids
            if (wallGrids.Length == 0)
            {
                Debug.LogWarning("No WallGrids found in the scene!");
                return;
            }

            // Pick a random WallGrid
            //int randomIndex = wallGrids.Length % seed;
            //int randomIndex = Random.Range(0, wallGrids.Length);
            // fixed exit position
            GameObject selectedWallGrid = wallGrids[20];

            // Save the position and rotation of the selected WallGrid
            Vector3 position = selectedWallGrid.transform.position;
            Quaternion rotation = selectedWallGrid.transform.rotation;

            // Destroy the selected WallGrid
            Destroy(selectedWallGrid);

            // Instantiate the specific prefab at the same position and rotation
            Instantiate(exitPrefab, position, rotation);

            //Debug.Log($"Replaced WallGrid at index {randomIndex} with the specific prefab.");
        }

        IEnumerator GenerateCeilingLights()
        {
            // original +2, but why?
            for (int x = 0; x < width/2 + 2; x++)
            {
                for (int y = 0; y < height/2 + 2; y++)
                {
                    // Determine light position within the big grid cell using this offset
                    Vector3 lightPosition = new Vector3((x * 4) * 10, 4.6f, (y * 4) * 10);

                    // Instantiate the ceiling light at the calculated position
                    GameObject ceiling = Instantiate(ceilingLightPrefab, lightPosition, Quaternion.identity);

                    // Ensure ceiling does not affect NavMesh
                    NavMeshModifier modifier = ceiling.AddComponent<NavMeshModifier>();
                    modifier.ignoreFromBuild = true;
                }
            }
            //BakeLighting();
            yield return null;
        }

        IEnumerator GenerateCeiling()
        {
            // why +3 tho?
            for (int x = 0; x < width * 2 + 3; x++)
            {
                for (int y = 0; y < height * 2 + 3; y++)
                {
                    // Determine room position within the big grid cell using this offset
                    Vector3 ceilingPosition = new Vector3(x * 10, 4.6f, y * 10);

                    // Instantiate the room at the calculated position
                    GameObject ceiling = Instantiate(ceilingPrefab, ceilingPosition, Quaternion.identity);                    
                }
            }

            yield return null;
        }

        /*public List<Vector3> GetSurroundingBigGridPrefabPositions(int bigGridX, int bigGridY, int cellSize, int seed)
        {
            List<Vector3> prefabPositions = new List<Vector3>();

            // Define bounds for the big grid (3x3 layout, so valid indices are 0, 1, and 2)
            int maxBigGridIndex = 2;

            // Loop through the 3x3 area centered on the given big grid
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Skip the center big grid itself
                    if (dx == 0 && dy == 0) continue;

                    // Calculate the neighboring big grid coordinates
                    int neighborBigGridX = bigGridX + dx;
                    int neighborBigGridY = bigGridY + dy;

                    // Check if the neighboring big grid is within bounds
                    if (neighborBigGridX >= 0 && neighborBigGridX <= maxBigGridIndex &&
                        neighborBigGridY >= 0 && neighborBigGridY <= maxBigGridIndex)
                    {
                        // Calculate the world position of the top-left corner of the neighboring big grid
                        Vector3 bigGridOrigin = new Vector3(neighborBigGridX * 2 * cellSize, 0, neighborBigGridY * 2 * cellSize);

                        // Determine which cell in the 2x2 big grid contains the prefab based on the seed
                        int prefabCellIndex = (seed + neighborBigGridX * 3 + neighborBigGridY * 5) % 4;

                        // Calculate the offset within the 2x2 grid for the prefab cell
                        Vector3 prefabPositionOffset;
                        switch (prefabCellIndex)
                        {
                            case 0: prefabPositionOffset = new Vector3(0, 0, 0); break;      // Top-left
                            case 1: prefabPositionOffset = new Vector3(cellSize, 0, 0); break; // Top-right
                            case 2: prefabPositionOffset = new Vector3(0, 0, cellSize); break; // Bottom-left
                            default: prefabPositionOffset = new Vector3(cellSize, 0, cellSize); break; // Bottom-right
                        }

                        // Add the prefab's exact position within the neighboring big grid
                        Vector3 prefabPosition = bigGridOrigin + prefabPositionOffset;
                        prefabPositions.Add(prefabPosition);
                    }
                }
            }

            return prefabPositions;
        }*/

        public List<GameObject> GetSurroundingWalls(GameObject inputWall, int cellSize)
        {
            // Get the position of the input wall in world space
            Vector3 prefabPosition = inputWall.transform.position;

            // Get the parent (or prefab group) of the input wall
            Transform inputWallParent = inputWall.transform.parent;

            // Calculate the big grid and small grid this wall belongs to
            (int bigGridX, int bigGridY, int smallGridX, int smallGridY) = GetGridCoordinates(prefabPosition, cellSize);

            // List to store the surrounding walls
            List<GameObject> surroundingWalls = new List<GameObject>();

            // Loop through the surrounding 8 big grids (3x3 area minus the center big grid)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Skip the center big grid (where the input wall is located)
                    if (dx == 0 && dy == 0) continue;

                    // Calculate the neighboring big grid coordinates
                    int neighborBigGridX = bigGridX + dx;
                    int neighborBigGridY = bigGridY + dy;

                    // Check if the neighboring big grid is within bounds (adjust for your grid dimensions)
                    if (neighborBigGridX >= 0 && neighborBigGridX < 3 && neighborBigGridY >= 0 && neighborBigGridY < 3)
                    {
                        // Get the wall prefab from the neighboring big grid
                        GameObject wall = GetWallFromBigGrid(neighborBigGridX, neighborBigGridY);

                        // Ensure the wall exists and is not part of the same prefab as the input wall
                        if (wall != null && wall.transform.parent != inputWallParent)
                        {
                            surroundingWalls.Add(wall);
                        }
                    }
                }
            }

            return surroundingWalls;
        }


        // Helper method to calculate grid coordinates from a prefab's world position
        private (int bigGridX, int bigGridY, int smallGridX, int smallGridY) GetGridCoordinates(Vector3 position, int cellSize)
        {
            // Calculate the big grid coordinates
            int bigGridX = Mathf.FloorToInt(position.x / (2 * cellSize));
            int bigGridY = Mathf.FloorToInt(position.z / (2 * cellSize));

            // Calculate the small grid coordinates within the big grid
            int smallGridX = Mathf.FloorToInt((position.x % (2 * cellSize)) / cellSize);
            int smallGridY = Mathf.FloorToInt((position.z % (2 * cellSize)) / cellSize);

            return (bigGridX, bigGridY, smallGridX, smallGridY);
        }

        // Helper method to get the wall prefab from a specific big grid
        private GameObject GetWallFromBigGrid(int bigGridX, int bigGridY)
        {
            // Retrieve the big grid GameObject (this assumes you have a 2D array of big grids)
            GameObject bigGridObject = bigGrid[bigGridX, bigGridY];

            // Find the wall prefab within the big grid (you may need to adjust this based on your hierarchy)
            foreach (Transform child in bigGridObject.transform)
            {
                if (child.CompareTag("Wall")) // Ensure its a wall prefab
                {
                    return child.gameObject;
                }
            }

            return null;
        }

       /* public void BakeLighting()
        {
            Lightmapping.Bake();
        }*/

        void BakeNavMesh()
        {
            navMeshSurface.BuildNavMesh();
            //Debug.Log("NavMesh has been baked on the main surface.");
        }
    }
}
