// using UnityEngine;
// using UnityEditor;
// using UnityEngine.Events;
// using System.Text.RegularExpressions;
// using System.Linq;
// using UnityEditor.Events;

// namespace Imagine.WebAR
// {
//     [CustomPropertyDrawer(typeof(HandObject.HandObject3DSettings.FingerClosenessEvents))]
//     public class FingerClosenessEventsDrawer : PropertyDrawer
//     {
//         private bool isExpanded = true;
//         // private static readonly string[] FingerNames = { "Index", "Middle", "Ring", "Pinky", "Thumb" };

//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             EditorGUI.BeginProperty(position, label, property);

//             // Draw foldout
//             isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), isExpanded, label);
//             position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

//             if (isExpanded)
//             {
//                 float singleLineHeight = EditorGUIUtility.singleLineHeight;
//                 float padding = EditorGUIUtility.standardVerticalSpacing;

//                 var properties = GetPropertiesArray(property);
               

//                 for (int i = 0; i < FingerNames.Length; i++)
//                 {
//                     var closedProp = properties[i * 2];
//                     var openedProp = properties[i * 2 + 1];
//                     var closedPropHeight = EditorGUI.GetPropertyHeight(closedProp);
//                     var openedPropHeight = EditorGUI.GetPropertyHeight(openedProp);

//                     Rect closedRect = new Rect(position.x, position.y, position.width / 2 - 5, singleLineHeight);
//                     Rect openedRect = new Rect(position.x + position.width / 2 + 5, position.y, position.width / 2 - 5, singleLineHeight);

//                     EditorGUI.PropertyField(closedRect, closedProp, new GUIContent($"On {FingerNames[i]} Finger Closed"));
//                     EditorGUI.PropertyField(openedRect, openedProp, new GUIContent($"On {FingerNames[i]} Finger Opened"));

//                     var rowHeight = Mathf.Max(closedPropHeight, openedPropHeight);

//                     position.y += rowHeight + padding;
//                 }

//                 Rect buttonRect = new Rect(position.x, position.y, position.width, singleLineHeight);
//                 if (GUI.Button(buttonRect, "Auto Set"))
//                 {
//                     AutoSetEvents(property);
//                 }
//                 position.y += singleLineHeight + padding;

//                 buttonRect = new Rect(position.x, position.y, position.width, singleLineHeight);
//                 if (GUI.Button(buttonRect, "Clear All"))
//                 {
//                    //Clear Events
//                    ClearEvents(property);
//                 }
//             }

//             EditorGUI.EndProperty();
//         }

//         public void AutoSetEvents(SerializedProperty property){

//             var handObjectSO = property.serializedObject;
//             var handObject = (HandObject)handObjectSO.targetObject;
//             var animator = handObject.s3d.handAnimator;

//             if(animator == null){
//                 EditorUtility.DisplayDialog("No Animator Found", "Please set your HandObject>3D Settings>Hand Animator first", "Got it");
//                 return;
//             }


//             var e = handObject.s3d.fingerClosenessEvents;
//             UnityEventTools.AddStringPersistentListener(e.OnIndexFingerClosed,  animator.ResetTrigger, "IndexOpened");
//             UnityEventTools.AddStringPersistentListener(e.OnIndexFingerClosed,  animator.SetTrigger, "IndexClosed");

//             UnityEventTools.AddStringPersistentListener(e.OnIndexFingerOpened,  animator.ResetTrigger, "IndexOpened");
//             UnityEventTools.AddStringPersistentListener(e.OnIndexFingerOpened,  animator.SetTrigger, "IndexOpened");

//             UnityEventTools.AddStringPersistentListener(e.OnMiddleFingerClosed,  animator.ResetTrigger, "MiddleOpened");
//             UnityEventTools.AddStringPersistentListener(e.OnMiddleFingerClosed, animator.SetTrigger, "MiddleClosed");

//             UnityEventTools.AddStringPersistentListener(e.OnMiddleFingerOpened,  animator.ResetTrigger, "MiddleClosed");
//             UnityEventTools.AddStringPersistentListener(e.OnMiddleFingerOpened, animator.SetTrigger, "MiddleOpened");

//             UnityEventTools.AddStringPersistentListener(e.OnRingFingerClosed,  animator.ResetTrigger, "RingOpened");
//             UnityEventTools.AddStringPersistentListener(e.OnRingFingerClosed,   animator.SetTrigger, "RingClosed");

