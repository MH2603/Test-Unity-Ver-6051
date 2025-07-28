using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class RenderingTexture : MonoBehaviour
{

    public RenderTexture renderTexture;
    [Range(0, 2)]
    public float ScaleRender = 1;
    float oldScaleRender = 22;
    public Camera Cam;



    private void Start()
    {
        renderTexture = new RenderTexture(Mathf.RoundToInt(Screen.width * ScaleRender), Mathf.RoundToInt(Screen.height * ScaleRender), 16);

        //   renderTexture.isPowerOfTwo = false;
        Cam.targetTexture = renderTexture;
        Cam.depth = -10; // force draw earlier than main camera
        oldScaleRender = ScaleRender;
    }
    void Update()
    {

        

        if (Cam == null)
        {

            Cam = GetComponent<Camera>();
        }



        if(ScaleRender != oldScaleRender) { 
         renderTexture = new RenderTexture(Mathf.RoundToInt(Screen.width * ScaleRender), Mathf.RoundToInt(Screen.height * ScaleRender), 24);

     //   renderTexture.isPowerOfTwo = false;
        Cam.targetTexture = renderTexture;
            Cam.depth = -10; // force draw earlier than main camera
            oldScaleRender = ScaleRender;
        }



        Shader.SetGlobalTexture("RenderTexDiamond", renderTexture);




    //   RenderTexture.ReleaseTemporary(renderTexture);

        /*
        if (RT == null)
        {
            RT = gameObject.GetComponent<Camera>().targetTexture;
        }

        RT.width = Mathf.RoundToInt(Screen.width * ScaleRender);
        RT.height = Mathf.RoundToInt(Screen.height * ScaleRender);
        */
    }
}
