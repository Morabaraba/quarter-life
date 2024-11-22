using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TwineTweeParser
{
    /// <summary>
    /// A parser for the Twee format used in Twine stories.
    /// </summary>
    public class TwineTweeParser
    {
        private enum ParsingState
        {
            None,
            StoryTitle,
            StoryData,
            Passage
        }

        /// <summary>
        /// A class to store passage data, including text and script.
        /// </summary>
        public class PassageData
        {
            public string Text { get; set; }
            public string Script { get; set; }
        }

        /// <summary>
        /// Parses a Twee file and extracts story metadata and passages.
        /// </summary>
        /// <param name="filePath">The path to the Twee file.</param>
        /// <param name="storyTitle">Outputs the story title.</param>
        /// <param name="startPassage">Outputs the start passage name.</param>
        /// <param name="passages">Outputs a dictionary of passage names and contents.</param>
        public void ParseFile(string filePath, out string storyTitle, out string startPassage, out Dictionary<string, PassageData> passages)
        {
            storyTitle = string.Empty;
            startPassage = string.Empty;
            passages = new Dictionary<string, PassageData>();

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                ParsingState currentState = ParsingState.None;
                string currentPassageName = null;
                string currentPassageText = "";
                string currentScript = "";

                foreach (string line in lines)
                {
                    Debug.Log($"line [{line}]");

                    if (line.StartsWith(":: "))
                    {
                        if (currentPassageName != null && currentState == ParsingState.Passage)
                        {
                            passages[currentPassageName] = new PassageData
                            {
                                Text = currentPassageText.Trim(),
                                Script = currentScript.Trim()
                            };
                            Debug.Log($"Parsed Passage - Name: {currentPassageName}, Content: {currentPassageText.Trim()}, Script: {currentScript.Trim()}");
                        }

                        int nameEndIndex = line.IndexOf('{');
                        currentPassageName = (nameEndIndex > -1) ? line.Substring(3, nameEndIndex - 3).Trim() : line.Substring(3).Trim();
                        currentPassageText = "";
                        currentScript = "";
                        Debug.Log($"currentPassageName [{currentPassageName}]");

                        currentState = currentPassageName switch
                        {
                            "StoryTitle" => ParsingState.StoryTitle,
                            "StoryData" => ParsingState.StoryData,
                            _ => ParsingState.Passage
                        };

                        continue;
                    }

                    switch (currentState)
                    {
                        case ParsingState.StoryTitle:
                            storyTitle = line.Trim();
                            Debug.Log($"Story Title: {storyTitle}");
                            currentState = ParsingState.None;
                            break;

                        case ParsingState.StoryData:
                            currentPassageText += line;
                            if (line.Contains("}")) // End of JSON object
                            {
                                var storyData = JsonUtility.FromJson<TwineStory.StoryData>(currentPassageText.Trim());
                                startPassage = storyData.start;
                                Debug.Log($"Start Passage: {startPassage}");
                                currentState = ParsingState.None;
                            }
                            break;

                        case ParsingState.Passage:
                            if (line.Contains("<script"))
                            {
                                currentScript += line + "\n";
                            }
                            else if (line.Contains("</script>"))
                            {
                                currentScript += line + "\n";
                                currentState = ParsingState.Passage; // Continue collecting passage text after script block
                            }
                            else if (currentScript.Length > 0)
                            {
                                currentScript += line + "\n"; // Continue collecting script content
                            }
                            else
                            {
                                currentPassageText += line + "\n";
                            }
                            break;

                        case ParsingState.None:
                        default:
                            break;
                    }
                }

                // Ensure the last passage is added
                if (currentPassageName != null && currentState == ParsingState.Passage)
                {
                    passages[currentPassageName] = new PassageData
                    {
                        Text = currentPassageText.Trim(),
                        Script = currentScript.Trim()
                    };
                    Debug.Log($"Parsed Passage - Name: {currentPassageName}, Content: {currentPassageText.Trim()}, Script: {currentScript.Trim()}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load or parse the Twine file: {e.Message}");
            }
        }
    }
}
