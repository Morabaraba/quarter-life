using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TwineTweeParser;

/// <summary>
/// A MonoBehaviour class for handling and displaying Twine stories in Unity.
/// </summary>
public class TwineStory : MonoBehaviour
{
    public string filePath;
    public Text uiText; // Assign this in the Inspector
    public GameObject player; // Assign the player GameObject in the Inspector
    public string storyTitle;
    public string startPassage;
    public Dictionary<string, TwineTweeParser.TwineTweeParser.PassageData> passages = new Dictionary<string, TwineTweeParser.TwineTweeParser.PassageData>();
    public string currentPassage;
    public float displayTime = 5.0f; // Minimum display time in seconds
    private bool isTextDisplayed = false;
    private float displayStartTime;
    public float detectionRadius = 1.0f; // Radius to check for player proximity
    public string interactiveKey = "e"; // Default interaction key
    public bool resetToStartPassage = true; // Reset to start passage when text is hidden
    public bool useInteractiveKeyWithChoices = true; // Choices to change current passage will only happen if interactive key + choice is pressed down
    private List<string> choices = new List<string>(); // List to store passage choices

    void Start()
    {
        TwineTweeParser.TwineTweeParser parser = new TwineTweeParser.TwineTweeParser();
        parser.ParseFile(filePath, out storyTitle, out startPassage, out passages);
        Debug.Log($"Available Passage Keys: [{string.Join(", ", passages.Keys)}]");    
        currentPassage = startPassage; // Initialize with start passage
        if (uiText != null)
        {
            uiText.gameObject.SetActive(false); // Hide the text at the start
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            bool isKeyPressed = string.IsNullOrEmpty(interactiveKey) || Input.GetKey(interactiveKey);

            if (distance <= detectionRadius && isKeyPressed && !isTextDisplayed)
            {
                ShowCurrentPassage();
            }

            // Always listen for number key presses to navigate to specific passages
            for (int i = 1; i <= choices.Count; i++)
            {
                if (Input.GetKeyDown(i.ToString()))
                {   
                    if (useInteractiveKeyWithChoices && !isKeyPressed) {
                        continue;
                    }
                    string choiceKey = choices[i - 1];
                    Debug.Log($"Interactive Key Pressed: [{interactiveKey}], Number Key Pressed: [{i}], Choice: [{choiceKey}]");                    
                    if (passages.ContainsKey(choiceKey))
                    {
                        Debug.Log($"Choice Key Matched: {choiceKey}");
                        currentPassage = choiceKey;
                        ShowCurrentPassage();
                        break;
                    }
                    else
                    {
                        Debug.Log($"Choice Key did NOT Match [{choiceKey}]. Available keys: [{string.Join(", ", passages.Keys)}]");
                    }
                }
            }

            if (isTextDisplayed && Time.time >= displayStartTime + displayTime)
            {
                if (distance > detectionRadius)
                {
                    uiText.gameObject.SetActive(false); // Hide the text when the player moves away
                    isTextDisplayed = false;
                    if (resetToStartPassage)
                    {
                        currentPassage = startPassage;
                    }
                }
                else if (string.IsNullOrEmpty(interactiveKey) || Input.GetKey(interactiveKey))
                {
                    // Allow re-interaction when within detection radius and key is pressed again
                    uiText.gameObject.SetActive(false);
                    isTextDisplayed = false;
                    if (resetToStartPassage)
                    {
                        currentPassage = startPassage;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Displays the current passage text in the UI.
    /// </summary>
    void ShowCurrentPassage()
    {
        if (uiText != null && passages.TryGetValue(currentPassage, out TwineTweeParser.TwineTweeParser.PassageData passageData))
        {
            // Process passageText to show choices in blue
            string processedText = Regex.Replace(passageData.Text, @"\[\[(.*?)\]\]", match => $"<color=blue>{match.Groups[1].Value}</color>");

            // Extract choices
            var matches = Regex.Matches(passageData.Text, @"\[\[(.*?)\]\]");
            choices.Clear();
            foreach (Match match in matches)
            {
                choices.Add(match.Groups[1].Value);
            }

            // Append choices to display text only if there are choices
            if (choices.Count > 0)
            {
                processedText += "\nChoices:\n";
                for (int i = 0; i < choices.Count; i++)
                {
                    processedText += $"{i + 1}. <color=blue>{choices[i]}</color>\n";
                }
            }

            uiText.text = processedText;
            uiText.gameObject.SetActive(true); // Show the text
            isTextDisplayed = true;
            displayStartTime = Time.time; // Record the time the text is displayed

            Debug.Log($"Trigger [{currentPassage}] Passage [{processedText}] at [{displayStartTime}]");

            // Print script to debug log if it exists
            if (!string.IsNullOrEmpty(passageData.Script))
            {
                Debug.Log($"Script: {passageData.Script}");
            }
        }
        else
        {
            Debug.LogError($"Trigger [{currentPassage}] Passage not found for key. Available keys: [{string.Join(", ", passages.Keys)}]");
        }
    }

    /// <summary>
    /// A data structure for storing Twine story metadata.
    /// </summary>
    [Serializable]
    public class StoryData
    {
        public string ifid;
        public string format;
        public string formatVersion;
        public string start;
        public int zoom;
    }
}
