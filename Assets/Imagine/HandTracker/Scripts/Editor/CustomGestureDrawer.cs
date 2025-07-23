using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEditor.Events;
using System.Runtime.InteropServices;

namespace Imagine.WebAR
{
    [CustomPropertyDrawer(typeof(HandObject.HandObject3DSettings.CustomGesture))]
    public class CustomGestureDrawer : PropertyDrawer
    {
        // private bool isExpanded = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw foldout
            // property.isExpanded = EditorGUI.Foldout (position, property.isExpanded, label);
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                float singleLineHeight = EditorGUIUtility.singleLineHeight;
                float padding = EditorGUIUtility.standardVerticalSpacing;

                var nameProp = property.FindPropertyRelative("gestureName");
                var propHeight = singleLineHeight;
                Rect rect = new Rect(position.x, position.y, position.width, propHeight);
                EditorGUI.PropertyField(rect, nameProp, new GUIContent("Gesture Name"));
                position.y += propHeight + padding;

                var thumbUpProp = property.FindPropertyRelative("thumbUp");
                var indexUpProp = property.FindPropertyRelative("indexUp");
                var middleUpProp = property.FindPropertyRelative("middleUp");
                var ringUpProp = property.FindPropertyRelative("ringUp");
                var pinkyUpProp = property.FindPropertyRelative("pinkyUp");
                propHeight = singleLineHeight;
                rect = new Rect(position.x, position.y, position.width/3, propHeight);
                EditorGUI.LabelField(rect, "Finger Values");
                EditorGUIUtility.labelWidth = 20;

                rect = new Rect(position.x + position.width/4 + position.width/7.5f * 1, position.y, position.width/7.5f, propHeight);
                EditorGUI.PropertyField(rect, pinkyUpProp, new GUIContent("PI"));
                rect = new Rect(position.x + position.width/4 + position.width/7.5f * 2, position.y, position.width/7.5f, propHeight);
                EditorGUI.PropertyField(rect, ringUpProp, new GUIContent("RI"));
                rect = new Rect(position.x + position.width/4 + position.width/7.5f * 3, position.y, position.width/7.5f, propHeight);
                EditorGUI.PropertyField(rect, middleUpProp, new GUIContent("MI"));
                rect = new Rect(position.x + position.width/4 + position.width/7.5f * 4, position.y, position.width/7.5f, propHeight);
                EditorGUI.PropertyField(rect, indexUpProp, new GUIContent("IN"));
                rect = new Rect(position.x + position.width/4 + position.width/7.5f * 5, position.y, position.width/7.5f, propHeight);
                EditorGUI.PropertyField(rect, thumbUpProp, new GUIContent("TH"));
                position.y += propHeight + padding;


                // var style = new GUIStyle(EditorStyles.textArea)
                // {
                //     font = EditorStyles.wordWrappedLabel,
                //     richText = true
                // };
                var style = EditorStyles.wordWrappedLabel;
                style.font = Font.CreateDynamicFontFromOSFont("Courier New", 12);

                var thumbUp = thumbUpProp.boolValue;
                var indexUp = indexUpProp.boolValue;
                var middleUp = middleUpProp.boolValue;
                var ringUp = ringUpProp.boolValue;
                var pinkyUp = pinkyUpProp.boolValue;

                propHeight = singleLineHeight * 7;
                rect = new Rect(position.x, position.y, position.width - 120, propHeight);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextArea(rect, PrintHand(thumbUp, indexUp, middleUp, ringUp, pinkyUp), style);
                EditorGUI.EndDisabledGroup();    
                position.y += propHeight + padding*2;

