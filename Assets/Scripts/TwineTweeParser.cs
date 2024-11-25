using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking; // Add this namespace for UnityWebRequest

namespace TwineTweeParser
{

    /// <summary>
    /// A parser for the Twee format used in Twine stories.
    /// </summary>
    public class TwineTweeParser
    {
        private string[] lines = new string[0];
        private enum ParsingState
        {
            None,
            StoryTitle,
            StoryData,
            Passage,
            ScriptTag,
            DivTag
        }

        /// <summary>
        /// A class to store passage data, including text and script.
        /// </summary>
        public class PassageData
        {
            public string Text { get; set; }
            public string Script { get; set; }
            public string Div { get; set; }
        }
        /// <summary>
        /// Parses a Twee file and extracts story metadata and passages.
        /// </summary>
        /// <param name="fileName">The filename to the Twee file.</param>
        public IEnumerator LoadFile(string fileName)
        {
            string filePath = Application.streamingAssetsPath + "/" + fileName;

            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                UnityWebRequest www = UnityWebRequest.Get(filePath);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("TwineTweeParser WWW filePath: [" + filePath + "] not found.");
                    lines = new string[0];
                }
                else
                {
                    string linesData = www.downloadHandler.text;
                    lines = linesData.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    lines = File.ReadAllLines(filePath);
                }
                else
                {
                    Debug.LogError("TwineTweeParser FILE filePath: [" + filePath + "] not found.");
                    lines = new string[0];
                }
            }
        }

        /// <summary>
        /// Parses a Twee file and extracts story metadata and passages.
        /// </summary>
        /// <param name="filePath">The path to the Twee file.</param>
        /// <param name="storyTitle">Outputs the story title.</param>
        /// <param name="startPassage">Outputs the start passage name.</param>
        /// <param name="passages">Outputs a dictionary of passage names and contents.</param>
        public void ParseFile(out string storyTitle, out string startPassage, out Dictionary<string, PassageData> passages)
        {
            storyTitle = string.Empty;
            startPassage = string.Empty;
            passages = new Dictionary<string, PassageData>();
            try
            {
                ParsingState currentState = ParsingState.None;
                string currentPassageName = null;
                string currentPassageText = "";
                string currentScript = "";
                string currentDiv = "";

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
                                Script = currentScript.Trim(),
                                Div = currentDiv.Trim()
                            };
                            Debug.Log($"Parsed Passage - Name: {currentPassageName}, Content: {currentPassageText.Trim()}, Script: {currentScript.Trim()}, Div: {currentDiv.Trim()}");
                        }

                        int nameEndIndex = line.IndexOf('{');
                        currentPassageName = (nameEndIndex > -1) ? line.Substring(3, nameEndIndex - 3).Trim() : line.Substring(3).Trim();
                        currentPassageText = "";
                        currentScript = "";
                        currentDiv = "";
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
                                currentState = ParsingState.ScriptTag;
                                currentScript += CleanScriptTag(line) + "\n";
                            }
                            else if (line.Contains("<div"))
                            {
                                currentState = ParsingState.DivTag;
                                currentDiv += CleanDivTag(line) + "\n";
                            }
                            else
                            {
                                currentPassageText += line + "\n";
                            }
                            break;

                        case ParsingState.ScriptTag:
                            if (line.Contains("</script>"))
                            {
                                currentScript += CleanScriptTag(line) + "\n";
                                currentState = ParsingState.Passage;
                            }
                            else
                            {
                                currentScript += line + "\n";
                            }
                            break;

                        case ParsingState.DivTag:
                            if (line.Contains("</div>"))
                            {
                                currentDiv += CleanDivTag(line) + "\n";
                                currentState = ParsingState.Passage;
                            }
                            else
                            {
                                currentDiv += line + "\n";
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
                        Script = currentScript.Trim(),
                        Div = currentDiv.Trim()
                    };
                    Debug.Log($"Parsed Passage - Name: {currentPassageName}, Content: {currentPassageText.Trim()}, Script: {currentScript.Trim()}, Div: {currentDiv.Trim()}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load or parse the Twine file: {e.Message}");
            }
        }

        /// <summary>
        /// Extracts choices from the given passage text.
        /// </summary>
        /// <param name="text">The passage text to extract choices from.</param>
        /// <returns>A list of choices extracted from the text.</returns>
        public List<string> ExtractChoices(string text)
        {
            var choiceList = new List<string>();
            var matches = Regex.Matches(text, @"\[\[(.*?)\]\]");
            foreach (Match match in matches)
            {
                choiceList.Add(match.Groups[1].Value);
            }
            return choiceList;
        }
        
        /// <summary>
        /// Cleans a line by removing the <script> or </script> tag and its attributes.
        /// </summary>
        /// <param name="line">The line to clean.</param>
        /// <returns>The cleaned line.</returns>
        private string CleanScriptTag(string line)
        {
            string pattern = @"<script.*?>|</script>";
            return Regex.Replace(line, pattern, "").Trim();
        }

        /// <summary>
        /// Cleans a line by removing the <div> or </div> tag and its attributes.
        /// </summary>
        /// <param name="line">The line to clean.</param>
        /// <returns>The cleaned line.</returns>
        private string CleanDivTag(string line)
        {
            string pattern = @"<div.*?>|</div>";
            return Regex.Replace(line, pattern, "").Trim();
        }
    }
}
