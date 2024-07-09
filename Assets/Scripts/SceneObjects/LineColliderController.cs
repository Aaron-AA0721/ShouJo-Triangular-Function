using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LineColliderController : MonoBehaviour
{
    // Start is called before the first frame update
    private LineRenderer line;
    private EdgeCollider2D edgeCollider;
    void Start()
    {
        if (GetComponent<Collider2D>() == null)
        {
            this.AddComponent<EdgeCollider2D>();
        }
        CheckVariable();
        if (edgeCollider)
        {
            UpdateCollider();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdatePosition(Vector3[] points)
    {
        CheckVariable();
        line.SetPositions(points);
        if (edgeCollider)
        {
            Vector2[] vector2Points = new Vector2[points.Length];

            // Convert each element of the Vector3 array to Vector2
            for (int i = 0; i < points.Length; i++)
            {
                vector2Points[i] = new Vector2(points[i].x, points[i].y);
            }
            edgeCollider.SetPoints(vector2Points.ToList());
        }
    }

    public void UpdateCollider(Vector2[] points = default)
    {
        //Debug.Log(points);
        CheckVariable();
        if (edgeCollider)
        {
            if(points != null)edgeCollider.SetPoints(points.ToList());
            else
            {
                Vector3[] vector3Points = new Vector3[line.positionCount];
                Vector2[] vector2Points = new Vector2[line.GetPositions(vector3Points)];
                for (int i = 0; i < vector3Points.Length; i++)
                {
                    vector2Points[i] = new Vector2(vector3Points[i].x, vector3Points[i].y);
                }
                edgeCollider.SetPoints(vector2Points.ToList());
            }
        }
        
    }

    void CheckVariable()
    {
        if (!line) line = GetComponent<LineRenderer>();
        if (!edgeCollider) edgeCollider = GetComponent<EdgeCollider2D>();
    }
}
