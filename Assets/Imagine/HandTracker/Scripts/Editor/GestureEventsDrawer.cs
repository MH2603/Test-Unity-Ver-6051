using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEditor.Events;

namespace Imagine.WebAR
{
    [CustomPropertyDrawer(typeof(HandObject.HandObject3DSettings.GestureEvents))]
    public class FingerClosenessEventsDrawer : PropertyDrawer
    {
        private bool isExpanded = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw foldout
            isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), isExpanded, label);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (isExpanded)
            {
                float singleLineHeight = EditorGUIUtility.singleLineHeight;
                float padding = EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel++;

                var manualIndent = EditorGUI.indentLevel * 15;

                var onFingerClosedGestureProp = property.FindPropertyRelative("OnFingerClosedGesture");
                var propHeight = GetPropertyHeight(onFingerClosedGestureProp);
                Rect rect = new Rect(position.x + manualIndent, position.y, position.width - manualIndent, propHeight);
                EditorGUI.PropertyField(rect, onFingerClosedGestureProp, new GUIContent("On Finger Closed"));

                position.y += propHeight;

                if(Application.IsPlaying(property.serializedObject.targetObject)){
                    GUI.color = Color.yellow;
                    var buttonPosX = position.x - manualIndent;
                    var buttonWidth = (position.width + manualIndent) / 6;
                    GUIStyle smallFontStyle = new GUIStyle(EditorStyles.label);
                    smallFontStyle.fontSize = 8; // Adjust font size as needed
                    
                    Rect buttonRect = new Rect(buttonPosX, position.y, buttonWidth, singleLineHeight);
                    EditorGUI.LabelField(buttonRect, new GUIContent("Test Events"), smallFontStyle);

                    var handObject = (HandObject)property.serializedObject.targetObject;

                    GUIStyle smallFontButton = new GUIStyle(GUI.skin.button);
                    smallFontButton.fontSize = 8; // Adjust size as needed

                    buttonRect = new Rect(buttonPosX + 1 * buttonWidth, position.y, buttonWidth-5, singleLineHeight);
                    EditorGUI.BeginChangeCheck();
                    handObject.s3d.fingerClosedBools["Thumb"] = GUI.Toggle(buttonRect, handObject.s3d.fingerClosedBools["Thumb"], "ThumbClosed", smallFontButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        handObject.s3d.gestureEvents.OnFingerClosedGesture?.Invoke("ThumbClosed",  handObject.s3d.fingerClosedBools["Thumb"]);
                    }
                    buttonRect = new Rect(buttonPosX + 2 * buttonWidth, position.y, buttonWidth-5, singleLineHeight);
                    EditorGUI.BeginChangeCheck();
                    handObject.s3d.fingerClosedBools["Index"] = GUI.Toggle(buttonRect, handObject.s3d.fingerClosedBools["Index"], "IndexClosed", smallFontButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        handObject.s3d.gestureEvents.OnFingerClosedGesture?.Invoke("IndexClosed",  handObject.s3d.fingerClosedBools["Index"]);
                    }
                    buttonRect = new Rect(buttonPosX + 3 * buttonWidth, position.y, buttonWidth-5, singleLineHeight);
                    EditorGUI.BeginChangeCheck();
                    handObject.s3d.fingerClosedBools["Middle"] = GUI.Toggle(buttonRect, handObject.s3d.fingerClosedBools["Middle"], "MiddleClosed", smallFontButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        handObject.s3d.gestureEvents.OnFingerClosedGesture?.Invoke("MiddleClosed",  handObject.s3d.fingerClosedBools["Middle"]);
                    }
                    buttonRect = new Rect(buttonPosX + 4 * buttonWidth, position.y, buttonWidth-5, singleLineHeight);
                    EditorGUI.BeginChangeCheck();
                    handObject.s3d.fingerClosedBools["Ring"] = GUI.Toggle(buttonRect, handObject.s3d.fingerClosedBools["Ring"], "RingClosed", smallFontButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        handObject.s3d.gestureEvents.OnFingerClosedGesture?.Invoke("RingClosed",  handObject.s3d.fingerClosedBools["Ring"]);
                    }
                    buttonRect = new Rect(buttonPosX + 5 * buttonWidth, position.y, buttonWidth-5, singleLineHeight);
                    EditorGUI.BeginChangeCheck();
                    handObject.s3d.fingerClosedBools["Pinky"] = GUI.Toggle(buttonRect, handObject.s3d.fingerClosedBools["Pinky"], "PinkyClosed", smallFontButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        handObject.s3d.gestureEvents.OnFingerClosedGesture?.Invoke("PinkyClosed",  handObject.s3d.fingerClosedBools["Pinky"]);
                    }

                    position.y += singleLineHeight + padding;
                    GUI.color = Color.white;
                }

                position.y += singleLineHeight + padding;


                // Rect buttonRect = new Rect(position.x + manualIndent, position.y, position.width - manualIndent, singleLineHeight);
                // if (GUI.Button(buttonRect, "Auto Set Finger Closed Gesture"))
                // {
                //     // AutoSetEvents(property);
                // }
                // position.y += singleLineHeight + padding;

                // buttonRect = new Rect(position.x + manualIndent, position.y, position.width - manualIndent, singleLineHeight);
                // if (GUI.Button(buttonRect, "Clear Finger Closed Gesture"))
                // {
                //    //Clear Events
                //    ClearEvents(property);
                // }
                // position.y += singleLineHeight + padding;


                //////////custom gestures
                
                // EditorGUI.indentLevel++;

                var customGestureProps = property.FindPropertyRelative("customGestures");
                propHeight = GetPropertyHeight(customGestureProps);
                rect = new Rect(
                    position.x, 
                    position.y, 
                    position.width,
                    propHeight
                );
                EditorGUI.PropertyField(rect, customGestureProps, new GUIContent("Custom Gestures"));
                position.y += propHeight;

                var OnAllGesturesLostProp = property.FindPropertyRelative("OnAllGesturesLost");
                propHeight = GetPropertyHeight(OnAllGesturesLostProp);
                rect = new Rect(position.x, position.y, position.width, propHeight);
                EditorGUI.PropertyField(rect, OnAllGesturesLostProp, new GUIContent("On All Gestures Lost"));

                position.y += propHeight;

                EditorGUI.indentLevel--;
                // EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public void AutoSetEvents(SerializedProperty property){

            // var handObjectSO = property.serializedObject;
            // var handObject = (HandObject)handObjectSO.targetObject;
            // var animator = handObject.s3d.handAnimator;

            // if(animator == null){
            //     EditorUtility.DisplayDialog("No Animator Found", "Please set your HandObject>3D Settings>Hand Animator first", "Got it");
            //     return;
            // }

            // UnityEventTools.AddPersistentListener(
            //     handObject.s3d.gestureEvents.OnFingerClosedGesture,  
            //     animator.SetBool);

            // EditorUtility.SetDirty(handObject);

            // EditorUtility.DisplayDialog(
            //     "Events Added", 
            //     "The following animator bool tiggers should now be received by your Animator " + animator.name + ":\n\n" +
            //     "• IndexClosed\n" + 
            //     "• MiddleClosed\n" + 
            //     "• RingClosed\n" + 
            //     "• PinkyClosed\n" + 
            //     "• ThumbClosed\n"
            //     , 
            //     "Got it");

        }

        void ClearEvents(SerializedProperty property){
            // var handObject = (HandObject)property.serializedObject.targetObject;

            // if(EditorUtility.DisplayDialog("Confirm Clearing Event Data", 
            // "Warning: This will delete all listeners registered in OnFingerClosedGesture.", "Proceed", "Cancel")
            // ){
            //     RemoveAllPersistentListeners(handObject.s3d.gestureEvents.OnFingerClosedGesture);
            // }
        }

        void RemoveAllPersistentListeners(UnityEvent<string, bool> e){
            var total = e.GetPersistentEventCount();
            Debug.Log("total = " + total);

            for(var i = total-1; i >= 0; i--){
                Debug.Log("removing i =  " + i);
                UnityEventTools.RemovePersistentListener(e, i);
            }
        }


        public float GetPropertyHeight(SerializedProperty property){
            float padding = EditorGUIUtility.standardVerticalSpacing;
            return EditorGUI.GetPropertyHeight(property) + padding;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float padding = EditorGUIUtility.standardVerticalSpacing;
            float basicRowHeight = EditorGUIUtility.singleLineHeight + padding;

            if (!isExpanded)
            {
                return basicRowHeight;
            }

            var contentHeight = 0f;
            contentHeight += GetPropertyHeight(property.FindPropertyRelative("OnFingerClosedGesture"));
            if(Application.IsPlaying(property.serializedObject.targetObject)){
                contentHeight += basicRowHeight; //Test OnFingerClosedGesture Buttons
            }
            contentHeight += basicRowHeight; //spacing

            contentHeight += GetPropertyHeight(property.FindPropertyRelative("customGestures"));
            contentHeight += GetPropertyHeight(property.FindPropertyRelative("OnAllGesturesLost"));

            return basicRowHeight + contentHeight;
        }     
    }
}
