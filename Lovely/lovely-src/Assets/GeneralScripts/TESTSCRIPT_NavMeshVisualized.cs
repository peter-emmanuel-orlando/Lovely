using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

[ExecuteInEditMode]
public class TESTSCRIPT_NavMeshVisualized : MonoBehaviour {

    [SerializeField]
    float raycastRadius = 4;
    [SerializeField]
    float samplePositionRadius = 4;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        NavMeshHit nmh;
        var rays = 5f;
        for (int i = 0; i <= rays;  i++)
        {
            var x = (i / rays) * 360f;
            for (int j = 0; j <= rays; j++)
            {
                var y = (j / rays) * 360f;
                for (int k = 0; k <= rays; k++)
                {
                    var z = (k / rays) * 360f;
                    var ray = Quaternion.Euler(x, y, z) * Vector3.forward * raycastRadius;
                    NavMesh.Raycast(transform.position, transform.position + ray, out nmh, -1);
                    Debug.DrawLine(transform.position, nmh.position, Color.green);
                    //Debug.DrawLine(transform.position, transform.position + ray);
                }
            }
        }
        
        NavMesh.SamplePosition(transform.position, out nmh, samplePositionRadius, NavMesh.AllAreas);
        Debug.DrawLine(transform.position, nmh.position, Color.cyan);
        NavMesh.Raycast(transform.position, nmh.position, out nmh, -1);
        var color = (nmh.hit) ? Color.red : Color.yellow;
        Debug.DrawLine(transform.position, nmh.position, color);
    }
}
