using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Imagine.WebAR;
using UnityEngine;

namespace Imagine.WebAR
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    [ExecuteInEditMode]
    public class HandMesh : MonoBehaviour
    {
        private HandObject handObject;
        private int[] boneIndices = new int[]{0,5,6,7,8,9,10,11,12,13,14,15,16,1,2,3,4,17,18,19,20};

        void Awake(){
            handObject = GetComponentInParent<HandObject>();
            
        }
        // void Update(){
        //     var smr = GetComponent<SkinnedMeshRenderer>();
        //     // if(smr != null && smr.bones.Length <= 0){
        //         Debug.Log("set bones");
        //         var bones = GetComponentInParent<HandObject>().joints;
        //         // smr.bones = bones.ToArray();
        //         // smr.bones = new Transform[]{
        //         //     bones[0],
        //         //     bones[5],
        //         //     bones[6],
        //         //     bones[7],
        //         //     bones[8],
        //         //     bones[9],
        //         //     bones[10],
        //         //     bones[11],
        //         //     bones[12],
        //         //     bones[13],
        //         //     bones[14],
        //         //     bones[15],
        //         //     bones[16],
        //         //     bones[1],
        //         //     bones[2],
        //         //     bones[3],
        //         //     bones[4],
        //         //     bones[17],
        //         //     bones[18],
        //         //     bones[19],
        //         //     bones[20]
        //         // };
        //         Debug.Log("bones count = " + smr.bones.Length);
        //         foreach(var bone in smr.bones){
        //             Debug.Log(bone.name);
        //         }
        //     // }
        //     //right - 0,5,6,7,8,9,10,11,12,13,14,15,16,1,2,3,4,17,18,19,20 
            
        // }
    }

}
