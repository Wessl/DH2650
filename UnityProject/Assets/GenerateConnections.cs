using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateConnections : MonoBehaviour
{
    public GameObject[] nodes;
    public GameObject[] platforms;
    public bool generateNodes;
    public bool generateConnections;
    public LayerMask UIMask;

    private void Awake()
    {
        if (generateNodes)
        {
            platforms = GameObject.FindGameObjectsWithTag("Platform");
            GenerateNodes();
        }
        if (generateConnections)
        {
            nodes = GameObject.FindGameObjectsWithTag("Node");
            GenerateGeneralConnections();
        }
    }
    // Start is called before the first frame update
    void Start()
    {   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateDropConnections()
    {
        foreach (var n in nodes)
        {
            if(n.layer.Equals("Platform"))
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down);

            }
        }

    }

    void GenerateNodes()
    {
        foreach(var obj in platforms)
        {
            float width = obj.transform.localScale.x;
            Node lastNode = null; ;
            for (int i = 0; i < width / 2; i++) {
                GameObject node = new GameObject("Node" + i);
                node.transform.parent = obj.transform;
                node.AddComponent<Node>();
                node.AddComponent<CircleCollider2D>();
                node.GetComponent<CircleCollider2D>().radius = 0.1f;
                node.layer = 5;
                node.tag = "Node";
                node.transform.localPosition = new Vector2(-0.5f+(1.0f + i * 2 )/ width, 1);
                Node n = node.GetComponent<Node>();
                if (i>0)
                {
                    n.connectedTo.Add(lastNode);
                    lastNode.connectedTo.Add(n);
                }
                lastNode = n;

            }
        }
    }

    void GenerateGeneralConnections()
    {
        List<GameObject> leftEdgeNodes = new List<GameObject>();
        List<GameObject> rightEdgeNodes = new List<GameObject>();
        foreach (var n in nodes)
        {
            Node node = n.GetComponent<Node>();
            if (node.connectedTo.Count == 1)
            {
                if (node.connectedTo[0].transform.position.x > node.transform.position.x)
                {
                    leftEdgeNodes.Add(n);
                }
                else
                    rightEdgeNodes.Add(n);
            }
        }
        foreach (var n in leftEdgeNodes)
        {
            RaycastHit2D sideHit = Physics2D.Raycast(n.transform.position, Vector2.left, 2, UIMask);
            if (sideHit.collider != null && sideHit.collider.CompareTag("Node"))
            {
                Node n1 = n.GetComponent<Node>();
                Node n2 = sideHit.collider.GetComponent<Node>();
                n1.connectedTo.Add(n2);
                continue;
            }
            RaycastHit2D jumpHit = Physics2D.Raycast(n.transform.position, new Vector2(-1,-1), Mathf.Sqrt(50), UIMask);
            if(jumpHit.collider != null && jumpHit.collider.CompareTag("Node"))
            {
                Node n1 = n.GetComponent<Node>();
                Node n2 = jumpHit.collider.GetComponent<Node>();
                n1.connectedTo.Add(n2);
                n2.connectedTo.Add(n1);
            }    
        }
        foreach (var n in rightEdgeNodes)
        {
            RaycastHit2D sideHit = Physics2D.Raycast(n.transform.position, Vector2.right, 2, UIMask);
            if (sideHit.collider != null && sideHit.collider.CompareTag("Node"))
            {
                Node n1 = n.GetComponent<Node>();
                Node n2 = sideHit.collider.GetComponent<Node>();
                n1.connectedTo.Add(n2);
                continue;
            }
            RaycastHit2D jumpHit = Physics2D.Raycast(n.transform.position, new Vector2(1, -1), Mathf.Sqrt(50), UIMask);

            if (jumpHit.collider != null && jumpHit.collider.CompareTag("Node"))
            {
                Node n1 = n.GetComponent<Node>();
                Node n2 = jumpHit.collider.GetComponent<Node>();
                n1.connectedTo.Add(n2);
                n2.connectedTo.Add(n1);
            }
        }

    }
}
