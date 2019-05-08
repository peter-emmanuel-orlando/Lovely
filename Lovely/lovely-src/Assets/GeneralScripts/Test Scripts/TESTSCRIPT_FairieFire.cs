using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Light))]
public class TESTSCRIPT_FairieFire : MonoBehaviour
{
    private NavMeshAgent navAgent;
    private Light light;
    float setNext = 0;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        light = GetComponent<Light>();
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame    
	void Update ()
    {
	    if(Time.time > setNext && (!navAgent.hasPath && !navAgent.pathPending))
        {
            setNext = Time.time + Random.Range(3f, 8f);
            var randomDelta = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
            //NavMeshHit hit;
            //NavMesh.Raycast(transform.position, transform.position + randomDelta, out hit, NavMesh.AllAreas);
            navAgent.destination = transform.position + randomDelta;// hit.position;
            Debug.DrawLine(transform.position, navAgent.destination, Color.green, 1);
            StopAllCoroutines();
            StartCoroutine(TransitionToColor(light, Random.ColorHSV(), 2));
        }
	}

    IEnumerator TransitionToColor(Light lightSource, Color finalColor, float duration)
    {
        if(duration <= 0 )
        {
            lightSource.color = finalColor;
        }
        else
        {
            var elapsedTime = 0f;
            var originalColor = light.color;
            while(elapsedTime <= duration)
            {
                var lerpFactor = elapsedTime / duration;                
                var newColor = Color.Lerp(originalColor, finalColor, lerpFactor);
                lightSource.color = newColor;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        yield break;
    }
}
