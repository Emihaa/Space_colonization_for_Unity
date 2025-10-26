using UnityEngine;
using System.Collections.Generic;

public class MeshGenerator
{
    private Mesh mesh;
    private GameObject holder;
    private int         vertexAmount;
    private List<Node>  nodesList = new List<Node>();
    private int[]       triangles;
    private Vector3[]   vertices;
    private Material    branchMat;
    private float       maxThickness;

    public MeshGenerator(Mesh mesh, GameObject holder, int vertexAmount, List<Node> nodesList, Material branchMat, float maxThickness)
    {
        this.mesh           = mesh;
        this.holder         = holder;
        this.vertexAmount   = vertexAmount;
        this.nodesList      = nodesList;
        this.branchMat      = branchMat;
        this.maxThickness   = maxThickness;
    }

    // Create circle of vertices by given vertexAmount
    private Vector3[] CreateCircle(Vector3 position, Vector3 dir, float radius)
    {
        Vector3[] circle = new Vector3[vertexAmount];
        Quaternion quat = Quaternion.FromToRotation(Vector3.up, dir);
        for (int i = 0; i < vertexAmount; i++)
        {
            float angle = ((float)i / vertexAmount) * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            pos = quat * pos; // rotate circle to matchbranch direction
            pos += position; // move circle to node position
            circle[i] = pos; // add the vertec to the array
        }
        return (circle);
    }

    // per node create circle of vertices and them to the array
    private void CreateVertixes()
    {
        foreach (var node in nodesList)
        {
            Vector3[] circle = CreateCircle(node._pos, node._direction, node._thickness);
            for (int i = 0; i < vertexAmount; i++)
            {
                vertices[node._index * vertexAmount + i] = circle[i] - holder.transform.position;
            }
        }
    }

    // we find all the end nodes and travel backwards to the root node to grow the branch radius
    private void GrowBranches()
    {
        foreach (var node in nodesList)
        {
            if (node._next == null)
            {
                Node currentNode = node;
                while (currentNode != null)
                {
                    if (currentNode._thickness < maxThickness)
                        currentNode._thickness += 0.005f;
                    currentNode = currentNode._prev;
                }
            }
        }
    }

    // create triangles between the vertices
    /* 
        a0 ---- a1
        |     / |
        |   /   |
        | /     |
        b0 ---- b1

    */
    private void CreatePolygons()
    {
        int t = 0;
        foreach (var node in nodesList)
        {
            for (int i = 0; i < vertexAmount; i++)
            {
                if (node._next != null)
                {
                    int nexti = (i + 1) % vertexAmount; // wrap around the circle, next vertec is always i+1 expect
                    // if we have gone around the circle we modulate it back to zero

                    int a0 = node._index * vertexAmount + i; // to get the int val of the correct vertex/pos from the vertex array
                    int a1 = node._index * vertexAmount + nexti; // triangle array stores the location of the correct vertex
                    int b0 = node._next._index * vertexAmount + i;
                    int b1 = node._next._index * vertexAmount + nexti;

                    // two triangles per quad
                    // order matters. Clockwise or counterclockwise affect the polygon normal direction
                    triangles[t++] = a0;
                    triangles[t++] = a1;
                    triangles[t++] = b0;

                    triangles[t++] = a1;
                    triangles[t++] = b1;
                    triangles[t++] = b0;
                }
            }
        }
    }

    public Mesh GenerateMesh()
    {
        MeshFilter meshf = holder.AddComponent<MeshFilter>();
        MeshRenderer meshr = holder.AddComponent<MeshRenderer>();

        int ringCount = nodesList.Count;
        vertices = new Vector3[vertexAmount * ringCount];
        triangles = new int[6 * vertexAmount * ringCount];

        GrowBranches();
        CreateVertixes();
        CreatePolygons();

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        meshf.mesh = mesh;
        if (branchMat != null)
            meshr.material = branchMat;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return (mesh);
    }
}
