using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class AddComponentsToModels : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    bool finalize = false;
    [SerializeField]
    bool seeResults = false;
    [SerializeField]
    bool replaceExisting = true;

    [ShowOnly]
    [SerializeField]
    List<Transform> transforms = new List<Transform>();
    [SerializeField]
    bool selectAllAddedComponents = false;
    [ShowOnly]
    [SerializeField]
    List<Component> addedComponents = new List<Component>();
    [ShowOnly]
    readonly string[] searchedAssemblies = new string[] {"[placeholder], Assembly-CSharp", "UnityEngine.[placeholder], UnityEngine.CoreModule", "UnityEngine.AI.[placeholder], Assembly-CSharp" };

	void Update ()
    {
        if (finalize)
            DestroyImmediate(this);

        if(seeResults)
        {
            seeResults = false;
            DestroyPreviouslyAddedComponents();
            transforms = new List<Transform>(transform.GetAllChildren());
            foreach (var child in transforms)
            {
                PotentiallyAddComponentToChild(child.gameObject);
            }
            //var type = Type.gettype(gameobjectName");
            //if(type extends component)
            //  child.gameobject.AddComponent(gameobjectName);
            //Debug.Log(typeof(UnityEngine.AI.NavMeshSurface).AssemblyQualifiedName);
            Debug.Log("Successfully added components to model!");
        }
        if(selectAllAddedComponents)
        {
            selectAllAddedComponents = false;
            var goList = new List<GameObject>();
            foreach (var item in addedComponents)
            {
                if(item != null)
                    goList.Add(item.gameObject);
            }
            Selection.objects = goList.ToArray();
        }
	}

    private void DestroyPreviouslyAddedComponents()
    {
        foreach (var component in addedComponents)
        {
            DestroyImmediate(component);
            Debug.Log("Successfully cleared prev components from model!");
        }
    }

    private void PotentiallyAddComponentToChild(GameObject recipient)
    {
        Type possibleComponentType = null;
        foreach (var assembly in searchedAssemblies)
        {
            var typeName = assembly.Replace("[placeholder]", recipient.name.Split('.')[0]);
            possibleComponentType = Type.GetType(typeName, false, true);
            if (possibleComponentType != null)
            {
                if(replaceExisting)
                {
                    foreach (var oldComponent in recipient.GetComponents(possibleComponentType))
                    {
                        DestroyImmediate(oldComponent);
                    }
                }
                var newAddition = recipient.AddComponent(possibleComponentType);
                addedComponents.Add(newAddition);
                break;
            }
            addedComponents.RemoveAll((Component c) => { return c == null; });
        }
    }

    private void OnDestroy()
    {
        if (!finalize) DestroyPreviouslyAddedComponents();
    }
#endif
}
