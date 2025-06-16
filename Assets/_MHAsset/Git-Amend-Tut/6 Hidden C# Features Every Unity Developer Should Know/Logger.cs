using System.Runtime.CompilerServices;
using UnityEngine;
using System.IO;

namespace MH.Git_Amend
{
    public interface ILogger
    {
        public void Log(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Debug.Log($"[{Path.GetFileName(filePath)}:{lineNumber} - {memberName}] {message}");
        }
    }
    
    
    public class UnityLogger : ILogger
    {
        
    }
}