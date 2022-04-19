using UnityEngine;
using System.Collections;

public class AppearEnemy : MonoBehaviour {
    public Transform prefab;
    public Transform role;
    private bool isAppear = false;
    // Use this for initialization
    void Start () {
        for (int i = 0; i < 5; i++)
        {
            Instantiate(prefab, new Vector3(transform.position.x + i * 2.0F, transform.position.y, transform.position.z + i * 2.0F), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update () {
        if(Vector3.Distance(role.position, transform.position) < 20 && false == isAppear)
        {
            isAppear = true;
            for (int i = 0; i < 5; i++)
            {
                Instantiate(prefab, new Vector3(transform.position.x + i * 2.0F, transform.position.y, transform.position.z + i * 2.0F), Quaternion.identity);
            }
        }
    }
}
