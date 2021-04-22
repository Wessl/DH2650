using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> connectedTo = new List<Node>();

    public float gCost;         // The distance of the path taken from the closest node to this node
    public float hCost;         // The "bird path" distance of this node and the target node
    public Node parent;


    public void OnDrawGizmos()
    {
        foreach (var n in connectedTo)
        {
            Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }

    public float fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
}