//             UnityEventTools.AddStringPersistentListener(e.OnRingFingerOpened,  animator.ResetTrigger, "RingClosed");
//             UnityEventTools.AddStringPersistentListener(e.OnRingFingerOpened,   animator.SetTrigger, "RingOpened");

//             UnityEventTools.AddStringPersistentListener(e.OnPinkyFingerClosed,  animator.ResetTrigger, "PinkyOpened");
//             UnityEventTools.AddStringPersistentListener(e.OnPinkyFingerClosed,  animator.SetTrigger, "PinkyClosed");

//             UnityEventTools.AddStringPersistentListener(e.OnPinkyFingerOpened,  animator.ResetTrigger, "PinkyClosed");
//             UnityEventTools.AddStringPersistentListener(e.OnPinkyFingerOpened,  animator.SetTrigger, "PinkyOpened");

//             UnityEventTools.AddStringPersistentListener(e.OnThumbFingerClosed,  animator.ResetTrigger, "ThumbOpened");
//             UnityEventTools.AddStringPersistentListener(e.OnThumbFingerClosed,  animator.SetTrigger, "ThumbClosed");

//             UnityEventTools.AddStringPersistentListener(e.OnThumbFingerOpened,  animator.ResetTrigger, "ThumbClosed");
//             UnityEventTools.AddStringPersistentListener(e.OnThumbFingerOpened,  animator.SetTrigger, "ThumbOpened");

//             EditorUtility.SetDirty(handObject);

//             EditorUtility.DisplayDialog(
//                 "Events Added", 
//                 "The following animator tiggers has been added to Animator " + animator.name + ":\n\n" +
//                 "• IndexClosed\t• IndexOpened\n" + 
//                 "• MiddleClosed\t• MiddleOpened\n" + 
//                 "• RingClosed\t• RingOpened\n" + 
//                 "• PinkyClosed\t• PinkyOpened\n" + 
//                 "• ThumbClosed\t• ThumbOpened\n"
//                 , 
//                 "Got it");

//         }

//         void ClearEvents(SerializedProperty property){
//             var handObjectSO = property.serializedObject;
//             var handObject = (HandObject)handObjectSO.targetObject;
//             var animator = handObject.s3d.handAnimator;

//             if(animator == null){
//                 EditorUtility.DisplayDialog("No Animator Found", "Nothing to clear. Please set your HandObject>3D Settings>Hand Animator first", "Got it");
//                 return;
//             }

//             if(EditorUtility.DisplayDialog("Confirm Clearing Event Data", 
//             "Warning: This will delete all listeners registered in all finger closeness events.", "Proceed", "Cancel")
//             ){
//                 var e = handObject.s3d.fingerClosenessEvents;
//                 RemoveAllPersistentListeners(e.OnIndexFingerClosed);
//                 RemoveAllPersistentListeners(e.OnIndexFingerOpened);
//                 RemoveAllPersistentListeners(e.OnMiddleFingerClosed);
//                 RemoveAllPersistentListeners(e.OnMiddleFingerOpened);
//                 RemoveAllPersistentListeners(e.OnRingFingerClosed);
//                 RemoveAllPersistentListeners(e.OnRingFingerOpened);
//                 RemoveAllPersistentListeners(e.OnPinkyFingerClosed);
//                 RemoveAllPersistentListeners(e.OnPinkyFingerOpened);
//                 RemoveAllPersistentListeners(e.OnThumbFingerClosed);
//                 RemoveAllPersistentListeners(e.OnThumbFingerOpened);
//             }
//         }

//         void RemoveAllPersistentListeners(UnityEvent e){
//             var total = e.GetPersistentEventCount();
//             Debug.Log("total = " + total);

//             for(var i = total-1; i >= 0; i--){
//                 Debug.Log("removing i =  " + i);
//                 UnityEventTools.RemovePersistentListener(e, i);
//             }
//         }

//         SerializedProperty[] GetPropertiesArray(SerializedProperty property){
//             SerializedProperty[] properties =
//             {
//                 property.FindPropertyRelative("OnIndexFingerClosed"),
//                 property.FindPropertyRelative("OnIndexFingerOpened"),
//                 property.FindPropertyRelative("OnMiddleFingerClosed"),
//                 property.FindPropertyRelative("OnMiddleFingerOpened"),
//                 property.FindPropertyRelative("OnRingFingerClosed"),
//                 property.FindPropertyRelative("OnRingFingerOpened"),
//                 property.FindPropertyRelative("OnPinkyFingerClosed"),
//                 property.FindPropertyRelative("OnPinkyFingerOpened"),
//                 property.FindPropertyRelative("OnThumbFingerClosed"),
//                 property.FindPropertyRelative("OnThumbFingerOpened")
//             };

