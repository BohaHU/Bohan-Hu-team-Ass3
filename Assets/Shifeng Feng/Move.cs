using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 表示一定需要这个控件
[RequireComponent(typeof(CharacterController))]
public class Move : MonoBehaviour
{
    // 获取摄像机对象的位置信息，[SerializeField] 类似于 java 的构造方法的方法参数
    [SerializeField] private Transform target;
    // 跳起来的速度
    public float jumpSpeed = 15.0f;
    // 重力
    public float gravity = -9.8f;
    // 最终垂直速度
    public float endsVelocity = -10.0f;
    // 在地面时的垂直速度
    public float minFall = -1.5f;
    // 跑起来的速度
    public float runSpeed = 10;
    // 走路的速度
    public float walkSpeed = 4;
    // 垂直速度
    private float verSpeed;

    // 移动的速度
    private float moveSpeed;
    // 用于存储当前的角色控件
    private CharacterController character;

    public GameObject successPanel;

    // 在被加载时执行
    void Start()
    {
        // 初始化
        character = GetComponent<CharacterController>();
        verSpeed = minFall;
        moveSpeed = walkSpeed;
    }
    // 每更新一帧时执行
    void Update()
    {
        // 用于存储移动信息
        Vector3 movement = Vector3.zero;
        // 获取左右方向的移动信息
        float horspeed = Input.GetAxis("Horizontal");
        // 获取前后方向的移动信息
        float verspeed = Input.GetAxis("Vertical");
        // 当发生了移动才执行
        if (horspeed != 0 || verspeed != 0)
        {
            // 设置左右位置
            movement.x = horspeed * moveSpeed;
            // 设置前后的位置
            movement.z = verspeed * moveSpeed;
            // 设置斜着走的最大速度更水平垂直走的速度一样
            movement = Vector3.ClampMagnitude(movement, moveSpeed);
            // 将移动的信息转化为以摄像机为全局坐标的位置，即保证你向前走一定是摄像机的视角方向
            movement = target.TransformDirection(movement);
        }
        // 当按下左 shift 是跟换速度
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = runSpeed;
        }
        else
        {
            moveSpeed = walkSpeed;
        }
        // 角色控件自带的一个方法，用于检测是否在地面
        if (character.isGrounded)
        {
            // 按了空格键则给垂直方向施加一个速度
            if (Input.GetButtonDown("Jump"))
            {
                verSpeed = jumpSpeed;
            }
            else
            {
                verSpeed = minFall;
            }
        }
        else
        {
            // 若已经跳起来了则将垂直方向的速度递减降低，来达到一个 下上下 的一个效果
            // Time.deltaTime 表示为每秒的刷新频率的倒数，用来控制每台电脑的移动速度都是一样的
            verSpeed += gravity * 3 * Time.deltaTime;
            // 限制最大坠落速度
            if (verSpeed < endsVelocity)
            {
                verSpeed = endsVelocity;
            }
        }
        // 给移动一个垂直速度
        movement.y = verSpeed;
        // 控制速度
        movement *= Time.deltaTime;
        // 角色控件自带的一个方法，若用 transform.Translate() 的话会无视碰撞器
        character.Move(movement);
    }
    public Text txtScore;
    public int score;
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name=="red")
        {
            score--;
        }
        if (other.gameObject.name == "green")
        {
            score++;
            if (score>=5)
            {
                successPanel.SetActive(true);
            }
        }
        other.gameObject.SetActive(false);
        txtScore.text = "得分:"+score.ToString();
    }
    public void restartGame()
    {
        SceneManager.LoadScene(1);
    }
}