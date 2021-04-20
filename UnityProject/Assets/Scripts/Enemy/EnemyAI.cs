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

    public float minDist, maxDist, jumpDist, engagementRange;

    public LayerMask UIMask;

    public bool engaged;

    public LayerMask playerAndGround;

    private void Awake()
    {
        //FindAllNodes();
    }
    // Start is called before the first frame update
    void Start()
    {
        engaged = true;
        FindAllNodes();
        if (CompareTag("NewGroundEnemy"))
            InvokeRepeating("UpdatePathNew", 0f, .5f);
    }

    // Update is called once per frame
    void Update()
    {
        if(CompareTag("NewGroundEnemy") && engaged)
            MoveTowardsPath();
        //ScanForPlayer();
    }

    void ScanForPlayer()
    {
        float dist = (target.transform.position - transform.position).sqrMagnitude;
        if (dist < engagementRange)
        {
            Vector2 dir = (target.transform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir,Mathf.Sqrt(dist),playerAndGround);

            if(hit.collider.CompareTag("Player"))
                engaged = true;
        }
        else
            engaged = false;
    }

    public void FindAllNodes()
    {
        allNodes = FindObjectsOfType<Node>().ToList();
    }

    Node GetClosestNodeTo(Transform t, bool target)
    {
        Collider2D[] hitCollider = Physics2D.OverlapCircleAll(t.position, maxDist, UIMask);
        
        Node cNode = null;

        float minDistance = Mathf.Infinity;
        foreach (Collider2D hit in hitCollider)
        {
            if (!hit.CompareTag("Node"))
                continue;
            Node node = hit.GetComponent<Node>();
            float dist = (node.transform.position - t.position).sqrMagnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
                cNode = node;
            }
        }
        // If there is no nodes hit, assign default node as cNode. Otherwise Nullreference later. Occurs e.g. if player falls off platform and gets too far away
        if (hitCollider.Length == 0)
        {
            cNode = allNodes[0];

        return cNode;
    }

    void UpdatePathNew()
    {
        /*
        if ((GetClosestNodeTo(target, true).Equals(targetNode) && GetClosestNodeTo(transform, false).Equals(closestNode) && Path.Count > 0) || */
        if (!enemyMovement.grounded || !engaged)
        {
            return;
        }
        Path.Clear();

        targetNode = GetClosestNodeTo(target, true);
        closestNode = GetClosestNodeTo(transform, false);

        List<Node> unvisited = new List<Node>();
        HashSet<Node> visited = new HashSet<Node>();
        unvisited.Add(closestNode);

        while(unvisited.Count > 0)
        {
            Node currentNode = unvisited[0];
            for(int i=1;i<unvisited.Count;i++)
            {
                if (unvisited[i].fCost < currentNode.fCost ||
                    (unvisited[i].fCost == currentNode.fCost && unvisited[i].fCost == currentNode.fCost && unvisited[i].hCost < currentNode.hCost))
                {
                    currentNode = unvisited[i];
                }
            }
            unvisited.Remove(currentNode);
            visited.Add(currentNode);
            if (currentNode.Equals(targetNode))
            {
                MakePath(closestNode, targetNode);
                return;
            }

            foreach(Node n in currentNode.connectedTo)
            {
                if (visited.Contains(n)) 
                    continue;

                //float gCost = (n.transform.position - closestNode.transform.position).sqrMagnitude
                float gCost = currentNode.gCost + Vector2.Distance(currentNode.transform.position, n.transform.position);
                if(gCost < n.gCost || !unvisited.Contains(n))
                {
                    n.gCost = gCost;
                    n.hCost = Vector2.Distance(n.transform.position, targetNode.transform.position);
                    n.parent = currentNode;

                    if (!unvisited.Contains(n))
                        unvisited.Add(n);
                }
            }
        }
    }


        // This only uses BFS basically, so it's not optimized at all. Will fix later.
        void UpdatePath()
    {
        if ((GetClosestNodeTo(target, true).Equals(targetNode) && GetClosestNodeTo(transform, false).Equals(closestNode) && Path.Count>0)  || !enemyMovement.grounded)
        {
            return;
        }
        Path.Clear();

        targetNode = GetClosestNodeTo(target, true);
        closestNode = GetClosestNodeTo(transform, false);
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
                //MakePath(nodeAndParent);
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

    void MakePath(Node start, Node end)
    {
        Path.Clear();
        Node currentNode = end;
        while(!currentNode.Equals(start))
        {
            Path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Path.Reverse();
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
            return;
        }
        float xMag = Mathf.Abs(currentNode.transform.position.x - transform.position.x);
        float nodeXMag = Mathf.Abs(currentNode.transform.position.x - currentNode.parent.transform.position.x);
        float yDiff = currentNode.transform.position.y - transform.position.y;
        
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
            print(nodeXMag);
            print(xMag);
            if ((transform.position.y < currentNode.transform.position.y && (yDiff > minDist)))
            {
                float jump = yDiff * 5;
                if (jump > 20)
                    jump = 20;
                enemyMovement.jumpSpeed = jump;
            } else if (nodeXMag > jumpDist && yDiff >= -0.1f && xMag <= nodeXMag)
            {
                float jump = nodeXMag * 3 + yDiff * 5;
                if (jump > 20)
                    jump = 20;
                enemyMovement.jumpSpeed = jump;
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

    private void OnDrawGizmos()
    {
        if(Path!=null)
        {
            foreach(var n in Path)
            {
                Gizmos.DrawIcon(n.transform.position, "blendSampler", true, Color.red);
            }
        }
    }
}
