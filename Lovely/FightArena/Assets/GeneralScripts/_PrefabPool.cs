using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
[ExecuteInEditMode]
#endif
public class _PrefabPool : _MasterComponent<_PrefabPool>
{
    //the serialized list of animations is neccessary so they can be accessed without any editor scripts
    [ShowOnly]
    [SerializeField]
    List<GameObject> prefabsGameObjects = new List<GameObject>();

    [SerializeField]
    bool setTrueToManuallyUpdate = false;

    static Dictionary<string, ISpawnable> prefabsDict;


    protected override void Awake()
    {
        base.Awake();
#if UNITY_EDITOR
        prefabsGameObjects = GatherAllPrefabs();
        SetUpDictionary();
#endif

    }

    private void Update()
    {
        if(setTrueToManuallyUpdate)
        {
#if UNITY_EDITOR
            prefabsGameObjects = GatherAllPrefabs();
            SetUpDictionary();
#endif
            setTrueToManuallyUpdate = false;
        }
    }

    static void SetUpDictionary()
    {
        prefabsDict = new Dictionary<string, ISpawnable>();
        
        var prefabs = Instance.prefabsGameObjects;
        prefabs.TrimExcess();
        for (int i = 0; i < prefabs.Count; i++)
        {
            var current = prefabs[i].GetComponent<ISpawnable>();
            //default animation names follow the form: ArmatureName|AnimationName
            //split off the name of the armature to leave just the animation name
            var prefabName = current.PrefabName;
            if (current != null && !prefabsDict.ContainsKey(prefabName)) prefabsDict.Add(prefabName, current);
        }
    }

    public static ISpawnable GetPrefab(string prefabName)
    {
        if (prefabsDict == null) SetUpDictionary();
        if (prefabsDict.ContainsKey(prefabName)) return prefabsDict[prefabName];
        else
            throw new KeyNotFoundException("there is no prefab by the name of '" + prefabName + "' in the pool");
    }

    public bool ContainsKey(string key)
    {
        return prefabsDict.ContainsKey(key);
    }

#if UNITY_EDITOR
    List<GameObject> GatherAllPrefabs()
    {
        var result = new List<GameObject>();

        string[] allFiles = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
        foreach (var dirtyPath in allFiles)
        {
            string path = "Assets" + dirtyPath.Replace(Application.dataPath, "").Replace('\\', '/');
            Debug.unityLogger.logEnabled = false;
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Debug.unityLogger.logEnabled = true;
            var data = go.GetComponent<ISpawnable>();
            if (data != null && data.gameObject != null) result.Add(data.gameObject);
        }
        return result;
    }
#endif

}
