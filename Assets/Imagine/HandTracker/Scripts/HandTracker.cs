using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using UnityEditor;

namespace Imagine.WebAR{

    public enum Handedness {Right, Left};


    [System.Serializable]
    public class HandTrackerSettings3D{
        public bool trackIndividualJoints = true;
        public float handThickness = 1.5f;
        public float defaultCanonicalHandLength = 0.8918802f;

    }

    [System.Serializable]
    public class HandTrackerSettings2D{
        
    }

    public class HandTracker : MonoBehaviour
    {
        [DllImport("__Internal")] private static extern void StartWebGLhTracker(string name);
        [DllImport("__Internal")] private static extern void StopWebGLhTracker();
        [DllImport("__Internal")] private static extern bool IsWebGLhTrackerReady();
        [DllImport("__Internal")] private static extern void SetWebGLhTrackerSettings(string settings);
    

        [SerializeField] ARCamera trackerCam;
        Camera cam;

        public enum HandTrackerMode{MODE_3D, MODE_2D};
        public HandTrackerMode trackerMode = HandTrackerMode.MODE_3D;
        
        public HandTrackerSettings3D ts3d;
        public HandTrackerSettings2D ts2d;

        [HideInInspector] public Dictionary<int, HandObject> handObjects = new Dictionary<int, HandObject>();

        [SerializeField] bool debugHands = true;
        [SerializeField] DebugLines debugLinesPrefab;
        [HideInInspector] [SerializeField] float debugCamMoveSensitivity = 1, debugCamTiltSensitivity = 20;
        public Dictionary<HandObject, DebugLines> debugLinesDict = new Dictionary<HandObject, DebugLines>();

        private Vector3 pos, forward, up, right;
        private Quaternion rot;

        public static string[] jointChain = new string[]{
            "0,1,2,3,4",
            "0,5,6,7,8",
            "9,10,11,12",
            "13,14,15,16",
            "0,17,18,19,20",
            "5,9,13,17",
        };

        public Dictionary<string, float> jointLengthsDict = new Dictionary<string, float>();

        // private List<Vector3> jointPosList = new List<Vector3>();



#if UNITY_EDITOR
        private static HandTracker _editorInstance;
        public static HandTrackerMode EditorTrackerMode {
            get{
                if(_editorInstance == null){
                    _editorInstance = FindObjectOfType<HandTracker>();
                }
                return _editorInstance.trackerMode;
            }
        }
        private static Mesh _editorSceneViewHand2dMesh;
        public static Mesh EditorSceneViewHand2dMesh{
            get{
                if(_editorSceneViewHand2dMesh == null){
                    _editorSceneViewHand2dMesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Imagine/HandTracker/Models/Hand_Icon.mesh");
                }
                return _editorSceneViewHand2dMesh;
            }
        }

        public static DebugLines EditorGetDebugLines(HandObject handObject){
            if(_editorInstance == null){
                _editorInstance = FindObjectOfType<HandTracker>();
            }
            return _editorInstance.debugLinesDict[handObject];
        }
#endif


        void Awake(){
            
        }

        IEnumerator Start()
        {

            InitHandObjects();

            cam = trackerCam.GetComponent<Camera>();

#if !UNITY_EDITOR && UNITY_WEBGL
            StartTracker();
            SetTrackerSettings();
#else
            // yield return new WaitForSeconds(1);
            // OnHandFound(0);

            // if(trackerMode == HandTrackerMode.MODE_3D){
            //     OnHandPoseUpdated3D(handPose3DStr);
            // }
            // else if (trackerMode == HandTrackerMode.MODE_2D)
            //     OnHandPoseUpdated2D(handPose2DStr);

            // yield return new WaitForSeconds(1);

            // if(trackerMode == HandTrackerMode.MODE_3D){
            //     OnHandPointsUpdated(handPoints3DStr);
            // }
#endif
            yield break;
        }

        void Update(){
#if UNITY_EDITOR
            Update_Debug();
#endif
        }


        // [TextArea] public string handPose3DStr;
        // [TextArea(6,10)] public string handPoints3DStr;
        // [TextArea] public string handPose2DStr;


        void InitHandObjects(){
            var foundHandObjects = FindObjectsOfType<HandObject>();
            foreach(var handObject in foundHandObjects){
                var handId = handObject.handIndex;
                if(handObjects.ContainsKey(handId)){
                    Debug.LogWarning("Duplicate hand id found (" + handId + "). Skipping.");
                    continue;
                }
                handObjects.Add(handId, handObject);

                //debug
                if(debugHands){
                    var debugLines = Instantiate(debugLinesPrefab, handObject.transform);
                    debugLines.Init(handObject, trackerMode);
                    debugLinesDict.Add(handObject, debugLines);
                }

                //gestures
                if(handObject.s3d.useGestures){
                    handObject.InitFingerCloseness();
                }

                handObject.gameObject.SetActive(false);
            }

            if(trackerMode == HandTrackerMode.MODE_3D)
            {
                ts3d.defaultCanonicalHandLength = GetHandLength(handObjects.First().Value.s3d.joints);
                Debug.Log("Canonical hand length = " + ts3d.defaultCanonicalHandLength);
            }
        

        }


