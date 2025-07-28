using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteInEditMode]
public class FlaresPostEffect : MonoBehaviour
{
    [Header("ToneMapping")]
    public float PostExposure = 1.2f;
    public float Contrast = 1.05f;
    [Range(0, 1)]
    public float Disaturate = 0.2f;
    [Range(-1, 1)]
    public float Min = -0.05f;
    [Range(0.5f, 1.0f)]
    public float Max = 0.9f;
    [Range(0, 10)]
    public float Saturation = 0.9f;
    [Space(15)]
    [Header("Flares")]
    public bool Flares;
    [Range(0, 3)]
    public float BlurAmount = 1;

    [Range(1, 64)]
    public int QualityFlares = 16;
    // public float BlurDistance = 200;
    [Range(0.5f, 2)]
    public float FlaresRange = 0.93f;

    [Range(0, 100)]
    public int FlareOffsetCount = 30;
    
    [Range(0, 20)]
    public float FlareIntensity = 2;


    [Space(40)]
    
    public bool OnLayer2Flares = false;
    [Header("Flares Layer 2")]
    [Range(0, 3)]
    public float BlurAmount2 = 1;


    // public float BlurDistance = 200;


    [Range(0, 100)]
    public int FlareOffsetCount2 = 30;

    [Range(0, 20)]
    public float FlareIntensity2 = 2;

    public float Ylevel = 0;

    [Space(10)]
    [Header("Vignette")]
    [Range(0, 2)]
    public float VignetteIntensity = 0.5f;


    //    public Transform Point1;

    //   public Transform Point2;

    //   public Transform Point3;

    public Material material;

    // public bool InfoLine;
    int boolInt;
    Camera Cam;



    

    RenderTexture BlumTex;

    public ComputeShader _ComputeShader;

    public ComputeShader _ComputeShaderCleaning;

    public RenderTexture ScreenRender;

    public RenderTexture ScreenRender2;

    int Width;
    int Hight;
    int Depth;


     bool on;
    private void Start()
    {

        on = true;
        Cam = GetComponent<Camera>();
        if (Cam.depthTextureMode == DepthTextureMode.None)
            Cam.depthTextureMode = Cam.depthTextureMode | DepthTextureMode.Depth;
#if UNITY_EDITOR
        material = (Material)AssetDatabase.LoadAssetAtPath("Assets/SuperRealisticDiamondShaders/Scripts/ToneMap.mat", typeof(Material));
#endif
    }

    void Enabled()
    {
        on = true;

        Cam = GetComponent<Camera>();
        Set();
    }

    
 public  void Set()
    {
        //    if (GetComponent<Camera>().depthTextureMode == DepthTextureMode.None)
        Cam.depthTextureMode = Cam.depthTextureMode | DepthTextureMode.Depth;

          }

    //    private Material BlurMaterial;

    // Creates a private material used to the effect
    // void Awake()
    //  {
    //      material = new Material(Shader.Find("Hidden/ToneMap"));
    //  }

    // Postprocess the image

    //  [ImageEffectOpaque]
    [ImageEffectAllowedInSceneView]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        Width = source.width;
        Hight = source.height;
        Depth = source.depth;


        //    if (intensity == 0)
        //     {
        //     Graphics.Blit(source, destination);
        //     return;
        //}






        var p = GL.GetGPUProjectionMatrix(Cam.projectionMatrix,true);


