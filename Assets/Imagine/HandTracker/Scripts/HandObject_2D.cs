using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Imagine.WebAR{
    public partial class HandObject
    {
        [System.Serializable]
        public class HandObject2DSettings{
            public float zDistance = 5;
            public UnityEvent<float> OnPosXChanged, OnPosYChanged;
            public UnityEvent<Vector2> OnPosChanged;
        }

        public HandObject2DSettings s2d;


#if UNITY_EDITOR
        void OnDrawGizmos2D(){
            Gizmos.color = Color.white;//handedness == Handedness.Right ? Color.red : Color.green;
            var l = 0.3f;
            var p1 = transform.position + new Vector3(-l, l, 0);
            var p2 = transform.position + new Vector3(-l, -l, 0);
            var p3 = transform.position + new Vector3(l, -l, 0);
            var p4 = transform.position + new Vector3(l, l, 0);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);

            Gizmos.color = new Color(1, 1, 1, 0.2f);//handedness == Handedness.Right ? new Color(1, 0.8f, 0.8f, 0.2f) : new Color(0.8f, 1, 0.8f, 0.2f);
            Gizmos.DrawMesh(HandTracker.EditorSceneViewHand2dMesh, 0, transform.position, transform.rotation);
        }
#endif
    }
}

