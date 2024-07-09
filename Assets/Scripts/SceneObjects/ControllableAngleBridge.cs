using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllableAngleBridge : MonoBehaviour
{
    [SerializeField] protected GameObject xLine;

    [SerializeField] protected GameObject radiusLine;

    [SerializeField] protected GameObject circle;
    [SerializeField] protected GameObject sinLine;
    [SerializeField] protected GameObject cosLine;
    [SerializeField] protected bool xLineOn = false;
    [SerializeField] protected bool radiusLineOn = false;
    [SerializeField] protected bool circleOn = false;
    [SerializeField] protected bool sinLineOn = false;
    [SerializeField] protected bool cosLineOn = false;
    [SerializeField] protected float angle;//[0,360)

    [SerializeField] protected float radius; //unit length
    protected virtual void Start()
    {
        UpdateGraphic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    protected virtual void UpdateGraphic()
    {
        if (xLine)
        {
            xLine.transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(1,new Vector3(radius*2,0,0));
            xLine.transform.GetChild(1).GetComponent<LineColliderController>().UpdatePosition(new Vector3[2]{new Vector3(0,0,0),new Vector3(radius*2,0,0)});
        }

        if (radiusLine)
        {
            radiusLine.transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(1,
                new Vector3(radius*2*Mathf.Cos(angle/180*Mathf.PI),radius*2*Mathf.Sin(angle/180*Mathf.PI),0));
            radiusLine.transform.GetChild(1).GetComponent<LineColliderController>().UpdatePosition(new Vector3[2]{new Vector3(0,0,0),
                new Vector3(radius*2*Mathf.Cos(angle/180*Mathf.PI),radius*2*Mathf.Sin(angle/180*Mathf.PI),0)});
        }

        if (circle)
        {
            circle.transform.GetChild(0).GetComponent<CircleLineController>().SetCircle(radius,angle);
            circle.transform.GetChild(1).GetComponent<CircleLineController>().SetCircle(radius,angle);
            circle.transform.GetChild(1).GetComponent<LineColliderController>().UpdateCollider();
        }

        if (sinLine)
        {
            sinLine.transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(0,
                new Vector3(radius*2*Mathf.Cos(angle/180*Mathf.PI),radius*2*Mathf.Sin(angle/180*Mathf.PI),0));
            sinLine.transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(1,
                new Vector3(radius*2*Mathf.Cos(angle/180*Mathf.PI),0,0));
            sinLine.transform.GetChild(1).GetComponent<LineColliderController>().UpdatePosition(new Vector3[2]{
                new Vector3(radius*2*Mathf.Cos(angle/180*Mathf.PI),radius*2*Mathf.Sin(angle/180*Mathf.PI),0),
                new Vector3(radius*2*Mathf.Cos(angle/180*Mathf.PI),0,0)});
        }

        if (cosLine)
        {
            cosLine.transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(0,
                new Vector3(0,0,0));
            cosLine.transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(1,
                new Vector3(radius*2*Mathf.Cos(angle/180*Mathf.PI),0,0));
            cosLine.transform.GetChild(1).GetComponent<LineColliderController>().UpdatePosition(new Vector3[2]{
                new Vector3(0,0,0),
                new Vector3(radius*2*Mathf.Cos(angle/180*Mathf.PI),0,0)});
        }
    }

    public void TriggerAngleAnswer(float degree)
    {
        degree -= 360 * Mathf.FloorToInt(degree / 360);
        if (Mathf.Abs(degree-angle)<1)
        {
            AngleCorrectTrigger();
        }
    }

    protected virtual void AngleCorrectTrigger()
    {
        
    }
    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>())
        {
            PlayerController.Bridges.Add(this);
            Debug.Log("player enter");
        }
            
    }
    protected void OnTriggerExit2D(Collider2D other)
    {
        if(other.GetComponent<PlayerController>())
            if (PlayerController.Bridges.Contains(this))
            {
                PlayerController.Bridges.Remove(this);
                Debug.Log("player exit");
            }
    }

    protected IEnumerator Rotateline(GameObject line, float degree,float duration = 1f)
    {
        float timer = 0f;
        Vector3 currentRotation = line.transform.localRotation.eulerAngles;
        while (timer < duration)
        {
            line.transform.localRotation = Quaternion.Euler(currentRotation + new Vector3(0, 0, degree * timer / duration));
            yield return null;
            timer += Time.deltaTime;
        }
    }
}