        public void StartTracker(){
#if UNITY_WEBGL && !UNITY_EDITOR
            StartWebGLhTracker(name);
#endif
        }

        public void StopTracker(){
#if UNITY_WEBGL && !UNITY_EDITOR
            StopWebGLhTracker();
#endif
        }

        void SetTrackerSettings(){
            var json = "{";
            json += "\"MODE\":\"" + trackerMode + "\",";
            json += "\"TRACK_INDIVIDUAL_JOINTS\":" + (ts3d.trackIndividualJoints ? "true" : "false");
            json += "}";
            SetWebGLhTrackerSettings(json);
        }

        void OnHandFound(int handId){
            var handObject = handObjects[handId];
            if(handObject != null){
                handObject.gameObject.SetActive(true);
            }
        }
        void OnHandLost(int handId){
            var handObject = handObjects[handId];
            if(handObject != null){
                handObject.gameObject.SetActive(false);
            }
        }

        void OnHandPoseUpdated3D(string data){
            // Debug.Log(data);
            var fields = data.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);
            var handId = int.Parse(fields[0]);
            var handedness = (Handedness)Enum.Parse(typeof(Handedness), fields[1]);
  
            var handObject3D = handObjects[handId];
            if(handObject3D == null){
                Debug.LogError("Can't find handId: " + handId);
                return;
            }

            handObject3D.handedness = handedness;

            var values = fields[2].Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries);

