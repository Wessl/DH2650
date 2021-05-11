using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWaspRoute : MonoBehaviour
{
    [SerializeField]
    private Transform[] waypoints;
    private Vector2 gizmoPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        for(float t = 0; t<=1; t += 0.05f)
        {
            gizmoPos = Mathf.Pow(1 - t, 3) * waypoints[0].position +
                3 * t * Mathf.Pow(1 - t, 2) * waypoints[1].position +
                3 * (1 - t) * Mathf.Pow(t, 2) * waypoints[2].position +
                Mathf.Pow(t, 3) * waypoints[3].position;

            Gizmos.DrawSphere(gizmoPos, 0.25f);
        }

        Gizmos.DrawLine(waypoints[0].position, waypoints[1].position);
        Gizmos.DrawLine(waypoints[2].position, waypoints[3].position);
    }
}
