using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Imagine.WebAR;

namespace Imagine.WebAR.Editor{
    [CustomPropertyDrawer(typeof(HandObject.HandObject3DSettings))]
    public class HandObject3DSettingsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw foldout
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                

                float singleLineHeight = EditorGUIUtility.singleLineHeight;
                float padding = EditorGUIUtility.standardVerticalSpacing;

                var jointsProp = property.FindPropertyRelative("joints");
                var propHeight = EditorGUI.GetPropertyHeight(jointsProp) + padding;
                Rect rect = new Rect(position.x, position.y, position.width, propHeight);
                EditorGUI.PropertyField(rect, jointsProp, new GUIContent("Joints"));
                position.y += propHeight + padding;

                // EditorGUILayout.Space();

                var useHandMeshProp = property.FindPropertyRelative("useHandMesh");
                propHeight = singleLineHeight + padding;
                rect = new Rect(position.x, position.y, position.width, propHeight);
                EditorGUI.PropertyField(rect, useHandMeshProp, new GUIContent("Use Hand Mesh"));
                position.y += propHeight + padding;

                if(useHandMeshProp.boolValue){
                    EditorGUI.indentLevel++;
                    var handMeshProp = property.FindPropertyRelative("handMesh");
                    propHeight = singleLineHeight + padding;
                    rect = new Rect(position.x, position.y, position.width, propHeight);
                    EditorGUI.PropertyField(rect, handMeshProp, new GUIContent("HandMesh"));
                    position.y += propHeight + padding;
                    EditorGUI.indentLevel--;
                }

                var useGesturesProp = property.FindPropertyRelative("useGestures");
                propHeight = singleLineHeight + padding;
                rect = new Rect(position.x, position.y, position.width, propHeight);
                EditorGUI.PropertyField(rect, useGesturesProp, new GUIContent("Use Gestures"));
                position.y += propHeight + padding;

                if(useGesturesProp.boolValue){
                    EditorGUI.indentLevel++;

                    var closenessThresholdsProp = property.FindPropertyRelative("closenessThresholds");
                    propHeight = EditorGUI.GetPropertyHeight(closenessThresholdsProp) + padding;
                    rect = new Rect(position.x, position.y, position.width, propHeight);
                    EditorGUI.PropertyField(rect, closenessThresholdsProp, new GUIContent("Finger Closeness Thresholds"), true);
                    position.y += propHeight + padding;

                    var debounceTimeProp = property.FindPropertyRelative("debounceTime");
                    propHeight = singleLineHeight + padding;
                    rect = new Rect(position.x, position.y, position.width, propHeight);
                    EditorGUI.PropertyField(rect, debounceTimeProp, new GUIContent("Debounce Time"));
                    position.y += propHeight + padding;

                    var gestureEventsProp = property.FindPropertyRelative("gestureEvents");
                    propHeight = EditorGUI.GetPropertyHeight(gestureEventsProp) + padding;
                    rect = new Rect(position.x, position.y, position.width, propHeight);
                    EditorGUI.PropertyField(rect, gestureEventsProp, new GUIContent("Gesture Events"), true);
                    position.y += propHeight + padding;
                    
                    
                    
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float padding = EditorGUIUtility.standardVerticalSpacing;
            float basicRowHeight = EditorGUIUtility.singleLineHeight + padding;

            if (!property.isExpanded)
            {
                return basicRowHeight;
            }

            var contentHeight = 0f;
            contentHeight += basicRowHeight; //dropdown header

            contentHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("joints")) + padding; //joints            
            
            contentHeight += basicRowHeight; //useHandMesh
            if(property.FindPropertyRelative("useHandMesh").boolValue){
                contentHeight += basicRowHeight; //handMesh
            }

            contentHeight += basicRowHeight; //useGestures
            if(property.FindPropertyRelative("useGestures").boolValue){
                contentHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("closenessThresholds")) + padding; //finger closeness thresholds
                contentHeight += basicRowHeight; //debounce time            
                contentHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("gestureEvents")) + padding; //gesture events

            }

            return contentHeight;
        }  
    }
}

