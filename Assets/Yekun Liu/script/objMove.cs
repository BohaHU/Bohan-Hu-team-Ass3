using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class objMove : MonoBehaviour
{
    Transform myCamera;  
    private CharacterController cc;
    Vector3 playerDirect;        
    Vector3 correctDirect;       
    float perError = 0.2f;       
    float speed = 0.1f;            
    Vector3 front;  
    Vector3 back;  
    Vector3 left;    
    Vector3 right; 
    public Animator ani;
    private bool ismove;

    public GameObject keytext1;
    public GameObject keyytext2;
    public Text timetext;
    private float time = 120;
    private int times;
    public GameObject YouWin;
    public GameObject music;

    public GameObject GameOver;
    
    private float number;
   
    void Start()
    {
        myCamera = Camera.main.transform;  
        front = Vector3.zero;
        back = Vector3.zero;
        left = Vector3.zero;
        right = Vector3.zero;
        correctDirect = Vector3.zero;
        cc = transform.GetComponent<CharacterController>();
    }

    
    void Update()
    {
        if (time>=0)
        {
            time -= Time.deltaTime;
            times = (int)time % 181;  
            timetext.text = string.Format("Time :{0}", times);
        }
       
        if (time<=0)
        {
            GameOver.SetActive(true);
        
            speed = 0;
        }
        if (number == 2 || number >= 0)
        {
            number -= Time.deltaTime;
            if (number <= 0)
            {
               
                keytext1.SetActive(false);
                number = 0;
            }
        }
        if (number == 3 || number >= 0)
        {
            number -= Time.deltaTime;
            if (number <= 0)
            {
                keyytext2.SetActive(false);
                number = 0;
            }
        }
        PlayerControl();
    }
    private void CalculateDirection()
    {
        Vector3 temp = transform.position - myCamera.position;
        temp = new Vector3(temp.x, 0, temp.z);
        front = temp;  


        if (correctDirect != Vector3.zero && Mathf.Abs(correctDirect.x - front.x) < perError && Mathf.Abs(correctDirect.z - front.z) < perError)  
        {
            front = correctDirect;
        }
        back = -front;
        left = new Vector3(-front.z, 0, front.x);
        right = -left;
        correctDirect = front;
    }

    /// <summary>
    /// 控制角色移动
    /// </summary>
    private void PlayerControl()
    {
        playerDirect = new Vector3(0, 0, 0);

        CalculateDirection();    

        if (Input.GetAxis("Horizontal") < 0)  
        {
            playerDirect = left;
        }
        if (Input.GetAxis("Horizontal") > 0)  
        {
            playerDirect = right;
        }
        if (Input.GetAxis("Vertical") > 0)
        {
            if (Input.GetAxis("Horizontal") < 0)  
            {
                playerDirect = front + left;
            }
            else if (Input.GetAxis("Horizontal") > 0) 
            {
                playerDirect = front + right;
            }
            else
            {
                playerDirect = front;     
            }
        }
        if (Input.GetAxis("Vertical") < 0)
        {
            if (Input.GetAxis("Horizontal") < 0)   
            {
                playerDirect = back + left;
            }
            else if (Input.GetAxis("Horizontal") > 0)   
            {
                playerDirect = back + right;
            }
            else
            {
                playerDirect = back;     
            }
        }
        if (playerDirect != Vector3.zero)
        {
            playerDirect += transform.position;
            MoveSpeed(playerDirect);
        }
        else
        {
            if (ismove)
            {
                ismove = false;
                ani.SetBool("idle_bool", true);
                ani.SetBool("run_bool", false);
            }


        }
    }


    private void MoveSpeed(Vector3 direct)
    {
        ismove = true;
        ani.SetBool("idle_bool", false);
        ani.SetBool("run_bool", true);
        transform.LookAt(new Vector3(direct.x, transform.position.y, direct.z));
        cc.Move(transform.forward * speed);
    }
   
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "key")
        {
            keytext1.SetActive(true);
            Destroy(other.gameObject);
            number = 2;
        }
        if (other.gameObject.tag == "keyy")
        {
            keyytext2.SetActive(true);
            Destroy(other.gameObject);
            number = 3;
            music.gameObject.name = "Realkey";
        }
        if (other.gameObject.tag == "keyexit")
        {
            if (music.gameObject.name == "Realkey")
            {
                YouWin.SetActive(true);
               
                speed = 0;
            }
        }
    }
}
