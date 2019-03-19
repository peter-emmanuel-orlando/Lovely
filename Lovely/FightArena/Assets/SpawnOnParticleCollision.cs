using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SpawnOnParticleCollision : MonoBehaviour
{
    [SerializeField]
    int maxSpawns = 20;


    [SerializeField]
    [ShowOnly]
    private List<GameObject> spawned = new List<GameObject>();

    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void Start()
    {
        part = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        Rigidbody rb = other.GetComponent<Rigidbody>();
        int i = 0;

        var newList = new List<GameObject>();
        for (int j = 0; j < spawned.Count; j++)
        {
            if (spawned[i] != null)
                newList.Add(spawned[i]);
        }
        spawned = newList;

        while (spawned.Count < maxSpawns && i < numCollisionEvents)
        {
            Vector3 pos = collisionEvents[i].intersection;
            var newSpawn = Instantiate<GameObject>(_PrefabPool.GetPrefab("HumanFemale").GameObject, pos, Quaternion.identity, null);
            //newSpawn.transform.position = pos;
            spawned.Add(newSpawn);
            i++;
        }
    }
}
