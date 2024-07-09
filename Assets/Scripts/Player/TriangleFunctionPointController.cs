using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class TriangleFunctionPointController : DragAndDrop
{
    // Start is called before the first frame update
    public GameObject updateStatus;
    public LineRenderer RadiusLine;
    public LineRenderer yLine;
    public LineRenderer xLine;
    public MathExpression Equation;
    public Image angleCurve;
    public Image arcCurve;
    private float prevAngle;
    private float angle;
    [SerializeField]private float RadiusLength;
    public Text degreeText;
    public Text radText;
    public Text sinText;
    public Text cosText;
    public Text tanText;
    private PlayerController _Controller;
    protected override void Start()
    {
        base.Start();
        angle = 0;
        prevAngle = 0;
        _Controller = FindObjectOfType<PlayerController>();
    }
    // Update is called once per frame
    void LateUpdate()
    {
        transform.parent.localScale = new Vector3(transform.parent.parent.parent.localScale.x, 1, 1);
        if (updateStatus.transform.localPosition.x * transform.parent.localScale.x > 0)
            updateStatus.transform.localPosition = new Vector3(-updateStatus.transform.localPosition.x,
                updateStatus.transform.localPosition.y, updateStatus.transform.localPosition.z);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        _Controller.StartRotatingUnitCircle();
    }
    public override void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag, "+name);
        if (isDraggable)
        {
            Vector2 MousePosition = Camera.main.ScreenToWorldPoint(eventData.position) - transform.parent.position;
            transform.localPosition = new Vector2(MousePosition.normalized.x*RadiusLength,MousePosition.normalized.y*RadiusLength);
            UpdateGraphic();
        }
        else return;
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        TriggerAngle(angle);
        if (ResetPos) angle = prevAngle = 0;
        UpdateGraphic();
        _Controller.StopRotatingUnitCircle();
    }
    private void UpdateGraphic()
    {
        angle = Vector2.Angle(transform.localPosition, Vector2.right);
        angle = (transform.localPosition.y < 0 ? 360 - angle : angle);
        if (angle > 270 && ((prevAngle%360 < 90 && prevAngle%360 >= 0) || (prevAngle%360 <-270 && prevAngle%360 >-360)) ) angle = (Mathf.Floor(prevAngle / 360)-1)*360+angle;
        else if (angle < 90 && ((prevAngle%360 > -90 && prevAngle%360 < 0) || (prevAngle%360 > 270 && prevAngle%360 < 360)) ) angle = (Mathf.Floor(prevAngle / 360)+1)*360+angle;
        else angle = Mathf.Floor(prevAngle / 360)*360+angle;
        prevAngle = angle;
        _Controller.UpdateRotatingUnitCircleArm(angle);
        // if (updateStatus)
        // {
        //     updateStatus.text = "";
        //     if (degreeDisplayFlag)
        //     {
        //         updateStatus.text += "θ = " + angle.ToString("#0")+"\n";
        //     }
        //
        //     if (radDisplayFlag)
        //     {
        //         updateStatus.text += "θ = " + (angle/180).ToString("#0.00")+"π"+"\n";
        //     }
        //
        //     if (sinDisplayFlag)
        //     {
        //         updateStatus.text += "Sinθ = " + Mathf.Sin(angle/180*Mathf.PI).ToString("#0.00")+"\n";
        //     }
        //
        //     if (cosDisplayFlag)
        //     {
        //         updateStatus.text += "Cosθ = " + Mathf.Cos(angle/180*Mathf.PI).ToString("#0.00")+"\n";
        //     }
        //
        //     if (tanDisplayFlag)
        //     {
        //         updateStatus.text += "Tanθ = " + Mathf.Tan(angle/180*Mathf.PI).ToString("#0.00")+"\n";
        //     }
        //     
        //         //"Degree: " + angle.ToString("#0.00") +"\nRadian:"+(angle/180).ToString("#0.00")+"Π\nSin: "+Mathf.Sin(angle/180*Mathf.PI).ToString("#0.00") +"\nCos: "+Mathf.Cos(angle/180*Mathf.PI).ToString("#0.00")+"\nTan: "+Mathf.Tan(angle/180*Mathf.PI).ToString("#0.00") ;
        // }

        if (degreeText)
        {
            degreeText.text = "θ = " + angle.ToString("#0");
        }

        if (radText)
        {
            radText.text = "θ = " + (angle / 180).ToString("#0.00") + "π";
        }
        if (sinText)
        {
            sinText.text = "Sinθ = " + Mathf.Sin(angle/180*Mathf.PI).ToString("#0.00");
        }
        if (cosText)
        {
            cosText.text = "Cosθ = " + Mathf.Cos(angle/180*Mathf.PI).ToString("#0.00");
        }
        if (tanText)
        {
            tanText.text = "Tanθ = " + Mathf.Tan(angle/180*Mathf.PI).ToString("#0.00");
        }
        if (RadiusLine)
        {
            RadiusLine.positionCount = 2;
            RadiusLine.SetPosition(0,new Vector3(0,0,0));
            RadiusLine.SetPosition(1,new Vector3(transform.localPosition.x,transform.localPosition.y,0));
        }
        
        if (yLine)
        {
            yLine.positionCount = 2;
            yLine.SetPosition(0,new Vector3(transform.position.x,transform.position.y,0));
            yLine.SetPosition(1,new Vector3(transform.position.x,transform.parent.position.y,0));
        }

        if (xLine)
        {
           xLine.positionCount = 2;
           xLine.SetPosition(0,new Vector3(transform.position.x,transform.parent.position.y,0));
           xLine.SetPosition(1,new Vector3(transform.parent.position.x,transform.parent.position.y,1)); 
        }

        if (angleCurve)
        {
            angleCurve.fillClockwise = angle < 0;
            angleCurve.fillAmount = Mathf.Abs(angle) / 360;
        }

        if (arcCurve)
        {
            arcCurve.fillClockwise = angle < 0;
            arcCurve.fillAmount = Mathf.Abs(angle) / 360;
        }
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
    }

    void TriggerAngle(float degree)
    {
        if (PlayerController.Bridges.Count>0)
        {
            if (degree < 0 || degree >= 360) degree -= 360 * Mathf.FloorToInt(degree / 360);
            Debug.Log(degree);
            foreach (var bridge in PlayerController.Bridges)
            {
                bridge.TriggerAngleAnswer(degree);
            }
        }
    }
}
