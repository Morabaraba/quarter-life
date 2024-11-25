using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking; // Add this namespace for UnityWebRequest
using DunGenRef;

[ExecuteInEditMode]
public class DunGenMap : MonoBehaviour
{
    /// <summary>
    /// Path to the map file.
    /// </summary>
    public string mapFile; // Path to the map file

    /// <summary>
    /// Default tile width.
    /// </summary>
    public float tileWidth = 2.0f; // Default tile width

    /// <summary>
    /// Default tile height.
    /// </summary>
    public float tileHeight = 2.0f; // Default tile height

    /// <summary>
    /// GameObject containing all the character prefabs.
    /// </summary>
    public GameObject charactersPrefab; // GameObject containing the nested chr-<char> prefabs

    /// <summary>
    /// Array to hold the map data.
    /// </summary>
    private string[] map;

    public void Start()
    {
        if (Application.isPlaying)
        {
            StartLoad();
        }
    }

    public void StartLoad()
    {
        StartCoroutine(StartLoading());
    }
    /// <summary>
    /// Coroutine to load the map data and continue execution.
    /// </summary>
    public IEnumerator StartLoading()
    {  
        yield return StartCoroutine(LoadMap());

        // Continue execution after map data has been loaded
        StartLoaded();
    }

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    public void StartLoaded()
    {
        GenerateMap();
    }

    /// <summary>
    /// Loads the map data from the specified file.
    /// </summary>
    public IEnumerator LoadMap()
    {
        string filePath = Application.streamingAssetsPath + "/" + mapFile;

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Map file not found: " + filePath);
                map = new string[0];
            }
            else
            {
                string mapData = www.downloadHandler.text;
                map = mapData.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
            }
        }
        else
        {
            if (File.Exists(filePath))
            {
                map = File.ReadAllLines(filePath);
            }
            else
            {
                Debug.LogError("Map file not found: " + filePath);
                map = new string[0];
            }
        }
    }

    /// <summary>
    /// Generates the map based on the loaded data.
    /// </summary>
    public void GenerateMap()
    {
        Vector3 basePosition = transform.position;  // Position of the GameObject this script is attached to

        // Remove and recreate the "map" parent GameObject
        Transform mapParent = transform.Find("map");
        if (mapParent != null)
        {
            DestroyImmediate(mapParent.gameObject);
        }
        GameObject mapObject = new GameObject("map");
        mapObject.transform.SetParent(transform);
        mapParent = mapObject.transform;

        for (int y = 0; y < map.Length; y++)
        {
            string row = map[y];
            for (int x = 0; x < row.Length; x++)
            {
                char cell = row[x];
                Vector3 position = basePosition + new Vector3(x * tileWidth, 0, -y * tileHeight);

                GameObject prefab = GetPrefabForCharacter(cell);
                if (prefab != null)
                {
                    Instantiate(prefab, position, Quaternion.identity, mapParent);
                }
                else
                {
                    Debug.LogWarning($"No prefab found for character '{cell}'");
                }
            }
        }
    }

    /// <summary>
    /// Gets the prefab for a given character from the charactersPrefab GameObject.
    /// </summary>
    /// <param name="character">The character to get the prefab for.</param>
    /// <returns>The prefab GameObject, or null if not found.</returns>
    GameObject GetPrefabForCharacter(char character)
    {
        if (charactersPrefab == null)
        {
            Debug.LogError("charactersPrefab is not set.");
            return null;
        }

        string prefabName = "chr-" + character;
        Transform prefabTransform = FindInChildren(charactersPrefab.transform, prefabName);

        if (prefabTransform != null)
        {
            // Check for ReferencePrefab script and return its referencePrefab if available
            DunGenRef.DunGenRef referencePrefab = prefabTransform.GetComponent<DunGenRef.DunGenRef>();
            if (referencePrefab != null)
            {
                return referencePrefab.prefab;
            }
            return prefabTransform.gameObject;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Recursively searches for a Transform with the specified name in the hierarchy.
    /// </summary>
    /// <param name="parent">The parent Transform to start the search from.</param>
    /// <param name="name">The name of the Transform to find.</param>
    /// <returns>The Transform if found, or null if not found.</returns>
    Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            Transform result = FindInChildren(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the bounds of the prefab to determine its size.
    /// </summary>
    /// <param name="prefab">The prefab GameObject.</param>
    /// <returns>The size of the prefab as a Vector3.</returns>
    Vector3 GetPrefabBounds(GameObject prefab)
    {
        Vector3 boundsSize = Vector3.zero;
        MeshFilter meshFilter = prefab.GetComponent<MeshFilter>();
        Collider collider = prefab.GetComponent<Collider>();

        if (meshFilter != null)
        {
            boundsSize = meshFilter.sharedMesh.bounds.size;
        }
        else if (collider != null)
        {
            boundsSize = collider.bounds.size;
        }
        else
        {
            Debug.LogError("The prefab does not have a MeshFilter or Collider component to determine bounds.");
        }

        return boundsSize;
    }
}
