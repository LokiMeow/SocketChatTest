﻿using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

public class Sender : MonoBehaviour {

    public static Sender SenderIns;

    /// <summary>
    /// 用户输入的内容
    /// </summary>
    string textEnter = "";

    void Awake()
    {
        SenderIns = this;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        GUI.skin.textField.fontSize = 15;
        GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
        
        textEnter = GUI.TextField(new Rect(Screen.width * 0.1f, Screen.height - Screen.height * 0.05f - 30, Screen.width * 0.6f, 30), textEnter);
        if (GUI.Button(new Rect(Screen.width * 0.75f, Screen.height - Screen.height * 0.05f - 30, Screen.width * 0.1f, 30), "Send") && textEnter!="")
        {
            string sendStr = textEnter;
            byte[] bSentAsBytes = Encoding.UTF8.GetBytes(sendStr);   //把字符串编码为字节

            Receiver.clientSocketIns.Send(bSentAsBytes, bSentAsBytes.Length, 0); //发送信息

            textEnter = "";
        }
    }
    

    void OnApplicationQuit()
    {
        if (Receiver.clientSocketIns != null)
        {
            Receiver.clientSocketIns.Send(new byte[0], 0, 0);
            Receiver.clientSocketIns.Close();
        }

    }
}