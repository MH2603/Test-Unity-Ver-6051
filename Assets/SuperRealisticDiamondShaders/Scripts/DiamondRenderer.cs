

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace DiamondRender
{

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteAlways]
    public class DiamondRenderer : MonoBehaviour
    {
        //        public JewelModel model;        
        public Color color = new Color(1,1,1,1);
        
               public Color colorRuby = new Color(1,1,1,1);
        

        [Range(0, 3)]
        public float ColorIntensity = 1.7f;

        [Range(0, 1.5f)]
        public float LightTransmission = 0.5f;

        [Range(0, 1)]
        public float ColorByDepth = 0.1f;

        
        [Range(0, 1)]
        public float RubyMist = 0.0f;

        [Range(0,10)]
        public int MaxReflection = 4;

        [Range(1, 5)]
        public float RefractiveIndex = 1.6f;
        //      public int maxReflectionCount;
         Cubemap environment;
        //        public float refractiveIndex;
        //       public float baseReflection;
        //    public bool prism;

        // public bool mergeSimilarPlanes = true;

        //    public bool changeShaders;

        public bool autoCaptureEnvironment = false;
            public bool captureEnvironmentOnSetup = false;
            public int captureEnvironmentSize = 512;


            //    public ReflectionProbe _reflectionProbe;

            public MaterialPropertyBlock block;


        /*    public Shader ShaderCubeMap;
            public Shader ShaderReflectionProbe; */

        // calculated
        [SerializeField]
      //  [HideInInspector]
        float scale;
        [SerializeField]
  //      [HideInInspector]
        Texture2D shapeTexture;
        [SerializeField]
        [HideInInspector]
        int planeCount;

        Cubemap capturedEnvironment = null;
        Material mat;

         float Time_;

        //   public Transform CentrePivot;
        //    public Transform CentrePivotDiamond2;
        //    public Transform CentrePivotDiamond3;

        //  public float dots = 0.5f;

        //  public float distRPI;
        Vector3 MinPos;

        Vector3 MaxPos;

        [HideInInspector]
        public Vector4 CentreModel;

        //   public float RPI = 0;
        // public Camera cam;

        MeshRenderer MR;

        [HideInInspector]
        public Matrix4x4 m;




        private void Start()
        {
          
            MR = GetComponent<MeshRenderer>();

            if (block == null)
            {
                block = new MaterialPropertyBlock();
                MR.GetPropertyBlock(block);
                
            }
        }

        private void Enable()
        {
           
            MR = GetComponent<MeshRenderer>();
            mat = MR.sharedMaterial;

            if (block == null)
            {
                block = new MaterialPropertyBlock();
                  MR.GetPropertyBlock(block);
               
            }
        }

        private void Update()
        {

            if(block == null)
            {
                   block = new MaterialPropertyBlock();

                     MR.GetPropertyBlock(block);
                
            }




            // if(_reflectionProbe != null)
            //       mat.SetFloat("lightEstimation2",Mathf.Clamp( _reflectionProbe.intensity,0,0.5f));


            //    if (_reflectionProbe != null)
            //    {


            //    RPI = _reflectionProbe.intensity;


            //       distRPI =  0.65f - (RPI - 0.65f);

            //      if (_reflectionProbe.intensity > 0.65f)
            //       {
            //        RPI = RPI * (1 - _reflectionProbe.intensity);
            //      }

            //  RPI = Mathf.Clamp(RPI, 0, 0.5f);

            //  mat.SetFloat("lightEstimation2", RPI + (distRPI * -1f));
            //  block.SetFloat("lightEstimation2", RPI);

            //   }


            if (MR == null)
            {
                MR = GetComponent<MeshRenderer>();
            }

            if (mat == null)
                mat = MR.sharedMaterial;

            //    point =  cam.ScreenToWorldPoint(CentrePivotDiamond3.position);




            //    mat.SetVector("poinT", point);





            //  Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);


            m = MR.worldToLocalMatrix;

            m.m03 -= CentreModel.x;
            m.m13 -= CentreModel.y;
            m.m23 -= CentreModel.z;


            //     Matrix4x4 m2 = Matrix4x4.TRS(CentrePivotDiamond2.position, CentrePivotDiamond2.rotation, CentrePivotDiamond2.localScale);

            /// Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            /// 
            

            block.SetVector("CentreModel", CentreModel);
            block.SetFloat("ColorByDepth", ColorByDepth);
                        block.SetFloat("RubyMist", RubyMist);
            block.SetColor("_Color", color); 
                        block.SetColor("colorRuby", colorRuby); 
            block.SetFloat("ColorIntensity", ColorIntensity);
            block.SetFloat("lighttransmission", LightTransmission);
            block.SetFloat("_RefractiveIndex", RefractiveIndex);
            block.SetInt("_MaxReflection", MaxReflection);
            block.SetMatrix("MatrixWorldToObject", m);

            if (shapeTexture != null) { 
            block.SetTexture("_ShapeTex", shapeTexture);
            block.SetInt("_SizeX", shapeTexture.width);
            block.SetInt("_SizeY", shapeTexture.height);
            block.SetFloat("_Scale", scale);
            block.SetInt("_PlaneCount", planeCount);
            }

            MR.SetPropertyBlock(block);

            //   mat.SetMatrix("MatrixWorldToObject2", m2);

            if (mat == null)
             mat = MR.sharedMaterial;

            //   mat.SetVector("CentrePivotDiamond", CentrePivot.position);



//   mat.SetVector("CentrePivotDiamond2", CentrePivotDiamond2.position);
/*
            if (changeShaders) { 

            if (Time_ < 2)
            {
                Time_ += Time.deltaTime / 2;
                    
                    //    ShaderCubeMap = Shader.Find( "Custom/DiamondShader");

                    //    if (ShaderCubeMap != null)
                    //     {
                    //         mat.shader = ShaderCubeMap;
                    //    }

                    //     }
                    //          else
                    //          {
                    //         ShaderReflectionProbe = Shader.Find("Custom/DiamondShaderReflectionProbe");

                    if (ShaderReflectionProbe != null)
                {
                    mat.shader = ShaderReflectionProbe;
                }
                }
            }
            */
            








            if (autoCaptureEnvironment)
            {
                CaptureEnvironment();
            }
        }


        


        [ContextMenu("Setup")]
        public void Setup()
        {
            mat = MR.sharedMaterial;


            AnalyzeMesh();

            MeshRenderer mr = GetComponent<MeshRenderer>();

            if (mat == null)
                mat = mr.sharedMaterial;

            //      Material m = new Material(Shader.Find( "Custom/DiamondShader"));

            block.SetTexture("_ShapeTex", shapeTexture);
            block.SetInt("_SizeX", shapeTexture.width);
            block.SetInt("_SizeY", shapeTexture.height);
            block.SetFloat("_Scale", scale);
            block.SetInt("_PlaneCount", planeCount);

            mr.SetPropertyBlock(block);

            //      m.SetColor("_Color", color);
            //    m.SetInt("_MaxReflection", maxReflectionCount);
            //     m.SetTexture("_Environment", environment);
            //     m.SetFloat("_RefractiveIndex", refractiveIndex);
            //     m.SetFloat("_BaseReflection", baseReflection);

            mr.material = mat;

            if (captureEnvironmentOnSetup)
            {
                CaptureEnvironment();
            }
        }

        [ContextMenu("CaptureEnvironment")]
        public void CaptureEnvironment()
        {
            Material m = GetComponent<MeshRenderer>().sharedMaterial;

            if( m == null )
            {
                Debug.LogWarning("Material is not setup yet. please do Setup first.");
                return;
            }

            if (capturedEnvironment == null)
            {
                capturedEnvironment = new Cubemap(captureEnvironmentSize, TextureFormat.ARGB32, false);
            }

            Camera cameraComponent = GetComponent<Camera>();

            bool temporaryCameraComponent = false;
            if (cameraComponent == null)
            {
                cameraComponent = gameObject.AddComponent<Camera>();
                temporaryCameraComponent = true;
            }

            cameraComponent.RenderToCubemap(capturedEnvironment);

            environment = capturedEnvironment;

            m.SetTexture("_Environment", capturedEnvironment);

#if UNITY_EDITOR
            if ( temporaryCameraComponent)
            {
                DestroyImmediate(cameraComponent);
            }
#endif
        }

        [ContextMenu("ApplyNumericParameters")]
        public void ApplyNumericParameters()
        {
            Material m = GetComponent<MeshRenderer>().sharedMaterial;




            m.SetInt("_SizeX", shapeTexture.width);
            m.SetInt("_SizeY", shapeTexture.height);
            m.SetFloat("_Scale", scale);
            m.SetInt("_PlaneCount", planeCount);
            m.SetColor("_Color", color);
            //     m.SetInt("_MaxReflection", maxReflectionCount);
            //     m.SetTexture("_Environment", environment);
            //       m.SetFloat("_RefractiveIndex", refractiveIndex);
            //       m.SetFloat("_BaseReflection", baseReflection);
        }

        bool AnalyzeMesh()
        {
            Mesh sourceMesh = GetComponent<MeshFilter>().sharedMesh;

            if (sourceMesh == null)
            {
                return false;
            }

            Vector3[] vertices = sourceMesh.vertices;
            Vector3[] normals = sourceMesh.normals;
            int[] indices = sourceMesh.GetIndices(0);





            MeshTopology topology = sourceMesh.GetTopology(0);


            CentreModel = new Vector4(1, 1, 1, 1);

            MaxPos = new Vector4(-9999999, -9999999, -9999999, 1);
            MinPos = new Vector4(9999999, 9999999, 9999999, 1);

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].x < MinPos.x)
                {
                    MinPos.x = vertices[i].x;
                }

                if (vertices[i].y < MinPos.y)
                {
                    MinPos.y = vertices[i].y;
                }

                if (vertices[i].z < MinPos.z)
                {
                    MinPos.z = vertices[i].z;
                }



                if (vertices[i].x > MaxPos.x)
                {
                    MaxPos.x = vertices[i].x;
                }

                if (vertices[i].y > MaxPos.y)
                {
                    MaxPos.y = vertices[i].y;
                }

                if (vertices[i].z > MaxPos.z)
                {
                    MaxPos.z = vertices[i].z;
                }

            }


            CentreModel.x = (MaxPos.x + MinPos.x) / 2;
            CentreModel.y = (MaxPos.y + MinPos.y) / 2;
            CentreModel.z = (MaxPos.z + MinPos.z) / 2;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].x = vertices[i].x - CentreModel.x;
                vertices[i].y = vertices[i].y - CentreModel.y;
                vertices[i].z = vertices[i].z - CentreModel.z;
            }


                scale = 0.0f;
            // calc scale
            for (int i = 0; i < vertices.Length; ++i)
            {
                // 5% margin
                scale = Mathf.Max(vertices[i].magnitude * 1.05f, scale);
            }

            int texSize = 4;
            Color[] planes = null;

            int stride = 3;
            if (topology == MeshTopology.Triangles)
            {
                stride = 3;
            }
            else if (topology == MeshTopology.Quads)
            {
                stride = 4;
            }
            else
            {
                // no support
                Debug.LogError("unsupported mesh topology detected : " + topology.ToString());
            }
            /*
            if (!mergeSimilarPlanes)
            {
                // old version (maybe better for some case?)
                planeCount = indices.Length / stride;

                while (texSize * texSize < planeCount)
                {
                    texSize *= 2;
                }

                planes = new Color[texSize * texSize];
                for (int i = 0; i < planeCount; i++)
                {
                    int index = i * stride;

                    int vertIndex = indices[index];
                    Vector3 primaryPosition = vertices[vertIndex];
                    Vector3 primaryNormal = normals[vertIndex];

                    Color packedPlane = PackPlaneIntoColor(primaryPosition, primaryNormal, scale);
                    planes[i] = packedPlane;
                    
                }
             //   print(1);
            }
            else
            {

                */



                // new version (optimized)
                List<Color> tmpPlanes = new List<Color>();

                int faceCount = indices.Length / stride;

                for (int i = 0; i < faceCount; i++)
                {
                    int index = i * stride;

                    int vertIndex = indices[index];
                    Vector3 primaryPosition = vertices[vertIndex];
                    Vector3 primaryNormal = normals[vertIndex];

                    Color packedPlane = PackPlaneIntoColor(primaryPosition, primaryNormal, scale);

                    bool duplicated = false;
                    foreach(Color c in tmpPlanes)
                    {
                        if( c == packedPlane )
                        {
                            duplicated = true;
                            break;
                        }
                    }

                    if( !duplicated )
                    {
                        tmpPlanes.Add(packedPlane);
                    }
                }
                
                planeCount = tmpPlanes.Count;
                while (texSize * texSize < planeCount)
                {
                    texSize *= 2;
                }
                planes = new Color[texSize * texSize];
                for( int i=0; i<tmpPlanes.Count; ++i )
                {
                    planes[i] = tmpPlanes[i];
                }
            

            //            if( shapeTexture == null)
            // must new everytime to prevent being shared...
            {
                shapeTexture = new Texture2D(texSize, texSize);
                shapeTexture.filterMode = FilterMode.Point;
            }
            shapeTexture.Reinitialize(texSize, texSize);
            shapeTexture.SetPixels(planes);
            shapeTexture.Apply();
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(shapeTexture, "Assets/SuperRealisticDiamondShaders/ShapeTextures/" + "_" + System.DateTime.Now.ToString("yyyy-MM-dd") + "_ " + System.DateTime.Now.Hour + "_ " + System.DateTime.Now.Minute + Random.Range(-99999,99999) + " shapeTexture_.asset"); // save the modified model
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

#endif
            return true;
        }

        static Color PackPlaneIntoColor(Vector3 position, Vector3 normal, float in_scale)
        {
            Color retval;

            retval.r = (normal.x + 1.0f) * 0.5f;
            retval.g = (normal.y + 1.0f) * 0.5f;
            retval.b = (normal.z + 1.0f) * 0.5f;

           //    retval.a = dots;
              retval.a = Vector3.Dot(position, normal) / in_scale;

            if (retval.a < 0 || retval.a > 1.0f)
            {
                // error
                //    Debug.LogError("invalid model scale or vertex position detected...");
            }

            return retval;
        }
    }
}
 