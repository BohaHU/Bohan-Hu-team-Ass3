using UnityEngine;
using System.Collections;

public class bullet : MonoBehaviour {

    void OnCollisionEnter(Collision collisionInfo)
    {
        Destroy(this.gameObject);
    }

    // Use this for initialization
    void Start () {
        //延时f秒
        float time = 0.4f;
        //f秒后销毁某个gameObject
        Destroy(this.gameObject, time);
    }
	
	// Update is called once per frame
	void Update () {

    }
}
