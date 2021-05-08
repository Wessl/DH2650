using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public List<Node> allNodes = new List<Node>();

    public Node closestNode;
    public Node targetNode;

    private Transform target;

    public List<Node> Path;

    public Enemy enemyMovement;

    public float minDist, maxDist, jumpDist, engagementRange, stopDistance;
    public float jumpStrength = 20;

    public LayerMask UIMask;

    public bool engaged;
    bool reached;

    public LayerMask playerAndGround;

    private void Awake()
    {
        target = GameObject.FindWithTag("Player").GetComponent<Transform>();
        //FindAllNodes();
    }
    // Start is called before the first frame update
    void Start()
    {
        //FindAllNodes();
        if (CompareTag("NewGroundEnemy"))
            InvokeRepeating("UpdatePathNew", 0f, .5f);
    }

    // Update is called once per frame
    void Update()
    {
        float dist = (target.position - transform.position).sqrMagnitude;

        if (engaged)
        {
            // Stop at desired range from player
            if (dist < stopDistance)
            {
                enemyMovement.xMovement = 0;
                enemyMovement.inRange = true;
                reached = true;
            }
            else
            {
                enemyMovement.inRange = false;
                reached = false;
            }
            if (CompareTag("NewGroundEnemy") && !reached)
                MoveTowardsPath();
        }
        if (!engaged && dist <= engagementRange)
        {
            ScanForPlayer(dist);
        }
        else if (dist > engagementRange)
        {
            enemyMovement.xMovement = 0;
            engaged = false;
        }
    }

    void ScanForPlayer(float dist)
    {
        Vector2 dir = (target.transform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir,Mathf.Sqrt(dist),playerAndGround);
        if (!hit)
            return;
        if (hit.collider.CompareTag("Player"))
        {
            engaged = true;
        }
    }

    public void FindAllNodes()
    {
        allNodes = FindObjectsOfType<Node>().ToList();
    }

    Node GetClosestNodeTo(Transform t)
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
        }
        
        return cNode;
    }

    void UpdatePathNew()
    {
        // Don't update path while not engaged or in air
        if (!engaged || !enemyMovement.grounded)
            return;
        /*
        if ((GetClosestNodeTo(target).Equals(targetNode) && GetClosestNodeTo(transform, false).Equals(closestNode) && Path.Count > 0) || */
        if (GetClosestNodeTo(transform).Equals(closestNode) && !enemyMovement.grounded)        // Don't update the path if the seeker isn't grounded or not engaged
        {
            return;
        }
        Path.Clear();

        targetNode = GetClosestNodeTo(target);            // Set the targetNode to the node closest to the target
        closestNode = GetClosestNodeTo(transform);       // Set the closestNode to the node closest to the seeker
        List<Node> unvisited = new List<Node>();
        HashSet<Node> visited = new HashSet<Node>();
        unvisited.Add(closestNode);     // Start the path-search with just the closest node as root and go from there

        while(unvisited.Count > 0)
        {
            Node currentNode = unvisited[0];
            for(int i=1;i<unvisited.Count;i++)      // For first iteration this will be skipped since only root is in
            {
                // If another univisted node has a lower fCost or equal fCost and lower hCost, then that is our new currentNode (see Node.cs for definition)
                if (unvisited[i].fCost < currentNode.fCost ||
                    (unvisited[i].fCost == currentNode.fCost && unvisited[i].fCost == currentNode.fCost && unvisited[i].hCost < currentNode.hCost))
                {
                    currentNode = unvisited[i];
                }
            }
            // Remove current node from unvisited and add it to visited
            unvisited.Remove(currentNode);
            visited.Add(currentNode);

            // If current node is the target node then we have our path and can return
            if (currentNode.Equals(targetNode))
            {
                MakePath(closestNode, targetNode);
                return;
            }

            foreach(Node n in currentNode.connectedTo) // Check all the current node's neighbors
            {
                if (visited.Contains(n))    // If a neighbor is already visited, skip it
                    continue;

                //float gCost = (n.transform.position - closestNode.transform.position).sqrMagnitude
                float gCost = currentNode.gCost + Vector2.Distance(currentNode.transform.position, n.transform.position);

                // Update the gCost and hCost of the node if it isn't in unvisited or if it has new lower gCost 
                if(gCost < n.gCost || !unvisited.Contains(n))
                {
                    n.gCost = gCost;
                    n.hCost = Vector2.Distance(n.transform.position, targetNode.transform.position);
                    n.parent = currentNode;

                    // If not already in unvisited, add it
                    if (!unvisited.Contains(n))
                        unvisited.Add(n);
                }
            }
        }
    }

    // Create the path by checking the parent of each node until you reach the other end
    void MakePath(Node start, Node end)
    {
        Path.Clear();
        Node currentNode = end;
        while(!currentNode.Equals(start))
        {
            Path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        // Path is in reverse so reverse it
        Path.Reverse();
    }

    // Traverse the path
    void MoveTowardsPath() {
        enemyMovement.xMovement = 0;
        Node currentNode = null;
        if (Path.Count > 0)
        {
            currentNode = Path.First();
            
        }  else
        {
            return;
        }
        // Distance between seeker and current node, used to switch the current node before fully reaching it
        float xMag = Mathf.Abs(currentNode.transform.position.x - transform.position.x);
        // Distance between current node and the previous node, used to delay hoirzontal jumps until the right distance
        float nodeXMag = Mathf.Abs(currentNode.transform.position.x - currentNode.parent.transform.position.x);
        // Vertical distance between seeker and current node
        float yDiff = currentNode.transform.position.y - transform.position.y;
        
        if (currentNode && xMag >= minDist && yDiff <= maxDist)     // Move towards node if the horizontal distance to it is big enough
        {
            if (transform.position.x > currentNode.transform.position.x)
            {
                enemyMovement.xMovement = -1;
            }
            else if (transform.position.x < currentNode.transform.position.x)
            {
                enemyMovement.xMovement = 1;
            }

            if (enemyMovement.canJump)
            {
                // Do a jump depending on the vertical distance
                if (yDiff > minDist && xMag >= 1.515f)
                {
                    float jump = jumpStrength/2 + yDiff * 2.5f;
                    if (jump > jumpStrength)
                        jump = jumpStrength;
                    enemyMovement.jumpSpeed = jump;
                }
                // Do a "horizontal" jump if the seeker is close enough to the edge and the the node isn't below the seeker
                else if (nodeXMag > jumpDist && yDiff >= -0.1f && xMag <= nodeXMag)
                {
                    float jump = nodeXMag * 3 + yDiff * 5;
                    if (jump > jumpStrength)
                        jump = jumpStrength;
                    enemyMovement.jumpSpeed = jump;
                }
            }
        }
        else if(enemyMovement.grounded)     // If horizontal distance is small enough and seeker is grounded, get a new node
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
