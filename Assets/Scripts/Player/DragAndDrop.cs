using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler,IDragHandler,IEndDragHandler, IDropHandler, IPointerEnterHandler,IPointerExitHandler
{
    // Start is called before the first frame update
    public RectTransform Rect { get; protected set; }
    public Vector2 InitialPos;
    public Image Render_Image { get; protected set; } = null;
    private Color OriginalColor;
    public bool isDraggable  = true;
    [SerializeField] protected bool ResetPos = true;
    protected virtual void Start()
    {
        Rect = GetComponent<RectTransform>();
        if(Render_Image == null)Render_Image = GetComponent<Image>();
        InitialPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag, "+name);
        if (isDraggable)
        {
            Render_Image.raycastTarget = false;
            InitialPos = transform.localPosition;
            transform.position = (Vector2)Camera.main.ScreenToWorldPoint(eventData.position) + new Vector2(0,Rect.sizeDelta.y/2+10);
        }
        else return;
        //Debug.Log("OnBeginDrag, "+name);
    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag, "+name);
        if (isDraggable)
        {
            transform.position += (Vector3)eventData.delta;
        }
        else return;
    }
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (isDraggable)
        {
            Render_Image.raycastTarget = true;
            if(ResetPos)transform.localPosition = InitialPos;
        }
        else return;
        //Debug.Log("OnEndDrag, "+name);
    }
    public virtual void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop, "+name);
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("OnPointerDown, "+name);
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("OnPointerEnter, "+name);
        int parentNum = 0;
        Transform t = transform;
        while (t.parent != null)
        {
            t = t.parent;
            parentNum++;
        }

        if (Render_Image != null)
        {
            OriginalColor = Render_Image.color;
            Render_Image.color = new Color(OriginalColor.r*(1- parentNum/10f), (1- parentNum/10f)*OriginalColor.g, (1- parentNum/10f)*OriginalColor.b, 1);
        }
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("OnPointerExit, "+name);
        if (Render_Image != null)
        {
            Render_Image.color = OriginalColor;
        }
    }
}
