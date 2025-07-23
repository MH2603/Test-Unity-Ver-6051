using System.Collections;
using System.Collections.Generic;
using Imagine.WebAR;
using UnityEngine;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Imagine.WebAR
{
    public partial class HandObject
    {
        [System.Serializable]
        public class HandObject3DSettings{
            public List<Transform> joints;
            public bool useHandMesh = false;
            public SkinnedMeshRenderer handMesh;

            public bool useGestures = false;
            // public Animator handAnimator;


            [System.Serializable]
            public class FingerClosenessThresholds{
                [Range(0,1)] public float indexThreshold = 0.75f;
                [Range(0,1)] public float middleThreshold = 0.75f;
                [Range(0,1)] public float ringThreshold = 0.75f;
                [Range(0,1)] public float pinkyThreshold = 0.75f;
                [Range(0,1)] public float thumbThreshold = 0.75f;
                
                [Space]
                [Range(0,1)] public float indexThreshold2 = 0.95f;
                [Range(0,1)] public float middleThreshold2 = 0.95f;
                [Range(0,1)] public float ringThreshold2 = 0.95f;
                [Range(0,1)] public float pinkyThreshold2 = 0.95f;
                [Range(0,1)] public float thumbThreshold2 = 0.95f;
            }
            public FingerClosenessThresholds closenessThresholds;

            public Dictionary<string, bool> fingerClosedBools = new Dictionary<string, bool>();
            public Dictionary<string, bool> fingerClosedDebounceFlags = new Dictionary<string, bool>();

            public float debounceTime = 0.2f;


            [System.Serializable]
            public class GestureEvents{
                public UnityEvent<string, bool> OnFingerClosedGesture;

                public List<CustomGesture> customGestures;
                public UnityEvent OnAllGesturesLost;
                [HideInInspector] public bool gestureFoundInLastFrame = false;
            }
            public GestureEvents gestureEvents;

            [System.Serializable]
            public class CustomGesture{
                public string gestureName;
                public bool thumbUp, indexUp, middleUp, ringUp, pinkyUp;
                public UnityEvent OnDetected; 
                public UnityEvent OnLost;

                [HideInInspector] public bool isFoundOnLastFrame = false;

                public bool CheckInvokeFoundLost(bool tUp, bool iUp, bool mUp, bool rUp, bool pUp)
                {
                    bool retVal = false;

                    if(this.thumbUp == tUp && this.indexUp == iUp && this.middleUp == mUp && this.ringUp == rUp && this.pinkyUp == pUp)
                    {
                        if(!isFoundOnLastFrame){
                            Debug.Log("OnDetected " + gestureName);
                            OnDetected?.Invoke();
                            isFoundOnLastFrame = true;
                        }
                        retVal = true;
                    }
                    else if(this.thumbUp != tUp || this.indexUp != iUp || this.middleUp != mUp || this.ringUp != rUp || this.pinkyUp != pUp)
                    {
                        if(isFoundOnLastFrame){
                            Debug.Log("OnLost " + gestureName);
                            OnLost?.Invoke();
                            isFoundOnLastFrame = false;
                        }
                    }
                    return retVal;
                }
            }

        }

        public HandObject3DSettings s3d;

        [System.Serializable]
        public class HandObject3DDebugSettings{
            public bool debugShowCameraRaycast = false;
            public bool debugShowAxes = false;
        }
        public HandObject3DDebugSettings dbgS3d;

        // public void SetHandAnimator(Animator anim){
        //         s3d.handAnimator = anim;        
        // }
        
        public void InitFingerCloseness(){

            s3d.fingerClosedBools.Add("Index", false);
            s3d.fingerClosedBools.Add("Middle", false);
            s3d.fingerClosedBools.Add("Ring", false);
            s3d.fingerClosedBools.Add("Pinky", false);
            s3d.fingerClosedBools.Add("Thumb", false);

            s3d.fingerClosedDebounceFlags.Add("Index", false);
            s3d.fingerClosedDebounceFlags.Add("Middle", false);
            s3d.fingerClosedDebounceFlags.Add("Ring", false);
            s3d.fingerClosedDebounceFlags.Add("Pinky", false);
            s3d.fingerClosedDebounceFlags.Add("Thumb", false);

        }

        public void ResetFingerCloseness(){
            s3d.fingerClosedDebounceFlags["Index"] = false;
            s3d.fingerClosedDebounceFlags["Middle"] = false;
            s3d.fingerClosedDebounceFlags["Ring"] = false;
            s3d.fingerClosedDebounceFlags["Pinky"] = false;
            s3d.fingerClosedDebounceFlags["Thumb"] = false;
        }


        //closeness toggle
        public void SetFingerClosed(string key, bool value)
        {
            if(s3d.fingerClosedBools[key] == value)
                return;
                
            s3d.fingerClosedBools[key] = value;

            if (!s3d.fingerClosedDebounceFlags[key])
            {
                // s3d.fingerClosedBools[key] = value;
                // Debug.Log($"{key} set to: {s3d.fingerClosedBools[key]}");
                if(gameObject.activeInHierarchy)
                {
                    StartCoroutine(DebounceCoroutine(key, value));
                }
            }
        }

        private IEnumerator DebounceCoroutine(string key, bool isClosed)
        {
            s3d.fingerClosedDebounceFlags[key] = true;
            var startVal = isClosed;
            yield return new WaitForSeconds(s3d.debounceTime);
            if(startVal == s3d.fingerClosedBools[key])
            {
                s3d.gestureEvents.OnFingerClosedGesture?.Invoke(key + "Closed", startVal);

                //Custom Gestures
                var gestureFoundInCurrentFrame = false;
                foreach(var g in s3d.gestureEvents.customGestures){
                    gestureFoundInCurrentFrame = gestureFoundInCurrentFrame || g.CheckInvokeFoundLost(
                        !s3d.fingerClosedBools["Thumb"],
                        !s3d.fingerClosedBools["Index"],
                        !s3d.fingerClosedBools["Middle"],
                        !s3d.fingerClosedBools["Ring"],
                        !s3d.fingerClosedBools["Pinky"]
                    );
                }

                if(s3d.gestureEvents.gestureFoundInLastFrame && !gestureFoundInCurrentFrame){
                    s3d.gestureEvents.OnAllGesturesLost?.Invoke();
                }
                s3d.gestureEvents.gestureFoundInLastFrame = gestureFoundInCurrentFrame;
            }
            s3d.fingerClosedDebounceFlags[key] = false;
        }

        void OnDisable(){
            //hack: to fix debounceflags getting stuck
            ResetFingerCloseness();
        }

        
        

#if UNITY_EDITOR
        void OnDrawGizmos3D(){

            var joints = s3d.joints;
            var cam = Camera.main;
            // var limits = HandTracker.EditorJointLimits;
            var jctr = 0;
            foreach(var joint in joints){
                Handles.color = Color.white;
                Gizmos.DrawSphere(joint.position, 0.01f);

                if(dbgS3d.debugShowCameraRaycast && cam != null){
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(joint.position, cam.transform.position);
                }

                if(dbgS3d.debugShowAxes){
                    var length = 0.04f;
                    Handles.color = Color.red;
                    Handles.DrawLine(joint.position, joint.position + joint.right * length, 3);

                    Handles.color = Color.green;
                    Handles.DrawLine(joint.position, joint.position + joint.up * length, 3);

                    Handles.color = Color.blue;
                    Handles.DrawLine(joint.position, joint.position + joint.forward * length, 3);
                }

                jctr++;

            }

            Gizmos.color = Color.white;//handedness == Handedness.Left ? Color.green : Color.red;
            var lctr = 0;
            foreach(var line in HandTracker.jointChain){
                var indices = line.Split(new string[]{","}, System.StringSplitOptions.RemoveEmptyEntries);
                var pctr = 0;
                for(var i = 1; i < indices.Length; i++){//(var indexStr in indices){
                    
                    var index1 = int.Parse(indices[i]);
                    var index2 = int.Parse(indices[i-1]);
                    Gizmos.DrawLine(joints[index1].position, joints[index2].position);
                    pctr++;
                }
                lctr++;
            }
        }
#endif

    }

    

}
