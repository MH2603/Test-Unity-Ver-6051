using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

#if UNITY_POST_PROCESSING_STACK_V2 
using UnityEngine.Rendering.PostProcessing;
#endif
[ExecuteInEditMode]
public class ImportPostProcces : MonoBehaviour
{
    static AddRequest Request;
    public static bool Lo;
  //  [HideInInspector]
    public static bool Reload;
    static GameObject ggameObject;
#if UNITY_POST_PROCESSING_STACK_V2
    public PostProcessResources postProcessResources;
#endif
    //   public GameObject Button1;
    //  public GameObject Button2;
    [InitializeOnLoadMethod]
    public void Awake()
    {
#if !UNITY_POST_PROCESSING_STACK_V2
        if (PlayerSettings.colorSpace == ColorSpace.Gamma)
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
        }
#endif


#if UNITY_POST_PROCESSING_STACK_V2
        postProcessResources = (PostProcessResources)AssetDatabase.LoadAssetAtPath("Packages/Post Processing/PostProcessing/PostProcessResources.asset", typeof(PostProcessResources));
#endif
        if (!Application.isPlaying)
        {
            Lo = true;



        }

     //   if (Application.isPlaying)
       // {
   //         EditorApplication.ExitPlaymode();
 //       }

    }
    [ContextMenu("Setup")]
    public void OnEnable()
    {

#if !UNITY_POST_PROCESSING_STACK_V2
        if (PlayerSettings.colorSpace == ColorSpace.Gamma)
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
        }
#endif

#if UNITY_POST_PROCESSING_STACK_V2
        if (!Application.isPlaying)
        {
            postProcessResources = (PostProcessResources)AssetDatabase.LoadAssetAtPath("Assets/SuperRealisticDiamondShaders/PostProcessResources.asset", typeof(PostProcessResources));



            gameObject.GetComponent<PostProcessLayer>().Init(postProcessResources);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            ClearConsole();
        }
#endif
    }

    private static void ClearConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");

        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        clearMethod.Invoke(null, null);
    }

    public void Update()
    {

#if !UNITY_POST_PROCESSING_STACK_V2
        if (PlayerSettings.colorSpace == ColorSpace.Gamma)
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
        }
#endif

