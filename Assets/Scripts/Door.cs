using System.Collections;
using UnityEngine;

/// <summary>
/// Represents a door that can be opened by an interactive key or automatically based on a key color.
/// </summary>
public class Door : MonoBehaviour
{
    /// <summary>
    /// The key used to interact with the door.
    /// </summary>
    public string interactiveKey = "e"; // Default interaction key

    /// <summary>
    /// The color of the key required to open the door. If not set, any key can open it.
    /// </summary>
    public string doorKeyColor = ""; // Door key color for PlayerPref check, if any

    /// <summary>
    /// The radius within which the player can interact with the door.
    /// </summary>
    public float radius = 1.0f; // Interaction radius

    /// <summary>
    /// The distance the door will slide when opened.
    /// </summary>
    public float slideDistance = 2.0f; // Distance to slide the door

    /// <summary>
    /// The time it takes for the door to slide open.
    /// </summary>
    public float slideTime = 1.0f; // Time to slide the door

    /// <summary>
    /// Optional player prefab to assign manually. If not set, the script will search for a GameObject tagged "Player".
    /// </summary>
    public GameObject playerPrefab; // Public player prefab

    /// <summary>
    /// Whether the door should operate automatically based on the key color.
    /// </summary>
    public bool operateOnKeyColor = false; // Operate based on door key color

    /// <summary>
    /// The interval at which to check the key color condition (in seconds).
    /// </summary>
    public float checkOperationEvery = 0.5f; // Check operation interval

    private bool isOpen = false; // Indicates whether the door is open
    private Transform player; // The player's transform
    private float checkTimer = 0; // Timer to track interval checks

    /// <summary>
    /// Initializes the door, finding the player either by assigned prefab or by searching for a GameObject tagged "Player".
    /// </summary>
    void Start()
    {
        InitializePlayer();
    }

    /// <summary>
    /// Initializes the player transform.
    /// </summary>
    void InitializePlayer()
    {
        if (playerPrefab != null)
        {
            player = playerPrefab.transform;
            Debug.Log($"[{gameObject.name}] Player prefab assigned.");
        }
        else
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                Debug.Log($"[{gameObject.name}] Player found with tag 'Player'.");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] Player not found. Please assign a player prefab or tag the player GameObject with 'Player'.");
            }
        }
    }

    /// <summary>
    /// Checks if the player is within interaction radius and if the interactive key is pressed to open the door.
    /// Also checks the key color condition at regular intervals if operateOnKeyColor is true.
    /// </summary>
    void Update()
    {
        if (player != null)
        {
            CheckPlayerDistance();
            
            // Check key color condition at regular intervals
            if (operateOnKeyColor)
            {
                checkTimer += Time.deltaTime;
                if (checkTimer >= checkOperationEvery)
                {
                    checkTimer = 0;
                    HandleKeyColorCondition();
                }
            }
        }
    }

    /// <summary>
    /// Checks if the player is within interaction radius and handles door opening based on interactive key press.
    /// </summary>
    void CheckPlayerDistance()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        bool isKeyPressed = Input.GetKeyDown(interactiveKey);

        if (distance <= radius && isKeyPressed && !isOpen)
        {
            if (ShouldOpenDoor())
            {
                Debug.Log($"[{gameObject.name}] Opening door.");
                StartCoroutine(SlideDoorUp());
            }
            else
            {
                Debug.Log($"[{gameObject.name}] Door key color condition not met.");
            }
        }
    }

    /// <summary>
    /// Determines if the door should open based on the key color condition.
    /// </summary>
    /// <returns>True if the door should open, false otherwise.</returns>
    bool ShouldOpenDoor()
    {
        return string.IsNullOrEmpty(doorKeyColor) || PlayerPrefs.GetInt($"doorKeyColor.{doorKeyColor}", 0) == 1;
    }

    /// <summary>
    /// Handles the key color condition and opens or closes the door accordingly.
    /// </summary>
    void HandleKeyColorCondition()
    {   
        int doorKeyColorVal = PlayerPrefs.GetInt($"doorKeyColor.{doorKeyColor}", 0);
        Debug.Log($"[{gameObject.name}] Door [doorKeyColor.{doorKeyColor} = {doorKeyColorVal}]");
        if ( doorKeyColorVal == 1 && !isOpen)
        {
            Debug.Log($"[{gameObject.name}] SlideDoorUp [doorKeyColor.{doorKeyColor} = {doorKeyColorVal}]");
            StartCoroutine(SlideDoorUp());
        }
        else if (doorKeyColorVal == 0 && isOpen)
        {
            Debug.Log($"[{gameObject.name}] SlideDoorDown [doorKeyColor.{doorKeyColor} = {doorKeyColorVal}]");
            StartCoroutine(SlideDoorDown());
        }
    } 

    /// <summary>
    /// Slides the door up over a specified time period.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    IEnumerator SlideDoorUp()
    {
        isOpen = true;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + new Vector3(0, slideDistance, 0);
        float elapsedTime = 0;

        while (elapsedTime < slideTime)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / slideTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        Debug.Log($"[{gameObject.name}] Door fully opened.");
    }

    /// <summary>
    /// Slides the door down over a specified time period.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    IEnumerator SlideDoorDown()
    {
        isOpen = false;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition - new Vector3(0, slideDistance, 0);
        float elapsedTime = 0;

        while (elapsedTime < slideTime)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / slideTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        Debug.Log($"[{gameObject.name}] Door fully closed.");
    }
}
