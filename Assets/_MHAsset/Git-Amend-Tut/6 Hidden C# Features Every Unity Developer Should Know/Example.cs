using System.IO;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace MH.Git_Amend
{
    public class Example : MonoBehaviour
    {
        ILogger logger = new UnityLogger();
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Log("Hellow World!");
            //logger.Log("This is a log message from the logger interface.");
            
            ShowFormatting(-5);
        }
        
        void ShowFormatting(int value)
        {
            string format = "##;(##);**Zero**";
            string result = value.ToString(format);
            logger.Log($"Input: {value} -> Formatted: {result}");
        }

        public void Log(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Debug.Log($"[{Path.GetFileName(filePath)}:{lineNumber} - {memberName}] {message}");
        }

    }

}