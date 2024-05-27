using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler,IDragHandler,IEndDragHandler, IDropHandler, IPointerEnterHandler,IPointerExitHandler
{
    // Start is called before the first frame update
    public RectTransform Rect { get; protected set; }
    private Vector2 InitialPos;
    public Image Render_Image { get; protected set; } = null;
    public bool isDraggable  = false;
    void Start()
    {
        Rect = GetComponent<RectTransform>();
        InitialPos = Rect.localPosition;
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
            InitialPos = Rect.localPosition;
        }
        else return;
        //Debug.Log("OnBeginDrag, "+name);
    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag, "+name);
        if (isDraggable)
        {
            Rect.localPosition += (Vector3)eventData.delta;
        }
        else return;
    }
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (isDraggable)
        {
            Render_Image.raycastTarget = true;
            Rect.localPosition = InitialPos;
        }
        else return;
        Debug.Log("OnEndDrag, "+name);
    }
    public virtual void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop, "+name);
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown, "+name);
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter, "+name);
        int parentNum = 0;
        Transform t = transform;
        while (t.parent != null)
        {
            t = t.parent;
            parentNum++;
        }

        if (Render_Image != null)
        {
            Render_Image.color = new Color(1- parentNum/10f, 1- parentNum/10f, 1- parentNum/10f, 1);
        }
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit, "+name);
        if (Render_Image != null)
        {
            Render_Image.color = new Color(1, 1, 1, 1);
        }
    }
}
