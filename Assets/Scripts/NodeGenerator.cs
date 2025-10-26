using UnityEngine;
using System.Collections.Generic;

public class NodeGenerator
{
    Transform               obj;
    private int             grow;
    private float           killRadius;
    private float           attractionRadius;
    private float           branchLen;
    private int             index;
    private List<Vector3>   attractorPoints;
    private List<Node>      nodesList = new List<Node>();

    public NodeGenerator(Transform obj, int grow, float killRadius, float attractionRadius, float branchLen, List<Vector3> attractorPoints)
    {
        this.obj                = obj;
        this.grow               = grow;
        this.killRadius         = killRadius;
        this.attractionRadius   = attractionRadius;
        this.branchLen          = branchLen;
        this.attractorPoints    = attractorPoints;
    }

    // creates a new Node of each branch and connects it to the correct counterpart
    // not so sure of these yet
    private Node NewNode (Vector3 pos, Vector3 dir, Node nextNode, Node prevNode, int index)
    {
        Node newNode = new Node(pos, dir);
        newNode._next = nextNode;
        newNode._prev = prevNode;
        newNode._index = index;

        return (newNode);
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
            {
                tempNode._attractors.Add(point);
            }
        }
    }

    // goes through the nodesList checking if any of the Nodes have attractors affecting them
    // and then creates a new node to the normalized direction of those attractors
    private bool GenerateNewNodes(ref int index, Node rootNode)
    {
        bool grow = false;
        Node prevNode = rootNode;
        for (int i = 0; i < nodesList.Count; i++)
        {
            Node node = nodesList[i];
            if (node._attractors.Count != 0)
            {
                Vector3 pos = new Vector3(0, 0, 0);
                foreach (var point in node._attractors)
                {
                    pos += (point - node._pos).normalized;
                }
                pos /= node._attractors.Count;
                pos.Normalize();
                pos = node._pos + pos * branchLen;
                foreach (var point in node._attractors)
                {
                    if ((point - pos).magnitude <= killRadius)
                        attractorPoints.Remove(point);
                }
                Node newNode = NewNode(pos, (node._pos - pos).normalized, node, prevNode, index);
                nodesList.Add(newNode);
                node._attractors.Clear();
                grow = true;
                prevNode = newNode;
                index++;
            }
        }
        return (grow);
    } 

    public List<Node> CreateNodes ()
    {
        Node rootNode = NewNode(obj.position , new Vector3(0,0,0), null, null, 0);
        nodesList.Add(rootNode);
        index = 1;
        for (int i = 0; i < grow; i++)
        {
            SearchAttractorPoints();
            if (GenerateNewNodes(ref index, rootNode) == false)
            {
                Debug.Log("no new nodes");
                break ;
            }
        }
        return (nodesList);
    }
}
