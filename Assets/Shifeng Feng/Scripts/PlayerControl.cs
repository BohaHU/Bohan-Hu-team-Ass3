using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerControl : NetworkBehaviour
{

    // Use this for initialization
    public float speed;
    public Text intscore;
   // [SyncVar(hook = "OnValueChange")]
    public static int score = 0;
   // private Client client;

    public GameObject winText;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        //client = GameObject.Find("GameManager").GetComponent<Client>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            score++;
            intscore.text = score.ToString();
            if (score > 5)
            {
                print("hao");
                winText.SetActive(true);
            }
        }
    }
    // Update is called once per frame
    //void FixedUpdate()
    //{
    //    if (!isLocalPlayer)
    //        return;

    //    float moveHorizontal = Input.GetAxis("Horizontal");
    //    float moveVertical = Input.GetAxis("Vertical");
    //    Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
    //    //AddForce
    //    GetComponent<Rigidbody>().AddForce(movement * speed * Time.deltaTime);

    //}
    private void Update()
    {
        //if (client.state == ClientState.anotherPlayer)
        //{
        //    transform.position = client.point;
        //    return;
        //}

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * speed);
        Vector3 pos = transform.position;
       // client.SavePoint(pos);
        if (score >= 5)
        {
            winText.SetActive(true);
        }
    }
    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    public void OnValueChange()
    {

    }
}