//             return properties;
//         }

//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         {
//             if (!isExpanded)
//             {
//                 return EditorGUIUtility.singleLineHeight;
//             }

//             float padding = EditorGUIUtility.standardVerticalSpacing;

//             // float lineHeight = EditorGUIUtility.singleLineHeight;

//             var properties = GetPropertiesArray(property);
//             var contentHeight = 0f;
//             for (int i = 0; i < 5; i++)
//             {
//                 var closedProp = properties[i * 2];
//                 var openedProp = properties[i * 2 + 1];
//                 var closedPropHeight = EditorGUI.GetPropertyHeight(closedProp);
//                 var openedPropHeight = EditorGUI.GetPropertyHeight(openedProp);

//                 var rowHeight = Mathf.Max(closedPropHeight, openedPropHeight);
//                 contentHeight += rowHeight + padding;
//             }

//             return (EditorGUIUtility.singleLineHeight + padding) * 3 + contentHeight;
//         }

//         public static SerializedProperty FindParentProperty(SerializedProperty serializedProperty)
//         {
//             var propertyPaths = serializedProperty.propertyPath.Split('.');
//             if (propertyPaths.Length <= 1)
//             {
//                 return default;
//             }

//             // Start with the serialized object itself
//             var parentSerializedProperty = serializedProperty.serializedObject.FindProperty(propertyPaths[0]);
            
//             for (int index = 1; index < propertyPaths.Length - 1; index++)
//             {
//                 // Check if the current property is an array element
//                 if (propertyPaths[index] == "Array" && propertyPaths.Length > index + 1 && Regex.IsMatch(propertyPaths[index + 1], "^data\\[\\d+\\]$"))
//                 {
//                     var match = Regex.Match(propertyPaths[index + 1], "^data\\[(\\d+)\\]$");
//                     if (match.Success)
//                     {
//                         var arrayIndex = int.Parse(match.Groups[1].Value);
//                         parentSerializedProperty = parentSerializedProperty.GetArrayElementAtIndex(arrayIndex);
//                         index++; // Skip the next path component which is already processed
//                     }
//                 }
//                 else
//                 {
//                     // Find the relative property for non-array elements
//                     parentSerializedProperty = parentSerializedProperty.FindPropertyRelative(propertyPaths[index]);
//                 }
//             }

//             return parentSerializedProperty;
//         }

//         // public static UnityEvent ToUnityEvent(SerializedProperty property)
//         // {
//         //     var targetObject = property.serializedObject.targetObject;
//         //     var field = targetObject.GetType().GetField(property.propertyPath, 
//         //         System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
//         //     return (UnityEvent)field.GetValue(targetObject);
//         // }

//         // public static Animator ToAnimator(SerializedProperty property)
//         // {
//         //     var targetObject = property.serializedObject.targetObject;
//         //     var field = targetObject.GetType().GetField(property.propertyPath, 
//         //         System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
//         //     return (Animator)field.GetValue(targetObject);
//         // }

//         // public static Animator ToAnimator(SerializedProperty property)
//         // {
//         //     var targetObject = property.serializedObject.targetObject;
//         //     var currentObject = targetObject;

//         //     // Split the property path into components
//         //     foreach (var path in property.propertyPath.Split('.'))
//         //     {
//         //         // Get the member for the current path component
//         //         var member = currentObject.GetType().GetMember(path, 
//         //             BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
//         //             .FirstOrDefault();

//         //         if (member is FieldInfo field)
//         //         {
//         //             currentObject = field.GetValue(currentObject);
//         //         }
//         //         else if (member is PropertyInfo propertyInfo)
//         //         {
//         //             currentObject = propertyInfo.GetValue(currentObject);
//         //         }
//         //         else
//         //         {
//         //             throw new System.InvalidOperationException($"Unable to find field or property: {path}");
//         //         }
//         //     }

//         //     // Cast the final object to Animator, ensuring it is a UnityEngine.Object
//         //     return currentObject as Animator;
//         // }

//         public static T ChangeType<T>(SerializedProperty property)
//         {
//             var targetObject = property.serializedObject.targetObject;
//             Debug.Log("tyoe = " + targetObject.GetType());
//             var field = targetObject.GetType().GetField(property.propertyPath, 
//                 System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
//             return (T)field.GetValue(targetObject);
//         }

        
//     }
// }
