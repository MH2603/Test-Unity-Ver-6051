using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Imagine.WebAR.Editor
{
    public class PostProcessBuild_HT : MonoBehaviour
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            // var htmlLines = File.ReadAllLines(buildPath + "/index.html");
            var htmlText = File.ReadAllText(buildPath + "/index.html");

            if(HandTrackerGlobalSettings.Instance.dontOverrideHandCount){
                Debug.Log("Keeping numHands variable in index.html");
            }
            else{
                int handCount = HandTrackerGlobalSettings.Instance.maxHands;
                Debug.Log("Setting maxHands to " + handCount);
                htmlText = Regex.Replace(htmlText, @"var\s*numHands\s*=\s*\d+;", "var numHands = " + handCount + ";");
            }

            if(HandTrackerGlobalSettings.Instance.dontOverrideConfidence){
                Debug.Log("Keeping confidence variables in index.html");
            }
            else{
                float detectConfidence = HandTrackerGlobalSettings.Instance.detectConfidence;
                float presenceConfidence = HandTrackerGlobalSettings.Instance.presenceConfidence;
                float trackConfidence = HandTrackerGlobalSettings.Instance.trackConfidence;

                Debug.LogFormat("Setting confidences to {0},{1},{2}", detectConfidence, presenceConfidence, trackConfidence);
                htmlText = Regex.Replace(htmlText, @"[\d\.]+(?=,\s*//detectConfidence)", HandTrackerGlobalSettings.Instance.detectConfidence + ", //detectConfidence");
                htmlText = Regex.Replace(htmlText, @"[\d\.]+(?=,\s*//presenceConfidence)", HandTrackerGlobalSettings.Instance.presenceConfidence + ", //presenceConfidence");
                htmlText = Regex.Replace(htmlText, @"[\d\.]+(?=,\s*//trackConfidence)", HandTrackerGlobalSettings.Instance.trackConfidence + ", //trackConfidence");
            }
            
            File.WriteAllText(buildPath + "/index.html", htmlText);
        }
    }
}

