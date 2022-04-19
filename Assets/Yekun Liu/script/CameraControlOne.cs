using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlOne : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField]
    float target_offsety = 0.6f;   

    [Range(0, 90)]
    [SerializeField]
    float initRotate = 15f;     

    [Range(2, 10)]
    [SerializeField]
    float DISTANCE_DEAFULT = 3.2f;  

    public Transform target;  

    private float distance;


    private Vector3 playerTarget;

    private float lastRotate;

    // Use this for initialization
    void Start()
    {
        distance = DISTANCE_DEAFULT;    
        //target = GameObject.FindGameObjectWithTag("Player").transform;   
        playerTarget = new Vector3(target.position.x, target.position.y + target_offsety, target.position.z);   
        Quaternion cr = Quaternion.Euler(initRotate, transform.eulerAngles.y, 0);   
        //计算摄像机的位置
        Vector3 positon = playerTarget;
        positon += cr * Vector3.back * distance;
        transform.position = positon;
        transform.rotation = target.rotation;
        transform.LookAt(playerTarget);
    }
    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButton(1))
        {                                   
            transform.RotateAround(playerTarget, Vector3.up, Input.GetAxis("Mouse X") * 3f);
        }
    }
    
    void LateUpdate()
    {
        playerTarget = new Vector3(target.position.x, target.position.y + target_offsety, target.position.z);
        Quaternion cr = Quaternion.Euler(initRotate, transform.eulerAngles.y, 0);
        Vector3 positon = playerTarget + (cr * Vector3.back * distance);
        RaycastHit[] hits = Physics.RaycastAll(new Ray(playerTarget, (positon - playerTarget).normalized));
        distance = DISTANCE_DEAFULT;
        if (hits.Length > 0)
        {
            RaycastHit stand = new RaycastHit();
            float maxDistance = float.MaxValue;
            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider.isTrigger && hit.collider.name != target.name && hit.distance < maxDistance)
                {
                    stand = hit;
                    maxDistance = stand.distance;
                }
            }
            if (stand.collider != null)
            {
                string tag = stand.collider.gameObject.tag;
                
                distance = Vector3.Distance(stand.point, playerTarget);
                if (distance > DISTANCE_DEAFULT)
                {
                    distance = DISTANCE_DEAFULT;
                }
            }
        }
        positon = playerTarget + (cr * Vector3.back * distance);
        transform.position = Vector3.Lerp(transform.position, positon, 0.5f);  
        
        Debug.DrawRay(playerTarget, positon - playerTarget, Color.red);
        if (lastRotate != initRotate)
        {
            transform.LookAt(playerTarget);
            lastRotate = initRotate;
        }

        
        float fov = this.GetComponent<Camera>().fieldOfView;
        fov -= Input.GetAxis("Mouse ScrollWheel") * 10;
       
        this.GetComponent<Camera>().fieldOfView = Mathf.Clamp(fov, 30, 50);
    }
}
