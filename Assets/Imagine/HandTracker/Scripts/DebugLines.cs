using System;
using System.Collections;
using System.Collections.Generic;
using Imagine.WebAR;
using UnityEditor;
using UnityEngine;

namespace Imagine.WebAR{
    public class DebugLines : MonoBehaviour
    {
        public List<LineRenderer> lines;
        public List<Transform> spheres;

        public HandObject handObject;

        private HandTracker ht;

        // public bool debugHoldJoints = false;
        // private List<Transform> heldJoints;

        private string[] handConnections = new string[]{
            "0,1,2,3,4",
            "0,5,6,7,8",
            "9,10,11,12",
            "13,14,15,16",
            "0,17,18,19,20",
            "5,9,13,17",
        };

        public void Init(HandObject handObject, HandTracker.HandTrackerMode mode){
            this.handObject = handObject;
            if(mode == HandTracker.HandTrackerMode.MODE_3D){
                Init3D();
            }
            else if(mode == HandTracker.HandTrackerMode.MODE_2D){
                Init2D();
            }

            this.ht = FindObjectOfType<HandTracker>();
        }

        public void Init3D(){
            var color = Color.white;//handedness == Handedness.Right ? Color.red : Color.green;
            foreach(var line in lines){
                line.material.color = color;
            }
            foreach(var sphere in spheres){
                sphere.GetComponent<MeshRenderer>().material.color = color;
            }
        }

        public void Init2D(){

            var color = Color.white;//handedness == Handedness.Right ? Color.red : Color.green;
            lines[0].material.color = color;
            lines[0].widthMultiplier = handObject.s2d.zDistance/1000;//0.025f;
            for(var i = 1; i < lines.Count; i++){
                Destroy(lines[i].gameObject);
            }
            for(var i = 0; i < spheres.Count; i++){
                Destroy(spheres[i].gameObject);
            }
        }

        public void SetData(List<Transform> joints){
            
            var lctr = 0;
            foreach(var connections in handConnections){
                var indices = connections.Split(new string[]{","}, System.StringSplitOptions.RemoveEmptyEntries);
                var pctr = 0;

                foreach(var indexStr in indices){
                    var index = int.Parse(indexStr);
                    lines[lctr].SetPosition(pctr, joints[index].position);  
                        pctr++;
                }
                lctr++;
            }
                                        
            var jctr = 0;
            foreach(var joint in joints){
                spheres[jctr].position = joint.position;
                spheres[jctr].rotation = joint.rotation;
                // Debug.Log("sphere-" + jctr + " at " + spheres[jctr].position);
                jctr++;
            }


            // CheckJoints();

        }

        public void SetData(List<Vector3> corners){
            var pctr = 0;
            foreach(var p in corners){
                lines[0].SetPosition(pctr, p);  
                pctr++;
            }
        }

        // public void CheckJoints(){
        //     for(var i = 0; i < handObject.s3d.joints.Count; i++){
        //         var actual = handObject.s3d.joints[i].localEulerAngles;
        //         var min = ht.ts3d.jointLimits[i].minAngle;
        //         var max = ht.ts3d.jointLimits[i].maxAngle;

        //         var color = GetColorScore(i, actual, min, max);

        //         spheres[i].GetComponent<MeshRenderer>().material.color = color;
        //     }
        // }

        // public void SetDataLinesOnly(List<Transform> joints){
            
        //     var lctr = 0;
        //     foreach(var connections in handConnections){
        //         var indices = connections.Split(new string[]{","}, System.StringSplitOptions.RemoveEmptyEntries);
        //         var pctr = 0;

        //         foreach(var indexStr in indices){
        //             var index = int.Parse(indexStr);
        //             lines[lctr].SetPosition(pctr, joints[index].position);  
        //                 pctr++;
        //         }
        //         lctr++;
        //     }
        // }

        // public void Update(){
        //     if(debugHoldJoints && heldJoints != null){
        //         for(var i = 0; i < heldJoints.Count; i++){
        //             heldJoints[i].position = spheres[i].position;
        //             heldJoints[i].rotation = spheres[i].rotation;
        //             SetDataLinesOnly(heldJoints);
        //         }
        //     }
        // }

