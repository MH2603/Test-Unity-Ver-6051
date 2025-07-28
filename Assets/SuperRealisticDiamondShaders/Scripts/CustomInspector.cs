using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
namespace DiamondRender
{
    [CustomEditor(typeof(DiamondRenderer)), CanEditMultipleObjects]
    public class CustomInspector : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("CalculateMesh"))
            {
                foreach (var obj in targets)
                {
                    DiamondRenderer ed = (DiamondRenderer)obj;
                    ed.Setup();
                }
            }

        }

    }
}
#endif


