using UnityEngine;
using System.Collections.Generic;

// better generate the logic before play time and during the pay time it will grow to be

public class BranchGenerator : MonoBehaviour
{
    public GameObject   target;
    public int          grow                    = 10;
    public int          attractorAmount         = 500;
    public float        killRadius              = 0.2f;
    public float        attractionRadius        = 0.4f;
    public float        offsetDistance          = 0.1f;
    public float        branchLen               = 0.05f;
    
    [System.NonSerialized] public bool  showAttractionRadius    = false;
    [System.NonSerialized] public bool  showKillRadius          = false;
    [System.NonSerialized] public bool  showLines               = false;

    private List<Vector3>   attractorPoints     = new List<Vector3>();
    private List<Node>      nodesList           = new List<Node>();

    // things to do:
    // generate that the attraction points will be more evenly added based on the area space
    // give option that plants will grow based on the sun direction 

    private void WeightAreas(List<float> triangleAreas, ref float totalArea, Vector3[] vertices, int[] triangles)
    {
        int amount = triangles.Length;
        for (int i = 0; i < amount; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            float area = Vector3.Cross(v1 - v0, v2- v0).magnitude * 0.5f;
            triangleAreas.Add(area);
            totalArea += area;
        }
    }

    //this doest work. Check it out next
    private void PickArea(List<float> triangleAreas, ref float totalArea, Vector3[] normals, Vector3[] vertices, int[] triangles)
    {
        Transform t = target.transform;

        Debug.Log("totalarea:" + totalArea);
        while (attractorPoints.Count < attractorAmount)
        {
            float cumulative = 0f;
            int selectedTriangle = 0;
            float r = Random.value * totalArea;
            for (int i = 0; i < triangleAreas.Count; i++) 
            {
                cumulative += triangleAreas[i];
                if (r <= cumulative)
                {
                    selectedTriangle = i;
                    break ;
                }
            }
            int triIndex = selectedTriangle * 3;

            Vector3 v0 = vertices[triangles[triIndex]];
            Vector3 v1 = vertices[triangles[triIndex + 1]];
            Vector3 v2 = vertices[triangles[triIndex + 2]];

            Vector3 normal = (normals[triangles[triIndex]] + normals[triangles[triIndex + 1]] + normals[triangles[triIndex + 2]]).normalized;
            Vector3 localPos = BranchUtils.RandomPosOnTriangle(v0, v1, v2);
            localPos += normal * offsetDistance;
            Vector3 worldPos = t.TransformPoint(localPos);

            attractorPoints.Add(worldPos);
            if (attractorPoints.Count == attractorAmount)
                break;
        }
    }

    private void GenerateAttractors()
    {
        Mesh mesh = target.GetComponent<MeshFilter>().sharedMesh;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] normals = mesh.normals;

        attractorPoints.Clear();
        List<float> triangleAreas = new List<float>();
        float totalArea = 0f; // can we reach max float val? maybe we need to have larger val than float or check for overflow

        WeightAreas(triangleAreas, ref totalArea, vertices, triangles);
        PickArea(triangleAreas, ref totalArea, normals, vertices, triangles);

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
        SpaceColonization();
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
