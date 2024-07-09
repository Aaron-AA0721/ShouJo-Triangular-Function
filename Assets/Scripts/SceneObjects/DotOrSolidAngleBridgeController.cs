using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DotOrSolidAngleBridgeController : ControllableAngleBridge
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
        UpdateTransparency();
    }
    protected override void AngleCorrectTrigger()
    {
        base.AngleCorrectTrigger();
        ReverseLines();
    }
    public void ReverseLines()
    {
        xLineOn = !xLineOn;
        radiusLineOn = !radiusLineOn;
        circleOn = !circleOn;
        sinLineOn = !sinLineOn;
        cosLineOn = !cosLineOn;
        UpdateTransparency();
    }

    void UpdateTransparency()
    {
        if (xLine)
        {
            xLine.transform.GetChild(0).gameObject.SetActive(!xLineOn);
            xLine.transform.GetChild(1).gameObject.SetActive(xLineOn);
        }

        if (radiusLine)
        {
            radiusLine.transform.GetChild(0).gameObject.SetActive(!radiusLineOn);
            radiusLine.transform.GetChild(1).gameObject.SetActive(radiusLineOn);
        }

        if (circle)
        {
            circle.transform.GetChild(0).gameObject.SetActive(!circleOn);
            circle.transform.GetChild(1).gameObject.SetActive(circleOn);
        }

        if (sinLine)
        {
            sinLine.transform.GetChild(0).gameObject.SetActive(!sinLineOn);
            sinLine.transform.GetChild(1).gameObject.SetActive(sinLineOn);
        }
        if (cosLine)
        {
            cosLine.transform.GetChild(0).gameObject.SetActive(!cosLineOn);
            cosLine.transform.GetChild(1).gameObject.SetActive(cosLineOn);
        }
    }

}