        p[2, 3] = p[3, 2] = 0.0f;
        p[3, 3] = 1.0f;
        var clipToWorld = Matrix4x4.Inverse(p * Cam.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
        material.SetMatrix("clipToWorld", clipToWorld);


        //   Matrix4x4 viewToWorld = Cam.cameraToWorldMatrix;
        //   material.SetMatrix("_viewToWorld", viewToWorld);

        //   boolInt = InfoLine ? 1 : 0;

        //    material.SetInt("InfoLine", boolInt);
        //      material.SetVector("Vector1", Point1.position);
        //      material.SetVector("Vector2", Point2.position);
        //    material.SetVector("Vector3", Point3.position);
        material.SetFloat("PostExposure", PostExposure);
        material.SetFloat("Contrast", Contrast);

        if (Flares)
        {
            material.SetInt("FlareOffsetCount", FlareOffsetCount);
            material.SetFloat("FlareIntensity", FlareIntensity);
        }
        else
        {
            material.SetInt("FlareOffsetCount", 0);
            material.SetFloat("FlareIntensity", 0);

            material.SetInt("FlareOffsetCount2", 0);
            material.SetFloat("_BlurAmount2", 0);
            material.SetFloat("FlareIntensity2", 0);
        }

        material.SetFloat("_Disaturate", Disaturate);
        material.SetFloat("Saturation", Saturation);
        material.SetFloat("_Min", Min);
        material.SetFloat("_Max", Max);
        //  material.SetFloat("BlurDistance", BlurDistance); 
        material.SetFloat("BlurRange", FlaresRange);
        material.SetInt("FepthOfField", System.Convert.ToInt32(Flares)); 
        material.SetFloat("VignetteIntensity", VignetteIntensity);
        material.SetFloat("_BlurAmount", BlurAmount); 
        material.SetFloat("Ylevel", Ylevel);
        material.SetFloat("QualityFlares", QualityFlares);

        if (OnLayer2Flares) {
            material.SetInt("FlareOffsetCount2", FlareOffsetCount2);
            material.SetFloat("_BlurAmount2", BlurAmount2);
            material.SetFloat("FlareIntensity2", FlareIntensity2);
        }
        else
        {
            material.SetInt("FlareOffsetCount2", 0);
            material.SetFloat("_BlurAmount2", 0);
            material.SetFloat("FlareIntensity2", 0);

        }


        material.SetVector("PixelSize", new Vector4(Screen.width,Screen.height,1,1));

        if (Flares) {


           // Res = ThresholdFlareMove;

            
                //  BlumTex.Release();
               
                RenderTexture.ReleaseTemporary(BlumTex);
                BlumTex = null;

                BlumTex = RenderTexture.GetTemporary(source.width / QualityFlares, source.height / QualityFlares, 0, source.format);
                
                //      var temp1 = RenderTexture.GetTemporary(Screen.width / QualityFlares, Screen.height / QualityFlares, 0, source.format);
                Graphics.Blit(source, BlumTex, material, 1);
                


            



            if (on || null == ScreenRender || source.width / QualityFlares != ScreenRender.width / QualityFlares
|| source.height / QualityFlares != ScreenRender.height / QualityFlares)
            {
                if (null != ScreenRender)
                {
                    ScreenRender.Release();
                }
                ScreenRender = new RenderTexture(source.width / QualityFlares, source.height / QualityFlares, source.depth);
                ScreenRender.enableRandomWrite = true;
                ScreenRender.Create();
                on = false;
                _ComputeShaderCleaning.SetTexture(0, "ScreenRender", ScreenRender);
            }




            if (on || null == ScreenRender2 || source.width / QualityFlares != ScreenRender2.width / QualityFlares
|| source.height / QualityFlares != ScreenRender2.height / QualityFlares)
            {
                if (null != ScreenRender2)
                {
                    ScreenRender2.Release();
                }
                ScreenRender2 = new RenderTexture(source.width / QualityFlares, source.height / QualityFlares, source.depth);
                ScreenRender2.enableRandomWrite = true;
                ScreenRender2.Create();
                on = false;
                _ComputeShaderCleaning.SetTexture(0, "ScreenRender2", ScreenRender2);
            }
            

            ScreenRender.enableRandomWrite = true;
            ScreenRender2.enableRandomWrite = true;
            _ComputeShader.SetInt("FlareOffsetCount", FlareOffsetCount);
            _ComputeShader.SetFloat("FlareIntensity", FlareIntensity);
            _ComputeShader.SetVector("uvSize", new Vector2(source.width, source.height));
            _ComputeShader.SetFloat( "_BlurAmount", BlurAmount);

            _ComputeShader.SetTexture(0, "Source", source);

            _ComputeShader.SetTexture(0, "PointsTex", BlumTex);

            _ComputeShader.SetTexture(0, "ScreenRender", ScreenRender);




            _ComputeShaderCleaning.SetInt("FlareOffsetCount", FlareOffsetCount);
            _ComputeShaderCleaning.SetFloat("FlareIntensity", FlareIntensity);
            _ComputeShaderCleaning.SetVector("uvSize", new Vector2(source.width, source.height));
            _ComputeShaderCleaning.SetFloat("_BlurAmount", BlurAmount);

            _ComputeShaderCleaning.SetTexture(0, "Source", source);

            _ComputeShaderCleaning.SetTexture(0, "PointsTex", BlumTex);

               _ComputeShaderCleaning.SetTexture(0, "ScreenRender", ScreenRender);

            //   _ComputeShaderCleaning.SetTexture(0, "ScreenRender2", ScreenRender2);

              _ComputeShaderCleaning.Dispatch(0, BlumTex.width / 8, BlumTex.height / 8, 1);



            //     _ComputeShader.Dispatch(0, source.width / 8, source.height / 8, 1);


            material.SetTexture("_BlurTex", ScreenRender);

            material.SetInt("ScreenSizeY", Screen.height);
            material.SetInt("ScreenSizeX", Screen.width);

            material.SetTexture("_BlurTex2", ScreenRender2);

            //   Graphics.Blit(temp1, BlumTex, material, 1);


            //  RenderTexture.ReleaseTemporary(temp1);

        }
        
        /*
        else if (Flares)
        {
            material.SetFloat("_BlurAmount", BlurAmount);
            material.SetFloat("BlurRange", FlaresRange);
            RenderTexture BlumTex;
            BlumTex = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, source.format);
            var temp1 = RenderTexture.GetTemporary(Screen.width / 16, Screen.height / 16, 0, source.format);
            //   var temp2 = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, source.format);
            Graphics.Blit(source, temp1, material, 1);
            Graphics.Blit(temp1, BlumTex, material, 1);
          //  Graphics.Blit(temp2, BlumTex, material, 1);

           
            RenderTexture.ReleaseTemporary(BlumTex);
            material.SetTexture("_BlurTex", BlumTex);
            RenderTexture.ReleaseTemporary(temp1);
            //RenderTexture.ReleaseTemporary(temp2);
        }
        */

        //   Graphics.Blit(ScreenRender, destination);
        Graphics.Blit(source, destination, material,0);
    }

    void OnDestroy()
    {

        if (null != ScreenRender)
        {
            ScreenRender.Release();
            ScreenRender = null;
        }
        if (null != ScreenRender2)
        {
            ScreenRender2.Release();
            ScreenRender2 = null;
        }
    }
}