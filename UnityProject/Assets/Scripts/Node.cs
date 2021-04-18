using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> connectedTo = new List<Node>();

    public void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "blendSampler");
        foreach(var n in connectedTo)
        {
            Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
}
