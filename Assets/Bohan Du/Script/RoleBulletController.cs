using UnityEngine;
using System.Collections;

public class RoleBulletController : MonoBehaviour {

    //初始子弹个数 
    private int bullets = 100;

    //游戏分数
    static public int score = 0;

    //受重力影响的子弹
    public Rigidbody bullet;

    //发射位置
    private GameObject firePoint;

    //准星贴图
    public Texture2D texture;

    //初始总血量
    private int blood = 100;

    //是否显示血量
    private bool isShowBlood = true;

    //血量背景贴图
    public Texture2D bloodBgTexture;

    //血量贴图
    public Texture2D bloodTexture;

    //全屏红色贴图
    public Texture2D AllRed;

    //全屏红色透明度
    private float howAlpha;

    //枪模型
    public GameObject Gun;

    // Use this for initialization
    void Start () {
        firePoint = GameObject.Find("firePoint");
    }
	
	// Update is called once per frame
	void Update () {
        //生成一道射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //射线碰撞
        RaycastHit hit;

        //点击鼠标左键
        if (Input.GetMouseButtonDown(0) && bullets > 0)
            {
            //剩余子弹个数
            bullets--;
            //通过射线获得目标点
            //Returns a ray going from camera through a screen point.
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Returns a point at distance units along the ray.
            Vector3 target = ray.GetPoint(20);
            //修改发射起点的朝向
            //firePoint.transform.LookAt(target);
            //实例化子弹
            Rigidbody clone = (Rigidbody)Instantiate(bullet, firePoint.transform.position, firePoint.transform.rotation);
            //初始化子弹的方向速度
            clone.velocity = (target - firePoint.transform.position) * 3;
            //播放子弹音频
            Gun.SendMessage("shootAudio");

            //如果射线碰到物体的话 1 << 9 打开第九层
            if (Physics.Raycast(ray, out hit, 100, 1 << 9))
            {
                //击中控制台反馈
                Debug.Log(hit.normal);
                //销毁碰撞器
                Destroy(hit.collider);
                //加分
                score++;
                //Destroy(hit.transform.gameObject);
                hit.transform.gameObject.GetComponent<EnemyController>().dead();
            }
        }
        //修改发射起点的朝向
        firePoint.transform.LookAt(Camera.main.ScreenPointToRay(Input.mousePosition).GetPoint(20));
        //胜利检测
        if(score >= 50)
        {
            Debug.Log("You Win!");
            Application.LoadLevel("YouWin");
        }

        //死亡检测
        if(blood <= 0 || bullets <= 0)
        {
            Debug.Log("Game Over!");
            Application.LoadLevel("GameOver");

        }
    }

    //加血
    public void addBlood()
    {
        if(blood > 50)
        {
            blood = 100;
        }
        else
        {
            blood += 50;
        }
    }

    //掉血
    public void reduceBlood(int attackType)
    {
        switch (attackType)
        {
            case 6:
                blood -= 6;
                break;
            case 8:
                blood -= 8;
                break;
            case 10:
                blood -= 10;
                break;
            default:
                Debug.Log("blood error");
                break;
        }
    }

    void OnGUI()
    {
        //子弹数+得分渲染
        GUI.Label(new Rect(10, Screen.height - 30, 150, 50), "bullet x" + bullets + "   score "+ score);
        //准星贴图
        Rect rect = new Rect(Input.mousePosition.x - (texture.width / 2),
        Screen.height - Input.mousePosition.y - (texture.height / 2),
        texture.width, texture.height);
        GUI.DrawTexture(rect, texture);
        //血条贴图
        GUI.DrawTexture(new Rect(0, 0, bloodBgTexture.width, bloodBgTexture.height), bloodBgTexture);
        GUI.DrawTexture(new Rect(0, 0, bloodTexture.width * (blood * 0.01f), bloodTexture.height), bloodTexture);
        //血量全屏贴图
        Color alpha = GUI.color;
        howAlpha = (100.0f - blood) / 120.0f;
        if(howAlpha < 0.42)
        {
            howAlpha = 0;
        }
        alpha.a = howAlpha;
        GUI.color = alpha;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), AllRed);
    }
}
