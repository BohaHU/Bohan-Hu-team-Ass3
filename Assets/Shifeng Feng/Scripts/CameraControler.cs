﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControler : MonoBehaviour {
    public GameObject player;
    private Vector3 offset;
	// Use this for initialization
	void Start () {
        offset = transform.position - player.transform.position;//当前位置减去玩家的位置
	}
	
	// Update is called once per frame
	void LateUpdate () {
        transform.position = player.transform.position + offset;
	}
}
