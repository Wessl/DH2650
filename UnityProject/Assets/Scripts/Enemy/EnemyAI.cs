using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public List<Node> allNodes = new List<Node>();

    public Node closestNode;
    public Node targetNode;

    public Transform target;

    public List<Node> Path;

    public Enemy enemyMovement;

    public float minDist, maxDist;

    private void Awake()
    {
        //FindAllNodes();
    }
    // Start is called before the first frame update
    void Start()
    {
        FindAllNodes();
        if (CompareTag("NewGroundEnemy"))
            InvokeRepeating("UpdatePath", 0f, .5f);
    }

    // Update is called once per frame
    void Update()
    {
        if(CompareTag("NewGroundEnemy"))
            MoveTowardsPath();
    }

    public void FindAllNodes()
    {
        allNodes = FindObjectsOfType<Node>().ToList();
    }

    Node GetClosestNodeTo(Transform t)
    {
        Node cNode = null;

        float minDistance = Mathf.Infinity;
        foreach (Node node in allNodes)
        {
            float dist = (node.transform.position - t.position).sqrMagnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
                cNode = node;
            }
        }

        return cNode;
    } 

    void UpdatePath()
    {
        if ((GetClosestNodeTo(target).Equals(targetNode) && GetClosestNodeTo(transform).Equals(closestNode) && Path.Count>0)  || !enemyMovement.grounded)
        {
            return;
        }
        Path.Clear();

        targetNode = GetClosestNodeTo(target);
        closestNode = GetClosestNodeTo(transform);
        if(targetNode == null || closestNode == null)
        {
            Debug.Log("Node is missing");
            return;
        }
        HashSet<Node> visitedNodes = new HashSet<Node>();
        Queue<Node> unvisitedNodes = new Queue<Node>();
        Dictionary<Node, Node> nodeAndParent = new Dictionary<Node, Node>();

        unvisitedNodes.Enqueue(closestNode);

        while(unvisitedNodes.Count > 0)
        {
            Node n = unvisitedNodes.Dequeue();
            if(n.Equals(targetNode))
            {
                MakePath(nodeAndParent);
                return;
            }

            foreach (Node node in n.connectedTo)
            {
                if(!visitedNodes.Contains(node))
                {
                    visitedNodes.Add(node);
                    nodeAndParent.Add(node, n);
                    unvisitedNodes.Enqueue(node);
                }
            }
        }
    }

    void MakePath(Dictionary<Node, Node> nodeAndP)
    {
        if (nodeAndP.Count > 0)
        {
            if(nodeAndP.ContainsKey(targetNode) && nodeAndP.ContainsValue(closestNode))
            {
                Node currNode = targetNode;
                while(currNode != closestNode)
                {
                    Path.Add(currNode);
                    currNode = nodeAndP[currNode];
                }
                Path.Add(closestNode);
                Path.Reverse();
            }
        }
    }

    void MoveTowardsPath() {
        enemyMovement.xMovement = 0;
        enemyMovement.jump = false;
        Node currentNode = null;
        if (Path.Count > 0)
        {
            currentNode = Path.First();
            
        }  else
        {
            currentNode = targetNode;
        }
        
        var xMag = Mathf.Abs(currentNode.transform.position.x - transform.position.x);
        var yDiff = currentNode.transform.position.y - transform.position.y;
        if (currentNode && xMag >= minDist && yDiff <= maxDist)
        {
            if (transform.position.x > currentNode.transform.position.x)
            {
                enemyMovement.xMovement = -1;
            }
            else if (transform.position.x < currentNode.transform.position.x)
            {
                enemyMovement.xMovement = 1;
            }
            if (transform.position.y < currentNode.transform.position.y && (yDiff > minDist))
            {
                enemyMovement.jump = true;
            }
        }
        else if(enemyMovement.grounded)
        {
            if (Path.Count > 1)
            {
                Path.Remove(Path.First());
            }

            if (currentNode == targetNode && Vector2.Distance(currentNode.transform.position, transform.position) < minDist)
            {
                Path.Clear();
            }
        }
        
    }
}
