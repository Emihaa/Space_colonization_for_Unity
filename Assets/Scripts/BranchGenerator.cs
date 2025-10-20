using UnityEngine;
using System.Collections.Generic;

// better generate the logic before play time and during the pay time it will grow to be

public class BranchGenerator : MonoBehaviour
{
    public GameObject   target;
    public int          grow                    = 10;
    public int          attractorAmount         = 500;
    public float        killRadius              = 0.01f;
    public float        attractionRadius        = 0.1f;
    public float        offsetDistance          = 0.05f;
    public float        branchLen               = 0.15f;
    public bool         showAttractionRadius    = false;
    public bool         showKillRadius          = false;
    public bool         showLines               = false;

    private List<Vector3>   attractorPoints     = new List<Vector3>();
    private List<Node>      nodesList           = new List<Node>();

    // things to do:
    // generate that the attraction points will be more evenly added based on the area space
    // give option that plants will grow based on the sun direction

    private void GenerateAttractors()
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
                Vector3 localPos = BranchUtils.RandomPosOnTriangle(vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]]);
                localPos += normal * offsetDistance;
                Vector3 worldPos = t.TransformPoint(localPos);

                attractorPoints.Add(worldPos);
                if (attractorPoints.Count == attractorAmount)
                    break;
            }
        }
    }

    private void SearchAttractorPoints()
    {
        foreach (var point in attractorPoints) 
        {
            Node tempNode = null;
            float tempDist = 3.40282347E+38f;
            foreach (var node in nodesList) 
            {
                float dist = (node._pos - point).magnitude;
                if (dist < attractionRadius && dist < tempDist)
                {
                    tempDist = dist;
                    tempNode = node;
                }
            }
            if (tempNode != null)
                tempNode._attractors.Add(point);
        }
    }

    private bool GenerateNewNodes(int index, Node rootNode)
    {
        bool grow = false;
        Node prevNode = rootNode;
        for (int i = 0; i < nodesList.Count; i++) 
        {
            Node node = nodesList[i];
            if (node._attractors.Count != 0)
            {
                Vector3 pos = new Vector3(0, 0, 0);
                foreach(var point in node._attractors) 
                {
                    pos += (point - node._pos).normalized;
                }
                pos /= node._attractors.Count;
                pos.Normalize();
                pos = node._pos + pos * branchLen;
                foreach(var point in node._attractors) 
                {
                    if ((point - pos).magnitude <= killRadius)
                        attractorPoints.Remove(point);
                }
                Node newNode = BranchUtils.NewNode(pos, node, prevNode, index);
                nodesList.Add(newNode);
                node._attractors.Clear();
                grow = true;
                prevNode = newNode;
            }
        }
        return (grow);
    } 

    private void CreateNodes ()
    {
        nodesList.Clear();
        Node rootNode = BranchUtils.NewNode(this.transform.position , null, null, 0);
        nodesList.Add(rootNode);
        for (int i = 0; i < grow; i++)
        {
            SearchAttractorPoints();
            if (GenerateNewNodes(i, rootNode) == false)
            {
                Debug.Log("no new nodes");
                break ;
            }
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (attractorPoints != null)
        {
            foreach (var point in attractorPoints)
                Gizmos.DrawSphere(point, 0.05f);
            if (nodesList != null)
            {
                foreach (var n in nodesList)
                {
                    if (showLines == false)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(n._pos, branchLen/2);
                    }
                    else
                    {
                        if (n._next != null)
                            Debug.DrawLine(n._pos, n._next._pos, Color.green);
                    }
                    Gizmos.color = Color.yellow;
                    if (showAttractionRadius == true)
                        Gizmos.DrawWireSphere(n._pos, attractionRadius);
                    Gizmos.color = Color.red;
                    if (showKillRadius == true)
                        Gizmos.DrawWireSphere(n._pos, killRadius);
                }
            }
        }
    }

    public void SpaceColonization()
    {
        if (target != null)
        {
            GenerateAttractors();
            CreateNodes();
        }
        else
        {
            Debug.Log("Add Target");
            attractorPoints.Clear();
            nodesList.Clear();
        }
    }

    // Automatically updates the AttractorPoints when there are changes done to the attributes.
    public void OnValidate() 
    {
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
