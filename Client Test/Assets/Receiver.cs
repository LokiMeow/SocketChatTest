using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

public class Receiver : MonoBehaviour {

    /// <summary>
    /// 显示内容
    /// </summary>
    string messages;

    int port = 2333;
    string host = "127.0.0.1";
    IPAddress ipAddress ;
    IPEndPoint ipEndPoint;
    public static Socket clientSocketIns;
    byte[] recvBytes = new byte[1024];
	// Use this for initialization
	void Start () {

        try
        {

            //创建终结点EndPoint
            ipAddress = IPAddress.Parse(host);
            ipEndPoint = new IPEndPoint(ipAddress, port);   //把ip和端口转化为IPEndPoint的实例

            //创建Socket并连接到服务器
            clientSocketIns = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);   //  创建Socket
            messages += "Connecting...\n";
            clientSocketIns.Connect(ipEndPoint); //连接到服务器

            //向服务器发送信息
            //string sendStr = "Hello,this is a socket test";
            messages += "";

            //接受从服务器返回的信息
            string recvStr = "";
           
            int bytes;
            bytes = clientSocketIns.Receive(recvBytes, recvBytes.Length, 0);    //从服务器端接受返回信息
            recvStr += Encoding.UTF8.GetString(recvBytes, 0, bytes);
            if (recvStr != null || recvStr != string.Empty)
                messages += string.Format("{0}\n", recvStr);    //回显服务器的返回信息

            clientSocketIns.BeginReceive(recvBytes, 0, recvBytes.Length, 0, ReceiveAsync, clientSocketIns);
        }
        catch (ArgumentException e)
        {
            messages += "argumentNullException:" + e + "\n";
        }
        catch (SocketException e)
        {
            messages += "SocketException:" + e + "\n";
        }
	}

    public class StateObject
    {
        public Socket workSocket = null;
        public const int BUFFER_SIZE = 1024;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public StringBuilder sb = new StringBuilder();
    }

    private void ReceiveAsync(IAsyncResult ar)
    {
        try
        {
            //StateObject socketObj = (StateObject)ar.AsyncState;
            //Socket socket = socketObj.workSocket;

            StateObject socketObj = new StateObject();
            Socket socket = ar.AsyncState as Socket;
            socketObj.buffer = recvBytes;

            int read = socket.EndReceive(ar);

            if (read > 0)
            {
                socketObj.sb.Append(Encoding.UTF8.GetString(socketObj.buffer, 0, read));
                string strContent;
                strContent = socketObj.sb.ToString();
                messages += strContent+"\n";
                socket.BeginReceive(recvBytes, 0, StateObject.BUFFER_SIZE, 0,
                                         new AsyncCallback(ReceiveAsync), socket);
            }
            else
            {
                //if (socketObj.sb.Length > 1)
                //{
                //    ////All of the data has been read, so displays it to the console
                //    //string strContent;
                //    //strContent = socketObj.sb.ToString();
                //    //messages += strContent;
                //}
                //socket.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("AcceptAsync发生异常，异常信息:\n{0}", e.Message);
        }
    }
	
	// Update is called once per frame
	void Update () {
       
	}

    void OnGUI()
    {
        GUI.skin.label.fontSize = 13;
        GUI.Box(new Rect(5, 5, Screen.width - 10, Screen.height - Screen.height * 0.075f - 30), "");
        GUI.contentColor = Color.black;
        GUI.Label(new Rect(10.5f, 10.5f, Screen.width - 20, Screen.height - 20), messages);
        GUI.contentColor = Color.white;
        GUI.Label(new Rect(10, 10, Screen.width - 20, Screen.height - 20), messages);
    }

  
}
