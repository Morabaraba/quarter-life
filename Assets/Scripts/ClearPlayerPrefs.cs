using UnityEngine;

public class ClearPlayerPrefs : MonoBehaviour
{
    void Start()
    {
        // Clear all PlayerPrefs values when the scene starts
        PlayerPrefs.DeleteAll();
    }
}
