using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// better generate the logic before play time and during the pay time it will grow to be

public class BranchGenerator : MonoBehaviour
{
    public  GameObject          sun;
    private GameObject          holder;
    public Material             branchMat;
    private Mesh                mesh;
    public  int                 grow                = 10;
    public  int                 attractorAmount     = 500;
    public  float               killRadius          = 0.2f;
    public  float               attractionRadius    = 0.4f;
    public  float               offsetDistance      = 0.1f;
    public  float               branchLen           = 0.05f;
    private readonly float      maxThickness        = 0.05f;
    
    [System.NonSerialized] public bool  showAttractionRadius    = false;
    [System.NonSerialized] public bool  showKillRadius          = false;
    [System.NonSerialized] public bool  showLines               = true;
    [System.NonSerialized] public bool  instantiated            = false;
    
    public  bool    sunEffect       = false;
                  
    private List<GameObject>    targets             = new List<GameObject>();
    private List<Vector3>       attractorPoints     = new List<Vector3>();
    private List<Node>          nodesList           = new List<Node>();

    private int[]       triangles;
    private Vector3[]   vertices;

    private AttractionPointGenerator attGen;
    private NodeGenerator               nodesGen;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (attractorPoints != null && instantiated != true)
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

    private bool FindTargets()
    {
        Transform parent = this.transform;
        targets.Clear();
        foreach (Transform child in parent)
        {
            targets.Add(child.gameObject);
        }
        if (targets.Count > 0)
            return (true);
        return (false);
    }

    public void SpaceColonization()
    {
        if (FindTargets())
        {
            nodesList.Clear();
            attGen = new AttractionPointGenerator(targets, sun, attractorAmount, offsetDistance, sunEffect, attractorPoints);
            attGen.GenerateAttractors();
            nodesGen = new NodeGenerator(this.transform, grow, killRadius, attractionRadius, branchLen, attractorPoints);
            nodesList = nodesGen.CreateNodes();
        }
        else
        {
            Debug.Log("No target(s) found, add them as children of the PlantGenerator object");
            attractorPoints.Clear();
            nodesList.Clear();
        }
    }

    public void DeleteBranches()
    {
        foreach (var node in nodesList)
        {
            DestroyImmediate(holder);
            holder = null;
            instantiated = false;
        }
    }

    public void GenerateBranches()
    {
        mesh.Clear();
        if (!holder)
        {
            holder = new GameObject("Branches");
            holder.transform.position = this.transform.position;
            MeshFilter meshf = holder.AddComponent<MeshFilter>();
            MeshRenderer meshr = holder.AddComponent<MeshRenderer>();

            int points = nodesList[0]._vertices.Length;
            int ringCount = nodesList.Count;

            vertices = new Vector3[points * ringCount];
            triangles = new int[6 * points * ringCount];

            // copy vertices from the nodes to one array
            foreach (var node in nodesList)
            {
                for (int j = 0; j < points; j++)
                {
                    vertices[node._index * points + j] = node._vertices[j] - this.transform.position;
                }
            }

            // create triangles
            int t = 0;
            foreach (var node in nodesList)
            {
                for (int i = 0; i < points; i++)
                {
                    if (node._next != null)
                    {
                        int nexti = (i + 1) % points; // wrap around the circle, next vertec is always i+1 expect
                        // if we have gone around the circle we modulate it back to zero

                        int a0 = node._index * points + i;
                        int a1 = node._index * points + nexti;
                        int b0 = node._next._index * points + i;
                        int b1 = node._next._index * points + nexti;

                        // two triangles per quad
                        triangles[t++] = a0;
                        triangles[t++] = b0;
                        triangles[t++] = a1;

                        triangles[t++] = a1;
                        triangles[t++] = b0;
                        triangles[t++] = b1;   
                    }
                }
            }

            mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            meshf.mesh = mesh;
            if (branchMat != null)
            {
                meshr.material = branchMat;
            }
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            instantiated = true;
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
