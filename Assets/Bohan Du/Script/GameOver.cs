using UnityEngine;
using System.Collections;

public class GameOver : MonoBehaviour {

    public Texture2D GameOverT;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GameOverT);
        //得分渲染
        GUI.Label(new Rect(10, Screen.height - 30, 150, 50), "分数：" + RoleBulletController.score + "分");
    }
}
