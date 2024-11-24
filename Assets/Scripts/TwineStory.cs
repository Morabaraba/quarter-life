using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Namespace for TextMeshPro

/// <summary>
/// A MonoBehaviour class for handling and displaying Twine stories in Unity.
/// </summary>
public class TwineStory : MonoBehaviour
{
    public string filePath;
    public GameObject textObject; // Generic text object, assign in the Inspector
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
    public bool addBackChoice = true; // Add a back choice to set current passage to start passage
    public string promptPrefix = "Choices:\n"; // Prefix for choice prompts
    private List<string> choices = new List<string>(); // List to store passage choices
    private TwineTweeParser.TwineTweeParser parser = new TwineTweeParser.TwineTweeParser(); // parser to help with scripts and choices

    // Cache for text components
    private Text uiText;
    private TextMesh textMesh;
    private TMP_Text tmpText;

    void Start()
    {        
        parser.ParseFile(filePath, out storyTitle, out startPassage, out passages);
        Debug.Log($"Available Passage Keys: [{string.Join(", ", passages.Keys)}]");    
        currentPassage = startPassage; // Initialize with start passage

        // Determine what type of text component is on the textObject
        if (textObject != null)
        {
            uiText = textObject.GetComponent<Text>();
            textMesh = textObject.GetComponent<TextMesh>();
            tmpText = textObject.GetComponent<TMP_Text>();
            
            if (uiText != null)
            {
                uiText.gameObject.SetActive(false); // Hide the text at the start
            }
            else if (textMesh != null)
            {
                textMesh.gameObject.SetActive(false); // Hide the text at the start
            }
            else if (tmpText != null)
            {
                tmpText.gameObject.SetActive(false); // Hide the text at the start
            }
            else
            {
                Debug.LogError("No compatible text component found on textObject.");
            }
        }
        else
        {
            Debug.LogError("TextObject not assigned.");
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

            if (isTextDisplayed && displayTime > 0 && Time.time >= displayStartTime + displayTime)
            {
                if (distance > detectionRadius)
                {
                    HideText();
                    isTextDisplayed = false;
                    if (resetToStartPassage)
                    {
                        currentPassage = startPassage;
                    }
                }
                else if (string.IsNullOrEmpty(interactiveKey) || Input.GetKey(interactiveKey))
                {
                    // Allow re-interaction when within detection radius and key is pressed again
                    HideText();
                    isTextDisplayed = false;
                    if (resetToStartPassage)
                    {
                        currentPassage = startPassage;
                    }
                }
            }
            else if (displayTime == 0 && isKeyPressed && isTextDisplayed)
            {
                HideText();
                isTextDisplayed = false;
            }
            else if (displayTime == 0 && isKeyPressed && !isTextDisplayed)
            {
                ShowCurrentPassage();
            }
        }
    }

    /// <summary>
    /// Displays the current passage text in the UI.
    /// </summary>
    void ShowCurrentPassage()
    {
        if (textObject != null && passages.TryGetValue(currentPassage, out TwineTweeParser.TwineTweeParser.PassageData passageData))
        {
            // Process passageText to show choices in blue
            string processedText = Regex.Replace(passageData.Text,  @"\[\[(.*?)\]\]", match => $"<color=blue>{match.Groups[1].Value}</color>");

            // Extract and build choices
            choices = ExtractChoices(passageData.Text + passageData.Div);

            // Append choices to display text only if there are choices
            if (choices.Count > 0)
            {
                processedText += $"\n{promptPrefix}"; 
                for (int i = 0; i < choices.Count; i++)
                {
                    processedText += $"{i + 1}. <color=blue>{choices[i]}</color>\n";
                }
            }
            // Add a "Back" choice if enabled
            Debug.Log($"addBackChoice: [{addBackChoice}] && [{currentPassage} != {startPassage} = {currentPassage != startPassage}]");
            if (addBackChoice && currentPassage != startPassage)
            {
                if (choices.Count == 0) { // if no prev choiced add the prompt
                    processedText += $"\n{promptPrefix}"; 
                }
                processedText += $"{choices.Count + 1}. <color=blue>Back</color>\n";
                choices.Add(startPassage);
            }

            DisplayText(processedText);

            isTextDisplayed = true;
            displayStartTime = Time.time; // Record the time the text is displayed

            Debug.Log($"Trigger [{currentPassage}] Passage [{processedText}] at [{displayStartTime}]");

            // Print script to debug log if it exists
            if (!string.IsNullOrEmpty(passageData.Script))
            {
                Debug.Log($"Script: {passageData.Script}");
                TwineScriptParser parser = new TwineScriptParser();
                parser.ParseAndExecute(passageData.Script);
            }
        }
        else
        {
            Debug.LogError($"Trigger [{currentPassage}] Passage not found for key. Available keys: [{string.Join(", ", passages.Keys)}]");
        }
    }

    /// <summary>
    /// Extracts choices from the given passage text.
    /// </summary>
    /// <param name="text">The passage text to extract choices from.</param>
    /// <returns>A list of choices extracted from the text.</returns>
    List<string> ExtractChoices(string text)
    {
        return parser.ExtractChoices(text);
    }

    /// <summary>
    /// Displays the text on the appropriate text component.
    /// </summary>
    /// <param name="text">The text to display.</param>
    void DisplayText(string text)
    {
        if (uiText != null)
        {
            uiText.text = text;
            uiText.gameObject.SetActive(true);
        }
        else if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.gameObject.SetActive(true);
        }
        else if (tmpText != null)
        {
            tmpText.text = text;
            tmpText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the text on the appropriate text component.
    /// </summary>
    void HideText()
    {
        if (uiText != null)
        {
            uiText.gameObject.SetActive(false);
        }
        else if (textMesh != null)
        {
            textMesh.gameObject.SetActive(false);
        }
        else if (tmpText != null)
        {
            tmpText.gameObject.SetActive(false);
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