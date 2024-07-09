using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayeGroundCheck : MonoBehaviour
{
    PlayerController _Controller;
    // Start is called before the first frame update
    void Start()
    {
        _Controller = this.gameObject.transform.parent.gameObject.GetComponent<PlayerController>();
        if(_Controller == null) Debug.LogError("Cannot find Player-Fox!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D Col){
        if(Col.gameObject.tag == "Ground"){
            //Debug.Log("Ground!");
            _Controller.OnGround();
        }
    }
    void OnTriggerExit2D(Collider2D Col){
        if(Col.gameObject.tag == "Ground"){
            _Controller.OffGround();
        }
    }
}
