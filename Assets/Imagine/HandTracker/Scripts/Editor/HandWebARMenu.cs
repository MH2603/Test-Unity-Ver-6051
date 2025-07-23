using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Imagine.WebAR.Editor
{
	public class WorldWebARMenu
	{
		[MenuItem("Assets/Imagine WebAR/Create/HandTracker", false, 1100)]
		public static void CreateHandTracker()
		{
			GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Imagine/HandTracker/Prefabs/HandTracker.prefab");
			GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			Selection.activeGameObject = gameObject;
			gameObject.name = "HandTracker";
		}

		[MenuItem("Assets/Imagine WebAR/Create/HandObject", false, 1101)]
		public static void CreateHandObject()
		{
			var handIndex = GameObject.FindObjectsOfType<HandObject>().Length;
			Transform parent = null;
			try{
				parent = GameObject.FindObjectOfType<HandTracker>().transform;
			}
			catch{
				Debug.LogWarning("HandTracker not found in your scene. Please make sure to create a HandTracker");
			}

			GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Imagine/HandTracker/Prefabs/HandObject.prefab");
			GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
			PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			Selection.activeGameObject = gameObject;
			gameObject.name = "HandObject " + handIndex;

			var handObject = gameObject.GetComponent<HandObject>();
			handObject.handIndex = handIndex;
			handObject.transform.localPosition = Vector3.right * handIndex;
		}

		
	}
}

