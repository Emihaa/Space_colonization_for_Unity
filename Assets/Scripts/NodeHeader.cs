using UnityEngine;
using System.Collections.Generic;


/* 
    https://algorithmicbotany.org/papers/colonization.egwnp2007.large.pdf
*/

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
    public float _thickness;
    public float _length;
    public Vector3 _direction;
    public List<Vector3> _attractors = new List<Vector3>();
    public Vector3[] _vertices;

    public Node(Vector3 pos, Vector3 dir)
    {
        _pos = pos;
        _prev = null;
        _next = null;
        _index = 0;
        _thickness = 0.005f;
        _length = 0.025f;
        _direction = dir;
        _vertices = CreateCircle(_pos, _direction, _thickness);
    }

    public Vector3[] CreateCircle(Vector3 position, Vector3 dir, float radius)
    {
        Vector3[] circle = new Vector3[5];
        Quaternion quat = Quaternion.FromToRotation(Vector3.up, dir);
        for (int i = 0; i < 5; i++)
        {
            float angle = ((float)i / 5) * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            pos = quat * pos; // rotate circle to matchbranch direction
            pos += position; // move circle to node position
            circle[i] = pos; // add the vertec to the array
        }

        return (circle);
    }
}