        public static Color GetColorScore(int index, Vector3 actual, Vector3 min, Vector3 max)
        {
            if(min == Vector3.zero && max == Vector3.zero){
                return Color.white;
            }

            // Ensure min is less than max
            min = Vector3.Min(min, max);
            max = Vector3.Max(min, max);

            // Calculate the middle of the range
            Vector3 middle = (min + max) / 2;

            // Normalize the actual value relative to the min and max
            float xScore = NormalizeToRange(actual.x, min.x, max.x);
            float yScore = NormalizeToRange(actual.y, min.y, max.y);
            float zScore = NormalizeToRange(actual.z, min.z, max.z);
            // Debug.LogFormat("index[{0}] -> {1}=>{2}, {3}=>{4}, {5}=>{6}", index, actual.x, xScore, actual.y, yScore, actual.z, zScore);

            // Combine scores and calculate the final normalized average score
            float averageScore = (xScore + yScore + zScore) / 3f;

            // Lerp between colors based on the score
            // Below 0 -> Red, 0 to 0.5 -> Yellow, 0.5 to 1 -> Green
            if (averageScore < 0f)
            {
                return Color.red; // Outside bounds
            }
            else if (averageScore <= 0.5f)
            {
                return Color.Lerp(Color.red, Color.yellow, averageScore / 0.5f);
            }
            else if (averageScore <= 1f)
            {
                return Color.Lerp(Color.yellow, Color.green, (averageScore - 0.5f) / 0.5f);
            }
            else
            {
                return Color.red; // Outside bounds
            }
        }

        public static Color GetColorScore(int index, float actual, float min, float max)
        {
            float score = NormalizeToRange(actual, min, max);
            if (score < 0f)
            {
                return Color.red; // Outside bounds
            }
            else if (score <= 0.5f)
            {
                return Color.Lerp(Color.red, Color.yellow, score / 0.5f);
            }
            else if (score <= 1f)
            {
                return Color.Lerp(Color.yellow, Color.green, (score - 0.5f) / 0.5f);
            }
            else
            {
                return Color.red; // Outside bounds
            }
        }

        private static float NormalizeToRange(float value, float min, float max)
        {
            value = (value + 180f) % 360f - 180f;
            // Debug.Log("val = " + value);

            float middle = (min + max) / 2;//0
            // Debug.Log("min=" + min + ", max=" + max + ", middle = " + middle);

            float halfRange = Mathf.Abs(max - min) / 2;
            halfRange = Math.Max(halfRange, 5f); //allow degree error
            // Debug.Log("halfRange = " + halfRange);

            // Score is based on distance from the middle
            float distanceFromMiddle = Mathf.Abs(value - middle);
            // Debug.Log("distanceFromMiddle = " + distanceFromMiddle);
            // Debug.Log("score = " + (1f - (distanceFromMiddle / halfRange)));
            return 1f - (distanceFromMiddle / halfRange); // Closer to middle -> higher score
        }

        public void OnDrawGizmos(){
            // var cam = Camera.main;
            // if(cam == null) return;

            // var limits = ht.ts3d.jointLimits;
            // var joints = handObject.s3d.joints;

            // var jctr = 0;
            // foreach(var sphere in spheres){
                
                
            //     Gizmos.color = Color.cyan;
            //     Gizmos.DrawLine(sphere.position, cam.transform.position);

            //     var length = sphere.localScale.x * 2;
            //     var parentJoint = joints[jctr].parent;
            //     if(parentJoint == null) continue;


            //     var discRad = 0.01f;
            //     Handles.color = Color.red;
            //     var minX = limits[jctr].minAngle.x;
            //     var maxX = limits[jctr].maxAngle.x;
            //     var rangeX = Mathf.Max(maxX-minX, 1f);
            //     var fromX = Quaternion.AngleAxis(limits[jctr].minAngle.x, parentJoint.right) * parentJoint.forward;
            //     Handles.DrawSolidArc(sphere.position, parentJoint.right, fromX, rangeX, discRad);
            //     Handles.DrawWireDisc(sphere.position, sphere.right,  discRad);


            //     Handles.color = Color.green;
            //     var minY = limits[jctr].minAngle.y;
            //     var maxY = limits[jctr].maxAngle.y;
            //     var rangeY = Mathf.Max(maxY-minY, 1f);
            //     var fromY = Quaternion.AngleAxis(limits[jctr].minAngle.y, parentJoint.up) * parentJoint.forward;
            //     Handles.DrawSolidArc(sphere.position, parentJoint.up, fromY, rangeY, discRad);
            //     Handles.DrawWireDisc(sphere.position, sphere.up,  discRad);

            //     Handles.color = Color.blue;
            //     var minZ = limits[jctr].minAngle.z;
            //     var maxZ = limits[jctr].maxAngle.z;
            //     var rangeZ = Mathf.Max(maxZ-minZ, 1f);
            //     var fromZ = Quaternion.AngleAxis(limits[jctr].minAngle.z, parentJoint.forward) * parentJoint.right;
            //     Handles.DrawSolidArc(sphere.position, parentJoint.forward, fromZ, rangeZ, discRad);
            //     Handles.DrawWireDisc(sphere.position, sphere.forward,  discRad);


            //     Handles.color = Color.red;
            //     Handles.DrawLine(sphere.position, sphere.position + sphere.right * length, 3);

            //     Handles.color = Color.green;
            //     Handles.DrawLine(sphere.position, sphere.position + sphere.up * length, 3);

            //     Handles.color = Color.blue;
            //     Handles.DrawLine(sphere.position, sphere.position + sphere.forward * length, 3);
                

            //     jctr++;
            // }
        }
    }

}
