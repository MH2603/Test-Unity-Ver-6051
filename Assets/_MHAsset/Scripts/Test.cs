using UnityEngine;

public class Test : MonoBehaviour
{
    public float speed = 5f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"{speed:0.0}");
    }
}
