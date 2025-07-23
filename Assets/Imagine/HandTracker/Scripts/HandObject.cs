using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Imagine.WebAR{


    public partial class HandObject : MonoBehaviour
    {
        
        // public Handedness handedness;

        [Range(0,3)] public int handIndex = 0;
        [HideInInspector] public Handedness handedness;

#if UNITY_EDITOR
        void OnDrawGizmos(){
            if(HandTracker.EditorTrackerMode == HandTracker.HandTrackerMode.MODE_3D){
                OnDrawGizmos3D();
            }
            else if(HandTracker.EditorTrackerMode == HandTracker.HandTrackerMode.MODE_2D){
                OnDrawGizmos2D();
            }
        }
#endif

    }
}
