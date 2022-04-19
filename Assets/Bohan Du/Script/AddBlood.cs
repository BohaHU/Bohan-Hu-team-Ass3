using UnityEngine;
using System.Collections;

public class AddBlood : MonoBehaviour {
    //主角
    public Transform role;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance(role.position, transform.position) < 2)
        {
            role.SendMessage("addBlood");
            Destroy(this.gameObject);
        }
    }
}
