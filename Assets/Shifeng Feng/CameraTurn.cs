﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTurn : MonoBehaviour
{
    // 水平视角移动的敏感度
    public float sensitivityHor = 3f;
    // 垂直视角移动的敏感度
    public float sensitivityVer = 3f;
    // 视角向上移动的角度范围，该值越小范围越大
    public float upVer = -40;
    // 视角向下移动的角度范围，该值越大范围越大
    public float downVer = 45;
    // 垂直旋转角度
    private float rotVer;

    // 旋转的方向问题
    // x 表示绕 x 轴旋转，即 前上后 的角度
    // y 表示绕 y 轴旋转，即 左前后 的角度
    // y 表示绕 y 轴旋转，即 左前后 的角度

    // Start is called before the first frame update
    void Start()
    {
        // 初始化当前的垂直角度
        rotVer = transform.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        // 获取鼠标上下的移动位置
        float mouseVer = Input.GetAxis("Mouse Y");
        // 获取鼠标左右的移动位置
        float mouseHor = Input.GetAxis("Mouse X");
        // 鼠标往上移动，视角其实是往下移，所以要想达到视角也往上移的话，就要减去它
        rotVer -= mouseVer * sensitivityVer;
        // 限定上下移动的视角范围，即垂直方向不能360度旋转
        rotVer = Mathf.Clamp(rotVer, upVer, downVer);
        // 水平移动
        float rotHor = transform.localEulerAngles.y + mouseHor * sensitivityHor;
        // 设置视角的移动值
        transform.localEulerAngles = new Vector3(rotVer, rotHor, 0);
    }
}