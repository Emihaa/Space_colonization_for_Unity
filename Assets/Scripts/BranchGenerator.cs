using UnityEngine;
using System.Collections.Generic;

// better generate the logic before play time and during the pay time it will grow to be

public class BranchGenerator : MonoBehaviour
{
    public GameObject   target;
    public GameObject   sun;
    public int          grow                    = 10;
    public int          attractorAmount         = 500;
    public float        killRadius              = 0.2f;
    public float        attractionRadius        = 0.4f;
    public float        offsetDistance          = 0.1f;
    public float        branchLen               = 0.05f;
    
    [System.NonSerialized] public bool  showAttractionRadius    = false;
    [System.NonSerialized] public bool  showKillRadius          = false;
    [System.NonSerialized] public bool  showLines               = false;
    public bool                         sunEffect               = false;                     

    private List<Vector3>   attractorPoints     = new List<Vector3>();
    private List<Node>      nodesList           = new List<Node>();

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
            nodesList.Clear();
            AttractionPointGenerator generator = new AttractionPointGenerator(target, sun, attractorAmount, offsetDistance, sunEffect, attractorPoints);
            generator.GenerateAttractors();
            NodeGenerator nodes = new NodeGenerator(this.transform, grow, killRadius, attractionRadius, branchLen, attractorPoints);
            nodesList = nodes.CreateNodes();
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
