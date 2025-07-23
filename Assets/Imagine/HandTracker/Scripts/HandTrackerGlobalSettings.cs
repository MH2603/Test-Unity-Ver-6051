using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Imagine.WebAR{

    public class HandTrackerGlobalSettings : ScriptableObject
    {
        [SerializeField][Range(1,4)] public int maxHands = 2;
        [SerializeField] public bool dontOverrideHandCount = false;
        [Space(10)]
        [SerializeField][Range(0.01f,1)] public float detectConfidence = 0.3f;
        [SerializeField][Range(0.01f,1)] public float presenceConfidence = 0.3f;
        [SerializeField][Range(0.01f,1)] public float trackConfidence = 0.3f;
        [SerializeField] public bool dontOverrideConfidence = false;
        
        
        private static HandTrackerGlobalSettings _instance;
        public static HandTrackerGlobalSettings Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Resources.Load<HandTrackerGlobalSettings>("HandTrackerGlobalSettings");
                }
                return _instance;

            }
        }
    }
}
