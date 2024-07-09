using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;

public class CircleLineController : MonoBehaviour
{
    [SerializeField] float radius = 1;

    [SerializeField] float angle = 0;

    [SerializeField] int stepNum = 720;

    private LineRenderer line;
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        //DrawCircle(stepNum, radius*2, angle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCircle(float r, float arc)
    {
        radius = r ;
        angle = arc;
        DrawCircle(stepNum,r*2,arc);
    }
    void DrawCircle(int steps, float r, float degree)
    {
        if(!line)line = GetComponent<LineRenderer>();
        int num = Mathf.CeilToInt(steps * degree / 360) + 1;
        line.positionCount = num;
        for (int i = 0; i < steps && i<num; i++)
        {
            line.SetPosition(i,new Vector3(Mathf.Cos(((float)i/steps)*2f*Mathf.PI)*r,Mathf.Sin(((float)i/steps)*2f*Mathf.PI)*r,0));
            //Debug.Log((float)i*2f*Mathf.PI/steps*r);
        }
    }
}
