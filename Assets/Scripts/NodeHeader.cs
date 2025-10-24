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
    public GameObject _mesh;
    public float _thickness;
    public float _length;
    public List<Vector3> _attractors = new List<Vector3>();

    public Node(Vector3 pos)
    {
        _pos = pos;
        _prev = null;
        _next = null;
        _index = 0;
        _mesh = null;
        _thickness = 0.05f;
        _length = 0.1f;
    }
}