            pos.x = float.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
            pos.y = float.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
            pos.z = float.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);

            forward.x = float.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture);
            forward.y = float.Parse(values[4], System.Globalization.CultureInfo.InvariantCulture);
            forward.z = float.Parse(values[5], System.Globalization.CultureInfo.InvariantCulture);

            up.x = float.Parse(values[6], System.Globalization.CultureInfo.InvariantCulture);
            up.y = float.Parse(values[7], System.Globalization.CultureInfo.InvariantCulture);
            up.z = float.Parse(values[8], System.Globalization.CultureInfo.InvariantCulture);

            right.x = float.Parse(values[9], System.Globalization.CultureInfo.InvariantCulture);
            right.y = float.Parse(values[10], System.Globalization.CultureInfo.InvariantCulture);
            right.z = float.Parse(values[11], System.Globalization.CultureInfo.InvariantCulture);

            rot = Quaternion.LookRotation(forward, up);

            if(trackerCam.isFlipped){
                rot.eulerAngles = new Vector3(rot.eulerAngles.x, rot.eulerAngles.y * -1, rot.eulerAngles.z * -1);
                pos.x *= -1;
            }

            handObject3D.transform.position = trackerCam.transform.TransformPoint(pos);
            handObject3D.transform.rotation = trackerCam.transform.rotation * rot;

        }


        void OnHandPointsUpdated(string data){
            // Debug.Log(data);
            var fields = data.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);
            var handId = int.Parse(fields[0]);  
            var handedness = (Handedness)Enum.Parse(typeof(Handedness), fields[1]);
            var handObject3D = handObjects[handId];
            if(handObject3D == null){
                Debug.LogError("Can't find handId: " + handId);
                return;
            }

            handObject3D.handedness = handedness;

            var joints = handObject3D.s3d.joints;


            

            var points = data.Replace(handId + ";" + handedness + ";", "").Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);
            var ctr = 0;

            //scale joints
            var scale = Vector3.one;
            if( (trackerCam.isFlipped && handedness == Handedness.Right) ||
                (!trackerCam.isFlipped && handedness == Handedness.Left)
            ){
                scale.x *= -1;
            }
            joints[0].localScale = scale;
            var handLength = GetHandLength(joints);
            // Debug.Log("handLength = " + handLength);
            joints[0].transform.parent.localScale = handLength / ts3d.defaultCanonicalHandLength * Vector3.one * ts3d.handThickness;
            

            foreach(var point in points){

                var coords = point.Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries);
                var pos = new Vector3(
                    float.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture), 
                    float.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture), 
                    float.Parse(coords[2], System.Globalization.CultureInfo.InvariantCulture));

                var up = new Vector3(
                    float.Parse(coords[3], System.Globalization.CultureInfo.InvariantCulture), 
                    float.Parse(coords[4], System.Globalization.CultureInfo.InvariantCulture), 
                    float.Parse(coords[5], System.Globalization.CultureInfo.InvariantCulture));

                var fwd = new Vector3(
                    float.Parse(coords[6], System.Globalization.CultureInfo.InvariantCulture), 
                    float.Parse(coords[7], System.Globalization.CultureInfo.InvariantCulture), 
                    float.Parse(coords[8], System.Globalization.CultureInfo.InvariantCulture));

                var rot = Quaternion.LookRotation(fwd, up);
                rot = rot * Quaternion.AngleAxis(180, Vector3.up);

                if(trackerCam.isFlipped){
                    rot.eulerAngles = new Vector3(rot.eulerAngles.x, rot.eulerAngles.y * -1, rot.eulerAngles.z * -1);
                    pos.x *= -1;
                }
                
                pos = trackerCam.transform.TransformPoint(pos);
                rot = trackerCam.transform.rotation * rot;

                joints[ctr].position = pos;
                joints[ctr].rotation = rot;
   
                ctr++;

            }  

            //process gestures
            if(handObject3D.s3d.useGestures){
                ProcessGestures(handObject3D, handedness);
            }

            

            if(debugHands){
                var debugLines = debugLinesDict[handObject3D];
                debugLines.SetData(joints);
            }

        }

        void ProcessGestures(HandObject handObject3D, Handedness handedness){

            var joints = handObject3D.s3d.joints;

            // //this should fix invalid rotations when hand is facing back
            // var right = (joints[9].position - joints[13].position).normalized;
            // var up = (joints[9].position - joints[0].position).normalized;
            // var forward = Vector3.Cross(up, right);
            // if(trackerCam.isFlipped && handedness == Handedness.Right ||
            //     !trackerCam.isFlipped && handedness == Handedness.Left)
            // {
            //     forward *= -1;
            // }
            // handObject3D.s3d.joints[0].parent.rotation = Quaternion.LookRotation(forward, up);

            var indexDot = Mathf.Abs(Vector3.Dot((joints[7].position - joints[6].position).normalized, 
                (joints[6].position - joints[5].position).normalized));
            var middleDot = Mathf.Abs(Vector3.Dot((joints[11].position - joints[10].position).normalized, 
                (joints[10].position - joints[9].position).normalized));
            var ringDot = Mathf.Abs(Vector3.Dot((joints[15].position - joints[14].position).normalized, 
                (joints[14].position - joints[13].position).normalized));
            var pinkyDot = Mathf.Abs(Vector3.Dot((joints[19].position - joints[18].position).normalized, 
                (joints[18].position - joints[17].position).normalized));
            var thumbDot = Mathf.Abs(Vector3.Dot((joints[4].position - joints[3].position).normalized, 
                (joints[3].position - joints[2].position).normalized));

            var indexDot2 = Mathf.Abs(Vector3.Dot((joints[6].position - joints[5].position).normalized, 
                (joints[5].position - joints[0].position).normalized));
            var middleDot2 = Mathf.Abs(Vector3.Dot((joints[10].position - joints[9].position).normalized, 
                (joints[9].position - joints[0].position).normalized));
            var ringDot2 = Mathf.Abs(Vector3.Dot((joints[14].position - joints[13].position).normalized, 
                (joints[13].position - joints[0].position).normalized));
            var pinkyDot2 = Mathf.Abs(Vector3.Dot((joints[18].position - joints[17].position).normalized, 
                (joints[17].position - joints[0].position).normalized));
             var thumbDot2 = Mathf.Abs(Vector3.Dot((joints[3].position - joints[2].position).normalized, 
                (joints[2].position - joints[1].position).normalized));


            var thr = handObject3D.s3d.closenessThresholds;

            handObject3D.SetFingerClosed("Index"  , indexDot  < thr.indexThreshold  || indexDot2 < thr.indexThreshold2);  
            handObject3D.SetFingerClosed("Middle" , middleDot < thr.middleThreshold || middleDot2 < thr.middleThreshold2);  
            handObject3D.SetFingerClosed("Ring"   , ringDot   < thr.ringThreshold   || ringDot2 < thr.ringThreshold2);  
            handObject3D.SetFingerClosed("Pinky"  , pinkyDot  < thr.pinkyThreshold  || pinkyDot2 < thr.pinkyThreshold2);  
            handObject3D.SetFingerClosed("Thumb"  , thumbDot  < thr.thumbThreshold  || thumbDot2 < thr.thumbThreshold2);  
        }


        void OnHandPoseUpdated2D(string data){
            // Debug.Log(data);
            var fields = data.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);
            var handId = int.Parse(fields[0]);
            var handedness = (Handedness)Enum.Parse(typeof(Handedness), fields[1]); 
            var handObject2D = handObjects[handId];
            if(handObject2D == null){
                Debug.LogError("Can't find handId: " + handId);
                return;
            }

            handObject2D.handedness = handedness;

            var values = fields[2].Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries);

            float minX = float.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
            float minY = float.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
            float maxX = float.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
            float maxY = float.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture);

            var zPos = trackerCam.transform.position.z + handObject2D.s2d.zDistance;
            var viewPos = new Vector3((maxX + minX)/2, (maxY + minY)/2, zPos);
            
            if(trackerCam.isFlipped){
                viewPos.x = 1 - viewPos.x;
            }
            
            var worldPos = trackerCam.cam.ViewportToWorldPoint(viewPos);

            handObject2D.transform.position = worldPos;

            handObject2D.s2d.OnPosChanged?.Invoke(viewPos);
            handObject2D.s2d.OnPosXChanged.Invoke(viewPos.x);
            handObject2D.s2d.OnPosYChanged.Invoke(viewPos.y);

            //debug
            if(debugHands){
                var debugLines = debugLinesDict[handObject2D];
                var pos = new Vector3((maxX + minX)/2, (maxY + minY)/2);

                var corners = new List<Vector3>();
                var p1 = trackerCam.cam.ViewportToWorldPoint(new Vector3(minX, maxY, zPos));
                var p2 = trackerCam.cam.ViewportToWorldPoint(new Vector3(maxX, maxY, zPos));
                var p3 = trackerCam.cam.ViewportToWorldPoint(new Vector3(maxX, minY, zPos));
                var p4 = trackerCam.cam.ViewportToWorldPoint(new Vector3(minX, minY, zPos));

                if(trackerCam.isFlipped){
                    p1.x *= -1;
                    p2.x *= -1;
                    p3.x *= -1;
                    p4.x *= -1;
                }

                corners.Add(p1);
                corners.Add(p2);
                corners.Add(p3);
                corners.Add(p4);
                corners.Add(p1);
                debugLines.SetData(corners);
            }

        }

        float GetHandLength(List<Transform> joints){
            float d = 0;
            d += Vector3.Distance(joints[0].position, joints[9].position);
            d += Vector3.Distance(joints[9].position, joints[10].position);
            d += Vector3.Distance(joints[10].position, joints[11].position);
            d += Vector3.Distance(joints[11].position, joints[12].position);

            return d;
        }

        private void Update_Debug()
        {
            var x_left = Input.GetKey(KeyCode.A);
            var x_right = Input.GetKey(KeyCode.D);
            var z_forward = Input.GetKey(KeyCode.W); ;
            var z_back = Input.GetKey(KeyCode.S); ;
            var y_up = Input.GetKey(KeyCode.R);
            var y_down = Input.GetKey(KeyCode.F);

            float speed = debugCamMoveSensitivity * Time.deltaTime;
            float dx = (x_right ? speed : 0) + (x_left ? -speed : 0);
            float dy = (y_up ? speed : 0) + (y_down ? -speed : 0);
            //float dsca = 1 + (z_forward ? speed : 0) + (z_back ? -speed : 0);
            float dz = (z_forward ? speed : 0) + (z_back ? -speed : 0);


            var y_rot_left = Input.GetKey(KeyCode.LeftArrow);
            var y_rot_right = Input.GetKey(KeyCode.RightArrow);
            var x_rot_up = Input.GetKey(KeyCode.UpArrow);
            var x_rot_down = Input.GetKey(KeyCode.DownArrow);
            var z_rot_cw = Input.GetKey(KeyCode.Comma);
            var z_rot_ccw = Input.GetKey(KeyCode.Period);

            var angularSpeed = debugCamTiltSensitivity * Time.deltaTime; //degrees per frame
            var d_rotx = (x_rot_up ? angularSpeed : 0) + (x_rot_down ? -angularSpeed : 0);
            var d_roty = (y_rot_right ? angularSpeed : 0) + (y_rot_left ? -angularSpeed : 0);
            var d_rotz = (z_rot_ccw ? angularSpeed : 0) + (z_rot_cw ? -angularSpeed : 0);

            Quaternion rot;
            rot.w = trackerCam.transform.rotation.w;
            rot.x = trackerCam.transform.rotation.x;
            rot.y = trackerCam.transform.rotation.y;
            rot.z = trackerCam.transform.rotation.z;

            // rot = new Quaternion(i, j, k, w);

            var dq = Quaternion.Euler(d_rotx, d_roty, d_rotz);
            rot *= dq;
            //rot *= Quaternion.AngleAxis(d_rotz, trackerCamera.transform.forward);
            //rot *= Quaternion.AngleAxis(d_roty, trackerCamera.transform.up);
            //rot *= Quaternion.AngleAxis(d_rotx, trackerCamera.transform.right);

            //Debug.Log(dx + "," + dy + "," + dsca);
            var dp = Vector3.right * dx + Vector3.up * dy + Vector3.forward * dz;
            trackerCam.transform.Translate(dp);
            trackerCam.transform.rotation = rot;
        }
    }
    
}

