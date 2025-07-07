using UnityEngine;
using System.Collections.Generic;
using Reflex.Attributes;

public class Greeter : MonoBehaviour
{
    [Inject] private readonly IEnumerable<string> _strings;
    [Inject] public Logger _logger;
    
    private void Start()
    {
        
    }

    public void Greet()
    {
        //Debug.Log(string.Join(" ", _strings)); // Should log: "Hello"
        
        _logger.Log("Hello");
    }
}