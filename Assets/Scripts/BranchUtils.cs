using UnityEngine;

public static class BranchUtils
{

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
    public static Vector3 RandomPosOnTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
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

    // creates a new Node of each branch and connects it to the correct counterpart
    // not so sure of these yet
    public static Node NewNode (Vector3 pos, Node nextNode, Node prevNode, int index)
    {
        Node newNode = new Node(pos);
        newNode._next = nextNode;
        newNode._prev = prevNode;
        newNode._index = index;

        return (newNode);
    }

}
