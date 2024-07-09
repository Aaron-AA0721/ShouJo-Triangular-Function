using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatableAngleBridge : ControllableAngleBridge
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void UpdateGraphic()
    {
        base.UpdateGraphic();
    }
    protected override void AngleCorrectTrigger()
    {
        base.AngleCorrectTrigger();
        RotateXLine();
    }

    void RotateXLine()
    {
        if (xLine)
        {
            xLineOn = !xLineOn;
            StartCoroutine(Rotateline(xLine,xLineOn ? angle : -angle,1f));
        }
    }
    
}
