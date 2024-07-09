using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private float moveX;
    private float moveY;
    private Rigidbody2D RigidBody;
    private Vector2 LookDirection;
    [SerializeField] private float moveSpeed = 7.5f;
    [SerializeField] private bool IsMovingControllable = true;
    [SerializeField] private int JumpCount = 1;
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private bool IsOnGround = true;
    [SerializeField] private int CurrentTool = 0;
    private Animator animator;
    [SerializeField] public static List<ControllableAngleBridge> Bridges = new List<ControllableAngleBridge>();
    [SerializeField] private int maxJump = 1;
    private bool isRotatingUnitCircle;
    [SerializeField] GameObject LeftArmLimb;
    [SerializeField] GameObject RightArmLimb;
    [SerializeField] GameObject LeftLegLimb;
    [SerializeField] GameObject RightLegLimb;
    [SerializeField] GameObject[] ToolControllers;
    [SerializeField] GameObject[] ToolsOnHand;
    void Start()
    {
        RigidBody = GetComponent<Rigidbody2D>();
        LookDirection = new Vector2(0, 0);
        animator = GetComponent<Animator>();
        SwitchTool(true);
    }

    // Update is called once per frame
    void Update()
    {
        Control();
        transform.localScale = new Vector2 (LookDirection.x < 0 ? -1f : 1f,1f);
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchTool(true);
            // UnitCircleController.SetActive(!UnitCircleController.activeSelf);
            // CircleOnHand.SetActive(!CircleOnHand.activeSelf);
        }
        else if (Input.GetKeyDown(KeyCode.Q)) SwitchTool(false);
        if(Input.GetKeyDown(KeyCode.Tab))EnableOrDisableTool();
    }

    void LateUpdate()
    {
        // Control();
        // transform.localScale = new Vector2 (LookDirection.x < 0 ? -1f : 1f,1f);
    }

    void SwitchTool(bool forward)
    {
        ToolControllers[CurrentTool].SetActive(false);
        ToolsOnHand[CurrentTool].SetActive(false);
        CurrentTool += forward ? 1 : -1;
        if (CurrentTool >= ToolsOnHand.Length) CurrentTool = 0;
        if (CurrentTool < 0) CurrentTool = ToolsOnHand.Length - 1;
        ToolsOnHand[CurrentTool].SetActive(true);
    }

    void EnableOrDisableTool()
    {
        if(ToolControllers[CurrentTool])
        ToolControllers[CurrentTool].SetActive(!ToolControllers[CurrentTool].activeSelf);
    }
    void Control()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
        
        // RigidBody.velocity = (Mathf.Abs(moveSpeed * moveX)<Mathf.Abs(RigidBody.velocity.x) && moveX * RigidBody.velocity.x>0)?
        //     new Vector2(RigidBody.velocity.x, RigidBody.velocity.y) : new Vector2(moveSpeed * moveX, RigidBody.velocity.y);
        RigidBody.velocity = new Vector2(moveSpeed * moveX, RigidBody.velocity.y);
        animator.SetBool("isMoving",moveX !=0);
        //Debug.Log("Why am I moving");
        if (!Mathf.Approximately(moveX,0) )
        {
            LookDirection.Set(moveX, moveY);
        }
        if(moveY>0)JumpUp();
    }
    void JumpUp(){
        //Input.GetButtonDown("Jump") && 
        if(JumpCount > 0 && IsMovingControllable)
        {
            //StartCoroutine(PlayAudio("Jump"));
            RigidBody.velocity = new Vector2(RigidBody.velocity.x, jumpSpeed);
            if(IsOnGround && animator)animator.SetTrigger("Jump");
            JumpCount--;
        }
    }
    void ReSetJumpCount(){
        JumpCount=maxJump;
    }
    public void OnGround(){
        //Debug.Log("Onground");
        IsOnGround=true;
        ReSetJumpCount();
    }
    public void OffGround(){
        //Debug.Log("Offground");
        //JumpCount--;
        IsOnGround=false;
    }

    public void StartRotatingUnitCircle()
    {
        isRotatingUnitCircle = true;
        animator.SetBool("isRotatingUnitCircle",isRotatingUnitCircle);
        UpdateRotatingUnitCircleArm(0);
    }

    public void UpdateRotatingUnitCircleArm(float angle)
    {
        if (isRotatingUnitCircle)
        {
            LeftArmLimb.transform.position = new Vector3(
            RightArmLimb.transform.position.x + 0.2f * Mathf.Cos(angle / 180 * Mathf.PI),
            RightArmLimb.transform.position.y + 0.2f * Mathf.Sin(angle / 180 * Mathf.PI),
            0);
        }
        
    }
    public void StopRotatingUnitCircle()
    {
        isRotatingUnitCircle = false;
        animator.SetBool("isRotatingUnitCircle",isRotatingUnitCircle);
    }
}
