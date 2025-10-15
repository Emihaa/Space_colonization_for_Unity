using UnityEngine;
using System.Collections.Generic;

// probably also need to add: 
// scale (one for vector scale and one for the thickness of the branch), 
// direction, 
// mesh(gameobj),
public class Node
{
    public Vector3 _pos;
    public Node _prev;
    public Node _next;
    public int _index;

    public Node(Vector3 pos)
    {
        _pos = pos;
        _prev = null;
        _next = null;
        _index = 0;
    }
}

    // better to have button that generates the plant while not on Play time
    // also should maybe create a plant growing simulation that grows on playtime?
    // first lets do one that you press simulation button when not active
    // then consider one that happens play time

public class BranchGenerator : MonoBehaviour
{
    public GameObject target;
    public int attractorAmount = 500;
    public float killRadius = 0.1f;
    public float attractionRadius = 0.01f;
    public float offsetDistance = 0.05f;

    private List<Vector3> attractorPoints = new List<Vector3>();
    private List<Node> nodesList = new List<Node>();

    /* Example: 
                v0 = (0, 0, 9)
                v1 = (-3, 4, 9)
                v2 = (2, 4, 9)

                u = 0.5f
                v = 0.7f
                new:
                u = 0.5f
                v = 0.3f
    
                pos = v0 + u*(v1 - v0) + v*(v2 - v0)
                    v1 - v0 = (-3, 4, 9) - (0, 0, 9) = (-3, 4, 0)
                    v2 - v0 = ( 2, 4, 9) - (0, 0, 9) = ( 2, 4, 0)
                    u * (v1 - v0) = 0.5 * (-3, 4, 0) = (-1.5, 2, 0)
                    v * (v2 - v0) = 0.3 * ( 2, 4, 0) = (0.6, 1.2, 0)
                    v0 + ... = (0, 0, 9) + (-1.5, 2, 0) + (0.6, 1.2, 0)
                    = (-0.9, 3.2, 9)
    */
    // Generate random pos inside given triangle and return that
    Vector3 RandomPosOnTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        float u = Random.value;
        float v = Random.value;
        
        if (u + v > 1)
        {
            u = 1f - u;
            v = 1f - v;
        }
        return (v0 + u * (v1 - v0) + v * (v2 - v0));
    }

    // generates attractorPoints from the mesh information to a Vector3 List
    // TODO: now it generates RandomPos for each face and then loops through the faces till it has reached the attractorAmount
    // Maybe there could be better way?
    [ContextMenu("Generate Attractors")]
    public void GenerateAttractors()
    {
        if (target != null)
        {
            Mesh mesh = target.GetComponent<MeshFilter>().sharedMesh;
            Transform t = target.transform;

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;

            attractorPoints.Clear();

            while (attractorPoints.Count < attractorAmount)
            {
                int amount = triangles.Length;
                for (int i = 0; i < amount; i += 3)
                {
                    Vector3 normal = (normals[triangles[i]] + normals[triangles[i + 1]] + normals[triangles[i + 2]]).normalized;
                    Vector3 localPos = RandomPosOnTriangle(vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]]);
                    localPos += normal * offsetDistance;
                    Vector3 worldPos = t.TransformPoint(localPos);

                    attractorPoints.Add(worldPos);
                    if (attractorPoints.Count == attractorAmount)
                        break;
                }
            }
        }
        createNodes();
    }

    private Node addNode (Vector3 pos, Node prevNode, Node nextNode, int index)
    {
        Node newNode = new Node(pos);
        newNode._next = nextNode;
        newNode._prev = prevNode;
        newNode._index = index;

        return (newNode);
    }

    /* 
        https://algorithmicbotany.org/papers/colonization.egwnp2007.large.pdf

        The tree is generated iteratively. In each iteration, an attraction point may influence the tree node that is closest to it. This influence occurs if the distance between the point and the closest node is less then a radius of influence di.
        There may be several attraction points that influence a single tree node v: we denote this set of points by S(v). If S(v) is not empty, a new tree node v will be created and attached to v by segment (vv). 
        The node v is positioned at a distance D from v, in the direction defined as the average of the normalized vectors toward all the sources s S(v).

    */
    private void createNodes ()
    {
        Transform t = this.transform;
        nodesList.Clear();

        Node rootNode = addNode(t.position, null, null, 0);
        nodesList.Add(rootNode);

        // first check if in the radius of each node there is an attractor/s
        // each attractor that is within the radius will be used to calculate new vector for the direction of new node
        // if there are attractors that are within a killzone of the new node then destroy them
        // if there are no attractors just go towards the previous nodes dir?
        Node currentNode = rootNode;
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < 1 ; i++)
        {
            points.Clear();
            foreach (var point in attractorPoints)
            {
                if ((currentNode._pos - point).sqrMagnitude < attractionRadius)
                {
                    Debug.Log(point);
                    points.Add(point);
                } 
            }
            if (points.Count != 0)
            {
                // the new position of new node should also be by the world pos
                Vector3 dist = new Vector3(0, 0, 0);
                foreach(var point in points) 
                {
                    dist += (point - currentNode._pos).normalized;
                }
                dist /= points.Count;
                dist.Normalize();
                Debug.Log("distance:" + dist);
                dist = currentNode._pos + dist * 0.15f;
                Node newNode = addNode(dist, currentNode, null, i);
                currentNode._next = newNode;
                nodesList.Add(newNode);
            }
            if (currentNode._next == null)
                break ;
            currentNode = currentNode._next;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (attractorPoints != null)
        {
            foreach (var point in attractorPoints)
            {
                Gizmos.DrawSphere(point, 0.05f);
            }
            Gizmos.color = Color.green;
            if (nodesList != null)
            {
                foreach (var n in nodesList)
                {
                    Gizmos.DrawSphere(n._pos, 0.1f);
                }
            }
        }
    }

    // Automatically updates the AttractorPoints when there are changes done to the attributes.
    // Not so lightweight
    private void OnValidate() 
    {
        GenerateAttractors();    
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
