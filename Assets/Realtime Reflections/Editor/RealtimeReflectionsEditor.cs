using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RealtimeReflections))]
public class RealtimeReflectionsEditor : Editor {
	RealtimeReflections castedTarget;

	int[] resolutions = {16, 32, 64, 128, 256, 512, 1024, 2048};
	string[] resolutionsStrings;

	SerializedObject myTarget;

	SerializedProperty layerMask;
	SerializedProperty materials;
    SerializedProperty probes;

	void OnEnable(){

		castedTarget = (RealtimeReflections)target;

		resolutionsStrings = new string[resolutions.Length];

		for (int i = 0; i < resolutions.Length; i++)
			resolutionsStrings [i] = resolutions [i].ToString ();

		myTarget = new SerializedObject (target);

		layerMask = myTarget.FindProperty ("layerMask");
		materials = myTarget.FindProperty ("materials");
        probes = myTarget.FindProperty("reflectionProbes");
	}

	public override void OnInspectorGUI(){
		myTarget.Update ();

		EditorGUIUtility.labelWidth = 152;

		EditorGUILayout.PrefixLabel ("Cubemap Resolution:");
		castedTarget.cubemapSize = EditorGUILayout.IntPopup (castedTarget.cubemapSize, resolutionsStrings, resolutions);

		EditorGUILayout.PrefixLabel ("Camera Near Clip:");
		castedTarget.nearClip = EditorGUILayout.Slider (castedTarget.nearClip, 0.01f, 1);

		EditorGUILayout.PrefixLabel ("Camera Far Clip:");
		castedTarget.farClip = EditorGUILayout.Slider (castedTarget.farClip, castedTarget.nearClip + 0.01f, Camera.main.farClipPlane);

		EditorGUILayout.PrefixLabel ("Reflection Layer Mask:");
		EditorGUILayout.PropertyField (layerMask, GUIContent.none);

		EditorGUILayout.PropertyField (materials, true);
        EditorGUILayout.PropertyField(probes, true);

		if (GUI.changed)
			EditorUtility.SetDirty (castedTarget);

		AssignMaterials ();

		myTarget.ApplyModifiedProperties ();
	}

	
	void AssignMaterials(){
		castedTarget.gameObject.GetComponent<Renderer>().materials = castedTarget.materials;
	}
}
