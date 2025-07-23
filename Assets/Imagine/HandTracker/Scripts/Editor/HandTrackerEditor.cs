using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Imagine.WebAR.Editor{
    [CustomEditor(typeof(HandTracker))]
    public class HandTrackerEditor : UnityEditor.Editor
    {
        HandTracker _target;
        void OnEnable(){
            _target = (HandTracker)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawEditorDebugger();

            serializedObject.ApplyModifiedProperties();
        }

        bool showKeyboardCameraControls = false;
        void DrawEditorDebugger(){
            //Editor Runtime Debugger
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor Debug Mode");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if(Application.IsPlaying(_target)){
                //Enable Disable
                foreach(var handObject in _target.handObjects.Values)
                {
                    if(handObject != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var name = handObject.handIndex + " - "  + handObject.gameObject.name;
                        EditorGUILayout.LabelField(name);

                        var handFound = handObject.gameObject.activeInHierarchy;

                        GUI.enabled = !handFound;
                        if(GUILayout.Button("Found")){
                            _target.SendMessage("OnHandFound", handObject.handIndex);

                            var cam = ((ARCamera)serializedObject.FindProperty("trackerCam").objectReferenceValue).transform;

                            cam.transform.position = handObject.transform.position + handObject.transform.forward * -1.5f;
                            cam.LookAt(handObject.transform);
                        }
                        GUI.enabled = handFound;
                        if(GUILayout.Button("Lost")){
                             _target.SendMessage("OnHandLost", handObject.handIndex);

                        }
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();
                    }
                        
                }    

                  
            }
            else{
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("Enter Play-mode to Debug In Editor");
                GUI.color = Color.white;
            }

            EditorGUILayout.Space();
            //keyboard camera controls
            showKeyboardCameraControls = EditorGUILayout.Toggle ("Show Keyboard Camera Controls", showKeyboardCameraControls);
            if(showKeyboardCameraControls){
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("W", "Move Forward (Z)");
                EditorGUILayout.LabelField("S", "Move Backward (Z)");
                EditorGUILayout.LabelField("A", "Move Left (X)");
                EditorGUILayout.LabelField("D", "Move Right (X)");
                EditorGUILayout.LabelField("R", "Move Up (Y)");
                EditorGUILayout.LabelField("F", "Move Down (Y)");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Up Arrow", "Tilt Up (along X-Axis)");
                EditorGUILayout.LabelField("Down Arrow", "Tilt Down (along X-Axis)");
                EditorGUILayout.LabelField("Left Arrow", "Tilt Left (along Y-Axis)");
                EditorGUILayout.LabelField("Right Arrow", "Tilt Right (Along Y-Axis)");
                EditorGUILayout.LabelField("Period", "Tilt Clockwise (Along Z-Axis)");
                EditorGUILayout.LabelField("Comma", "Tilt Counter Clockwise (Along Z-Axis)");
                EditorGUILayout.Space(40);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugCamMoveSensitivity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugCamTiltSensitivity"));
                EditorGUILayout.EndVertical();
                
            }    

            EditorGUILayout.EndVertical();
        }
    }
}