#if UNITY_POST_PROCESSING_STACK_V2

        //  if(Button1 == null || Button2 == null || Button1.name != "Button") { 
        //   Button1 = GameObject.Find("Canvas").transform.GetChild(0).gameObject;
        //   Button2 = GameObject.Find("Canvas").transform.GetChild(1).gameObject;
        //   }

        postProcessResources = (PostProcessResources)AssetDatabase.LoadAssetAtPath("Assets/SuperRealisticDiamondShaders/PostProcessResources.asset", typeof(PostProcessResources));








        /*
              if (!Application.isPlaying)
              {

                  //        


                  gameObject.GetComponent<PostProcessLayer>().Init(postProcessResources);


                  gameObject.AddComponent<PostProcessLayer>();
                  gameObject.GetComponent<PostProcessLayer>().volumeLayer = ~0;
                  gameObject.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                  gameObject.GetComponent<PostProcessLayer>().temporalAntialiasing.jitterSpread = 0.75f;
                  gameObject.GetComponent<PostProcessLayer>().temporalAntialiasing.stationaryBlending = 0.877f;
                  gameObject.GetComponent<PostProcessLayer>().temporalAntialiasing.motionBlending = 0.587f;
                  gameObject.GetComponent<PostProcessLayer>().temporalAntialiasing.sharpness = 1.453f; 
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            ClearConsole();
        }
        */





        if (Lo == false && !gameObject.GetComponent<PostProcessLayer>() || Lo == true && !Application.isPlaying)
        {



            if (!Application.isPlaying) {

                //        


                gameObject.GetComponent<PostProcessLayer>().Init(postProcessResources);

                /*
                gameObject.AddComponent<PostProcessLayer>();
                gameObject.GetComponent<PostProcessLayer>().volumeLayer = ~0;
                gameObject.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                gameObject.GetComponent<PostProcessLayer>().temporalAntialiasing.jitterSpread = 0.75f;
                gameObject.GetComponent<PostProcessLayer>().temporalAntialiasing.stationaryBlending = 0.877f;
                gameObject.GetComponent<PostProcessLayer>().temporalAntialiasing.motionBlending = 0.587f;
                gameObject.GetComponent<PostProcessLayer>().temporalAntialiasing.sharpness = 1.453f; */
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

                     ClearConsole();
            }
            //
        }
        /*
        if (gameObject.GetComponent<PostProcessVolume>() && !Application.isPlaying) {

            if (SceneManager.GetActiveScene().name == "Diamond Demo" || SceneManager.GetActiveScene().name == "Diamond Demo2" || SceneManager.GetActiveScene().name == "Diamond Demo3")
            {
                gameObject.GetComponent<PostProcessVolume>().sharedProfile = (PostProcessProfile)AssetDatabase.LoadAssetAtPath("Assets/Diamond Realistic Shaders/Scenes/SampleScene_Profiles/Camera Profile 3.asset", typeof(PostProcessProfile));
            }
            
            if (SceneManager.GetActiveScene().name == "DiamondRing" || SceneManager.GetActiveScene().name == "DiamondRingLWRP")
            {
                gameObject.GetComponent<PostProcessVolume>().sharedProfile = (PostProcessProfile)AssetDatabase.LoadAssetAtPath("Assets/Diamond Realistic Shaders/Scenes/SampleScene_Profiles/Camera Profile 8.asset", typeof(PostProcessProfile));
            }


            if (SceneManager.GetActiveScene().name == "DiamondRing2")
            {
                gameObject.GetComponent<PostProcessVolume>().sharedProfile = (PostProcessProfile)AssetDatabase.LoadAssetAtPath("Assets/Diamond Realistic Shaders/Scenes/SampleScene_Profiles/Camera Profile 6.asset", typeof(PostProcessProfile));
            }

            if (!gameObject.GetComponent<PostProcessVolume>().isGlobal)
            {
                gameObject.GetComponent<PostProcessVolume>().isGlobal = true;
                

                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
                ClearConsole();
        } */

        //   }


        /*
        if (Button1 != null && Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (Button1.activeSelf)
            {
                    gameObject.GetComponent<PostProcessLayer>().enabled = false;
                gameObject.GetComponent<HighSpeedPostProcessing>().enabled = true;
            }
            if (Button2.activeSelf)
            {
                gameObject.GetComponent<PostProcessLayer>().enabled = true;
                gameObject.GetComponent<HighSpeedPostProcessing>().enabled = false;
            }
        }*/

#endif

        if (Lo)
        {

#if UNITY_POST_PROCESSING_STACK_V2
            if (gameObject.GetComponent<PostProcessLayer>() && gameObject.GetComponent<PostProcessLayer>().volumeTrigger != null)
            {
                Lo = false;

            }
#endif

        }

        if (Lo)
        {


            //   if (!text.ToString().Contains("universal")){
            //       if (!text.ToString().Contains("postprocessing"))

            //         {
            //        if (Application.isPlaying)
            //        {
            //            EditorApplication.ExitPlaymode();
            //       }





                if ( !Application.isPlaying) {
                Lo = false;
               Add();
                    ggameObject = gameObject;
                         }
                    //     }


                    //     else
                    //    {
                    //   gameObject.AddComponent(typeof(Volume));
                    //  }



                    

        }

    }



 void Add()
{
        // Add a package to the Project
        Request = Client.Add("com.unity.postprocessing");
    EditorApplication.update += Progress;
        Lo = false;
    }

    static void Progress()
{
    if (Request.IsCompleted)
    {
            if (Request.Status == StatusCode.Success)
            {
                Debug.Log("Installed: " + Request.Result.packageId);
                //  PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "POSTPROCCESSS");
            }
            else if (Request.Status >= StatusCode.Failure)
                Debug.Log(Request.Error.message);

        EditorApplication.update -= Progress;
               Reload = true;


            Lo = false;
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            //          EditorSceneManager.OpenScene(SceneManager.GetActiveScene().path);




        }
    }
}

#endif