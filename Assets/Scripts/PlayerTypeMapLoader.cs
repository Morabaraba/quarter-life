using System.IO;
using UnityEngine;

public class PlayerTypeMapLoader : MonoBehaviour
{
    /// <summary>
    /// Path to the default map file.
    /// </summary>
    public string defaultMapFile = "map1.txt"; // Default map file path

    /// <summary>
    /// Path to the wizard map file.
    /// </summary>
    public string wizardMapFile = "map1-wizard.txt"; // Wizard map file path

    /// <summary>
    /// Path to the knight map file.
    /// </summary>
    public string knightMapFile = "map1-knight.txt"; // Knight map file path

    /// <summary>
    /// Reference to the DunGenMap component.
    /// </summary>
    private DunGenMap dunGenMap;

    /// <summary>
    /// Time interval in milliseconds to check for player type change.
    /// </summary>
    public float changeTimer = 0.5f; // Change to your desired interval

    private string previousPlayerType;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    void Start()
    {
        dunGenMap = GetComponent<DunGenMap>();
        if (dunGenMap != null)
        {
            previousPlayerType = PlayerPrefs.GetString("PlayerType", "");
            LoadAppropriateMap(previousPlayerType);
            InvokeRepeating("CheckPlayerType", 0, changeTimer);
        }
        else
        {
            Debug.LogError("DunGenMap component not found on the GameObject.");
        }
    }

    /// <summary>
    /// Checks the player's type and loads the appropriate map file.
    /// </summary>
    private void LoadAppropriateMap(string playerType)
    {
        if (playerType == "wizard")
        {
            dunGenMap.mapFile = wizardMapFile;
        }
        else if (playerType == "knight")
        {
            dunGenMap.mapFile = knightMapFile;
        }
        else
        {
            dunGenMap.mapFile = defaultMapFile;
        }
        StartCoroutine(dunGenMap.StartLoading());
    }

    /// <summary>
    /// Continuously checks for player type change.
    /// </summary>
    private void CheckPlayerType()
    {
        string currentPlayerType = PlayerPrefs.GetString("PlayerType", "");
        if (currentPlayerType != previousPlayerType)
        {
            previousPlayerType = currentPlayerType;
            LoadAppropriateMap(currentPlayerType);
        }
    }
}
