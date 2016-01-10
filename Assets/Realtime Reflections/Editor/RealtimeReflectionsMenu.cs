using UnityEngine;
using UnityEditor;
using System.Collections;

public class RealtimeReflectionsMenu : ScriptableObject
{
    [MenuItem("GameObject/Realtime Reflections/Add to Selected Object")]
    static void AddToSelectedObject()
    {

        GameObject reflectionPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Realtime Reflections/Prefabs/Reflection Manager.prefab", typeof(GameObject));
        reflectionPrefab = (GameObject)Instantiate(reflectionPrefab);

        Undo.RecordObject(reflectionPrefab, "Reflection Manager Creation");

        reflectionPrefab.name = "Reflection Manager";
        reflectionPrefab.transform.parent = Selection.activeGameObject.transform;

        reflectionPrefab.transform.localPosition = Vector3.zero;
        reflectionPrefab.transform.rotation = Quaternion.identity;

        if (!Selection.activeGameObject.GetComponent<Camera>() && Selection.activeGameObject.GetComponent<Renderer>()) {
            reflectionPrefab.GetComponent<RealtimeReflections>().materials = Selection.activeGameObject.GetComponent<Renderer>().materials;
        }
    }

    [MenuItem("GameObject/Realtime Reflections/Add to Main Camera")]
    static void AddToMainCamera()
    {

        GameObject reflectionPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Realtime Reflections/Prefabs/Reflection Manager.prefab", typeof(GameObject));
        reflectionPrefab = (GameObject)Instantiate(reflectionPrefab);

        Undo.RecordObject(reflectionPrefab, "Reflection Manager Creation");

        reflectionPrefab.name = "Reflection Manager";
        reflectionPrefab.transform.parent = Camera.main.transform;

        reflectionPrefab.transform.localPosition = Vector3.zero;
        reflectionPrefab.transform.rotation = Quaternion.identity;
    }

    [AddComponentMenu("Realtime Reflections/Planar Reflections/Add to Selected Object")]
    static void AddPlanarToSelectedObject()
    {

        GameObject go = Selection.activeGameObject;

        Undo.AddComponent<PlanarRealtimeReflection>(go);

        foreach (Material m in go.GetComponent<Renderer>().sharedMaterials) {
            if (m.name != "Default-Diffuse")
                m.shader = (Shader)AssetDatabase.LoadAssetAtPath("Assets/Realtime Reflections/Shaders/PlanarReflection.shader", typeof(Shader));
            else
                Debug.LogError("Realtime Reflections: Cannot apply shader to Default-Diffuse material to prevent problems while creating new primitives");
        }
    }
}