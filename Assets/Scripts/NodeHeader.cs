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

    public Node(Vector3 pos, Vector3 dir)
    {
        _pos = pos;
        _prev = null;
        _next = null;
        _index = 0;
        _thickness = 0.005f;
        _length = 0.025f;
        _direction = dir;
    }
}
