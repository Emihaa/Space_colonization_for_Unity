using UnityEngine;
using System.Collections.Generic;

public class AttractionPointGenerator
{
    private GameObject      target;
    private GameObject      sun;
    private int             attractorAmount;
    private float           offsetDistance;
    private bool            sunEffect;
    private List<Vector3>   attractorPoints;

    public AttractionPointGenerator(GameObject target, GameObject sun, int attractorAmount, float offsetDistance, bool sunEffect, List<Vector3> attractorPoints)
    {
        this.target             = target;
        this.sun                = sun;
        this.attractorAmount    = attractorAmount;
        this.offsetDistance     = offsetDistance;
        this.sunEffect          = sunEffect;
        this.attractorPoints    = attractorPoints;
    }

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
    // barycentric coordinates
    private Vector3 RandomPosOnTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
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

    // r = randomized float from 0 to totalArea
    // for loop to run throug the triangleArea weight list till we reach
    // weight that is equal or more to the r value
    // then we choose that triangle index multiply by 3 to match with the int[] triangle length
    // because if there are 6 triangles in the mesh:
    // triangleAreas.count is 6
    // int[] triangle length is 18 as it holds triple the information as one triangle = 3 vertices = triangle[i] +  triangle[i + 1] + triangle[i + 2]
    private int PickArea(List<float> triangleAreas, float totalArea)
    {
        int triIndex = 0;
        float cumulative = 0f;
        float r = Random.value * totalArea;
        for (int i = 0; i < triangleAreas.Count; i++) 
        {
            cumulative += triangleAreas[i];
            if (r <= cumulative)
            {
                triIndex = i * 3;
                break ;
            }
        }
        return (triIndex);
    }

    private void PlacePoint(List<float> triangleAreas, ref float totalArea, Vector3[] normals, Vector3[] vertices, int[] triangles)
    {
        Transform t = target.transform;

        while (attractorPoints.Count < attractorAmount)
        {
            int triIndex = PickArea(triangleAreas, totalArea);

            Vector3 v0 = vertices[triangles[triIndex]];
            Vector3 v1 = vertices[triangles[triIndex + 1]];
            Vector3 v2 = vertices[triangles[triIndex + 2]];

            Vector3 normal = (normals[triangles[triIndex]] + normals[triangles[triIndex + 1]] + normals[triangles[triIndex + 2]]).normalized;
            Vector3 localPos = RandomPosOnTriangle(v0, v1, v2);
            localPos += normal * offsetDistance;
            Vector3 worldPos = t.TransformPoint(localPos);

            attractorPoints.Add(worldPos);
        }
    }

    // checks if we take account the sun direction
    // calculates the total weight of all the triangle areas and adds each calculated area mass of each triangle to list
    // Vector3.Cross(v1 - v0, v2- v0).magnitude * 0.5f; <- v1 -v0 = edge vector
    // 
    private void WeightAreas(List<float> triangleAreas, ref float totalArea, Vector3[] normals, Vector3[] vertices, int[] triangles)
    {
        int amount = triangles.Length;
        for (int i = 0; i < amount; i += 3)
        {
            float area = 0;
            float dot = 0;
            if (sunEffect == true && sun != null)
            {
                Vector3 normal = (normals[triangles[i]] + normals[triangles[i + 1]] + normals[triangles[i + 2]]).normalized;
                Vector3 sunDir = -sun.transform.forward;
                dot = Vector3.Dot(normal, sunDir);
            }
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];
            area = Vector3.Cross(v1 - v0, v2- v0).magnitude * 0.5f;

            if (sunEffect == true && sun != null && dot < 0)
                area = 0;

            triangleAreas.Add(area);
            totalArea += area;
        }
    }

    public void GenerateAttractors()
    {
        Mesh mesh = target.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] normals = mesh.normals;

        attractorPoints.Clear();
        List<float> triangleAreas = new List<float>();
        float totalArea = 0f; // can we reach max float val? maybe we need to have larger val than float or check for overflow

        WeightAreas(triangleAreas, ref totalArea, normals, vertices, triangles);
        PlacePoint(triangleAreas, ref totalArea, normals, vertices, triangles);
    }
}
