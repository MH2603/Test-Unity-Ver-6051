using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Imagine.WebAR;
using System.Runtime.InteropServices;

namespace Imagine.WebAR.Editor
{
    [CustomEditor(typeof(HandObject))]
    public class HandObject3D_Editor : UnityEditor.Editor
    {
        HandObject _target;
        GameObject handMeshPrefab;

        bool lastUseHandMeshVal, lastUseCanonicalHandAnimationsVal;

        void OnEnable(){
            _target = (HandObject)target;
            handMeshPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Imagine/HandTracker/Prefabs/HandMesh.prefab");

            lastUseHandMeshVal = _target.s3d.useHandMesh;
            lastUseCanonicalHandAnimationsVal = _target.s3d.useGestures;


            CheckAndFixJoints();
        }

        public override void OnInspectorGUI(){
            // DrawDefaultInspector();
            GUILayout.Space(20);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("handIndex"));
            

            if(HandTracker.EditorTrackerMode == HandTracker.HandTrackerMode.MODE_3D){

                EditorGUILayout.PropertyField(serializedObject.FindProperty("s3d"), new GUIContent("3D Settings"));
                if(lastUseHandMeshVal != _target.s3d.useHandMesh){
                    ToggleHandMesh();
                    lastUseHandMeshVal = _target.s3d.useHandMesh;
                    SceneView.RepaintAll();
                }
                if(lastUseCanonicalHandAnimationsVal != _target.s3d.useGestures){
                    ToggleUseCanonicalAnimation();
                    lastUseCanonicalHandAnimationsVal = _target.s3d.useGestures;
                    SceneView.RepaintAll();
                }
                EditorGUILayout.Space(20);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dbgS3d"), new GUIContent("Debug 3D Settings"));
                
            }
            else if(HandTracker.EditorTrackerMode == HandTracker.HandTrackerMode.MODE_2D){
                EditorGUILayout.PropertyField(serializedObject.FindProperty("s2d"), new GUIContent("2D Settings"));
            }

            

            serializedObject.ApplyModifiedProperties();
        }

        // private static string ToHex(Color c){
        //     return "#" + ColorUtility.ToHtmlStringRGB(c);
        // }

        void ToggleHandMesh(){
            if(!_target.s3d.useHandMesh){
                if(_target.s3d.handMesh != null){
                    DestroyImmediate(_target.s3d.handMesh.gameObject);
                    _target.s3d.handMesh = null;
                }
                return;
            }
            else{
                var prefab = handMeshPrefab;//_target.handedness == Handedness.Left ? leftPrefab : rightPrefab;
                var go = Instantiate(prefab, _target.transform);
                var skinnedMesh = go.GetComponent<SkinnedMeshRenderer>();
                _target.s3d.handMesh = skinnedMesh;
                skinnedMesh.rootBone = _target.s3d.joints[0];

                int[] boneIndices = new int[]{0,5,6,7,8,9,10,11,12,13,14,15,16,1,2,3,4,17,18,19,20};
                var bones = new List<Transform>();
                foreach(var boneIndex in boneIndices){
                    bones.Add(_target.s3d.joints[boneIndex]);
                }
                skinnedMesh.bones = bones.ToArray();
            }
        }

        void ToggleUseCanonicalAnimation(){
            // if(!_target.s3d.useGestures){
            //     if(_target.s3d.handAnimator != null)
            //         _target.s3d.handAnimator.enabled = false;
            //     _target.s3d.handAnimator = null;
            //     return;
            // }
            // else{
            //     if(_target.s3d.handAnimator == null){
            //         var handAnimator = _target.gameObject.GetComponent<Animator>();
            //         if(handAnimator == null)
            //             handAnimator = _target.gameObject.AddComponent<Animator>();
            //         handAnimator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            //             "Assets/Imagine/HandTracker/Animations/CanonicalHand.controller");
            //         _target.s3d.handAnimator = handAnimator;
            //         handAnimator.enabled = true;
            //     }
            // }
        }

        void CheckAndFixJoints(){
            if(HandTracker.EditorTrackerMode == HandTracker.HandTrackerMode.MODE_3D){
                //check joints
                var joints = _target.s3d.joints;
                bool error = false;
                if(joints.Count < 21)
                    error = true;
                foreach(var joint in joints){
                    if(joint == null){
                        error = true;
                    }
                }

                if(error){
                    //let's try fixing
                    
                    //make sure we have 21 joints
                    while(joints.Count < 21){
                        joints.Add(null);
                    }

                    //fix nulls
                    if(joints[0] == null){
                        joints[0] = _target.transform.Find("Joint_0");
                    }
                    if(joints[1] == null){
                        joints[1] = _target.transform.Find("Joint_0/Joint_1");
                    }
                    if(joints[2] == null){
                        joints[2] = _target.transform.Find("Joint_0/Joint_1/Joint_2");
                    }
                    if(joints[3] == null){
                        joints[3] = _target.transform.Find("Joint_0/Joint_1/Joint_2/Joint_3");
                    }
                    if(joints[4] == null){
                        joints[4] = _target.transform.Find("Joint_0/Joint_1/Joint_2/Joint_3/Joint_4");
                    }



                    if(joints[5] == null){
                        joints[5] = _target.transform.Find("Joint_0/Joint_5");
                    }
                    if(joints[6] == null){
                        joints[6] = _target.transform.Find("Joint_0/Joint_5/Joint_6");
                    }
                    if(joints[7] == null){
                        joints[7] = _target.transform.Find("Joint_0/Joint_5/Joint_6/Joint_7");
                    }
                    if(joints[8] == null){
                        joints[8] = _target.transform.Find("Joint_0/Joint_5/Joint_6/Joint_7/Joint_8");
                    }


                    if(joints[9] == null){
                        joints[9] = _target.transform.Find("Joint_0/Joint_9");
                    }
                    if(joints[10] == null){
                        joints[10] = _target.transform.Find("Joint_0/Joint_9/Joint_10");
                    }
                    if(joints[11] == null){
                        joints[11] = _target.transform.Find("Joint_0/Joint_9/Joint_10/Joint_11");
                    }
                    if(joints[12] == null){
                        joints[12] = _target.transform.Find("Joint_0/Joint_9/Joint_10/Joint_11/Joint_12");
                    }


                    if(joints[13] == null){
                        joints[13] = _target.transform.Find("Joint_0/Joint_13");
                    }
                    if(joints[14] == null){
                        joints[14] = _target.transform.Find("Joint_0/Joint_13/Joint_14");
                    }
                    if(joints[15] == null){
                        joints[15] = _target.transform.Find("Joint_0/Joint_13/Joint_14/Joint_15");
                    }
                    if(joints[16] == null){
                        joints[16] = _target.transform.Find("Joint_0/Joint_13/Joint_14/Joint_15/Joint_16");
                    }


                    if(joints[17] == null){
                        joints[17] = _target.transform.Find("Joint_0/Joint_17");
                    }
                    if(joints[18] == null){
                        joints[18] = _target.transform.Find("Joint_0/Joint_17/Joint_18");
                    }
                    if(joints[19] == null){
                        joints[19] = _target.transform.Find("Joint_0/Joint_17/Joint_18/Joint_19");
                    }
                    if(joints[20] == null){
                        joints[20] = _target.transform.Find("Joint_0/Joint_17/Joint_18/Joint_19/Joint_20");
                    }

                    EditorUtility.SetDirty(_target);
   
                }
            }
        }
    }

}