                if(Application.IsPlaying(property.serializedObject.targetObject)){
                    //add test buttons
                    var buttonWidth = 120;
                    var buttonRect = new Rect(position.x + position.width - buttonWidth, position.y - 4*singleLineHeight, buttonWidth, singleLineHeight);
                    GUI.color = Color.yellow;

                    if (GUI.Button(buttonRect, "Test OnDetected")){
                        //find name
                        var gestureName = nameProp.stringValue;
                        var handObject = (HandObject)property.serializedObject.targetObject;
                        var customGesture = handObject.s3d.gestureEvents.customGestures.Find(g => g.gestureName == gestureName);
                        customGesture.OnDetected?.Invoke();
                    }
                    buttonRect = new Rect(position.x + position.width - buttonWidth, position.y - 3*singleLineHeight+padding, buttonWidth, singleLineHeight);
                    if (GUI.Button(buttonRect, "Test OnLost")){
                        //find name
                        var gestureName = nameProp.stringValue;
                        var handObject = (HandObject)property.serializedObject.targetObject;
                        var customGesture = handObject.s3d.gestureEvents.customGestures.Find(g => g.gestureName == gestureName);
                        customGesture.OnLost?.Invoke();
                    }
                    GUI.color = Color.white;
                }



                var eventProp = property.FindPropertyRelative("OnDetected");
                propHeight = EditorGUI.GetPropertyHeight(eventProp);
                rect = new Rect(position.x, position.y, position.width, propHeight);
                EditorGUI.PropertyField(rect, eventProp, new GUIContent("On Detected"));
                position.y += propHeight + padding;

                eventProp = property.FindPropertyRelative("OnLost");
                propHeight = EditorGUI.GetPropertyHeight(eventProp);
                rect = new Rect(position.x, position.y, position.width, propHeight);
                EditorGUI.PropertyField(rect, eventProp, new GUIContent("On Lost"));
                position.y += propHeight + padding;
            
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

            contentHeight += basicRowHeight; //name
            contentHeight += basicRowHeight; //bools
            contentHeight += basicRowHeight * 7; //ascii drawing

            contentHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OnDetected")) + padding; //event
            contentHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OnLost")) + padding; //event

            return contentHeight;
        }  

        public static string PrintHand(bool thumbUp, bool indexUp, bool middleUp, bool ringUp, bool pinkyUp)
        {
            string handCode = "";
            handCode += "   (r:_ )(m:. )(m:- )(m:. )(i:_ )     \n";
            handCode += " (p:_ )(r:| ) (rm:| ) (mi:| ) (i:| )    \n";
            handCode += "(p:| )(p: _)(pr:|.)(r: -)(rm:|.)(m: -)(mi:|.)(i: -)(i:|.) (t:, )(t:- )(t:, )\n";
            handCode += "|(p: _)|(r: _)|(m: _)|(IT:_ )|(t:/_) (t:/ ) \n";
            handCode += "|      (t: \\)(t: _) |  \n";
            handCode += " \\       /   \n";
            handCode += "  \\_____/    \n";

            var final = "";
            var vals = handCode.Split(new char[]{'(',')'});
            foreach(var val in vals){
                if(!val.Contains(":")){
                    final += val;
                }
                else{
                    var fields = val.Split(new char[]{':'});
                    if(fields[0] == "p"){
                        final += (fields[1][pinkyUp ? 0 : 1]);
                    }
                    else if(fields[0] == "pr"){
                        final += (fields[1][pinkyUp || ringUp ? 0 : 1]);
                    }
                    else if(fields[0] == "r"){
                        final += (fields[1][ringUp ? 0 : 1]);
                    }
                    else if(fields[0] == "rm"){
                        final += (fields[1][ringUp || middleUp ? 0 : 1]);
                    }
                    else if(fields[0] == "m"){
                        final += (fields[1][middleUp ? 0 : 1]);
                    }
                    else if(fields[0] == "mi"){
                        final += (fields[1][middleUp || indexUp ? 0 : 1]);
                    }
                     else if(fields[0] == "i"){
                        final += (fields[1][indexUp ? 0 : 1]);
                    }
                    else if(fields[0] == "t"){
                        final += (fields[1][thumbUp ? 0 : 1]);
                    }
                    else if(fields[0] == "IT"){
                        final += (fields[1][!indexUp || !thumbUp ? 0 : 1]);
                    }
                }
            }
            return final;
        }   
    }
}
