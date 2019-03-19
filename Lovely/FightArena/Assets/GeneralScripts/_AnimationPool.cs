using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
[ExecuteInEditMode]
#endif
public class _AnimationPool : _MasterComponent<_AnimationPool>
{
    //the serialized list of animations is neccessary so they can be accessed without any editor scripts
    [ShowOnly]
    [SerializeField]
    List<AnimationClip> animations = new List<AnimationClip>();

    [SerializeField]
    bool setTrueToManuallyUpdate = false;

    static Dictionary<string, AnimationClip> animationsDict;
    
    protected override void Awake()
    {
        base.Awake();
#if UNITY_EDITOR
        animations = GatherAllAnimations();
        SetUpDictionary();
#endif

    }

    private void Update()
    {
        if(setTrueToManuallyUpdate)
        {
            animations = GatherAllAnimations();
            setTrueToManuallyUpdate = false;
        }
    }

    static void SetUpDictionary()
    {
        animationsDict = new Dictionary<string, AnimationClip>();
        
        var animations = Instance.animations;
        animations.TrimExcess();
        for (int i = 0; i < animations.Count; i++)
        {
            var current = animations[i];
            //default animation names follow the form: ArmatureName|AnimationName
            //split off the name of the armature to leave just the animation name
            var splitName = current.name.Split('|');
            var animName = splitName[0];
            if (splitName.Length > 1)
                animName = splitName[1];
            if (current != null)
            {
                if (animationsDict.ContainsKey(animName))
                    Debug.LogWarning("AnimationPool contains multiple animations named '" + animName + "' only the first copy will be accessible!");
                else
                    animationsDict.Add(animName, current);
            }
        }
    }

    public static AnimationClip GetAnimation(string animationName)
    {
        if (animationsDict == null) SetUpDictionary();
        if (animationsDict.ContainsKey(animationName)) return animationsDict[animationName];
        else
            throw new KeyNotFoundException("there is no animation by the name of '" + animationName + "' in the pool");
    }

    public bool ContainsKey(string key)
    {
        return animationsDict.ContainsKey(key);
    }

#if UNITY_EDITOR
    List<AnimationClip> GatherAllAnimations()
    {
        var result = new List<AnimationClip>();

        string[] allFiles = Directory.GetFiles(Application.dataPath, "*.fbx", SearchOption.AllDirectories);
        foreach (var dirtyPath in allFiles)
        {
            string path = "Assets" + dirtyPath.Replace(Application.dataPath, "").Replace('\\', '/');
            var data = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var item in data)
            {
                AnimationClip clip = item as AnimationClip;
                if( clip != null && !clip.name.Contains("__preview__")) result.Add(clip);
            }
        }
        return result;
    }
#endif

}
