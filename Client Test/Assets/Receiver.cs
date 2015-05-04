using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

public class Receiver : MonoBehaviour
{

    /// <summary>
    /// 显示内容
    /// </summary>
    string messages;

    int port = 3333;
    string host = "192.168.88.103";
    IPAddress ipAddress;
    IPEndPoint ipEndPoint;
    string userName = string.Empty;
    public static Socket clientSocketIns;
    byte[] recvBytes = new byte[1024];

    public float fontScale;

    public static bool connectServer;
    // Use this for initialization
    void Start()
    {
        connectServer = true;
        port = PlayerPrefs.GetInt("port");
        host = PlayerPrefs.GetString("host");
        messages = "";
    }
    void ConnectServer()
    {
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
            byte[] bSentAsBytes = Encoding.UTF8.GetBytes(userName);   //把字符串编码为字节

            clientSocketIns.Send(bSentAsBytes, bSentAsBytes.Length, 0); //发送信息

            //接受从服务器返回的信息
            string recvStr = "";

            int bytes;
            bytes = clientSocketIns.Receive(recvBytes, recvBytes.Length, 0);    //从服务器端接受返回信息

            connectServer = false;

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
                messages += strContent + "\n";
                socket.BeginReceive(recvBytes, 0, StateObject.BUFFER_SIZE, 0,
                                         new AsyncCallback(ReceiveAsync), socket);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("AcceptAsync发生异常，异常信息:\n{0}", e.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector2 scrollPosition = Vector2.zero;

    void OnGUI()
    {
        GUI.skin.label.fontSize = (int)(Screen.width * fontScale);
        GUI.Box(new Rect(Screen.width * 0.025f, Screen.width * 0.025f, Screen.width - Screen.width * 0.025f * 2, Screen.height - Screen.height * 0.05f - GUI.skin.textField.fontSize * 2 - Screen.width * 0.025f * 2), "");

        GUILayout.BeginArea(new Rect(Screen.width * 0.033f, Screen.width * 0.033f, Screen.width * 2, Screen.height - Screen.height * 0.05f - GUI.skin.textField.fontSize * 2 - Screen.width * 0.03f * 2));
        GUILayout.BeginScrollView(scrollPosition);
        GUI.contentColor = Color.black;
        GUILayout.Label(messages, GUILayout.Width(Screen.width - Screen.width * 0.06f * 2));
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width * 0.03f, Screen.width * 0.03f, Screen.width - Screen.width * 0.03f * 2, Screen.height - Screen.height * 0.05f - GUI.skin.textField.fontSize * 2 - Screen.width * 0.03f * 2));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUI.contentColor = Color.white;
        GUILayout.Label(messages, GUILayout.Width(Screen.width - Screen.width * 0.06f * 2));
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (connectServer)
        {
            GUI.skin.textField.fontSize = (int)(Screen.width * fontScale);
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
            GUI.skin.button.fontSize = GUI.skin.textField.fontSize;
            GUI.BeginGroup(new Rect(Screen.width * 0.15f, (Screen.height - Screen.width * 0.6f) * 0.5f, Screen.width * 0.7f, Screen.width * 0.6f));
            GUI.Box(new Rect(0, 0, Screen.width * 0.7f, GUI.skin.textField.fontSize * 8), "");
            GUILayout.BeginArea(new Rect(5, 5, Screen.width * 0.7f, GUI.skin.textField.fontSize * 8));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width * 0.65f));
            GUILayout.Label("服务器IP:");
            host = GUILayout.TextField(host, GUILayout.Width(Screen.width * 0.4f));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width * 0.65f));
            GUILayout.Label("端口:    ");
            var portText = GUILayout.TextField(port.ToString(), GUILayout.Width(Screen.width * 0.4f));
            int.TryParse(portText, out port);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width * 0.65f));
            GUILayout.Label("用户名称:");
            userName = GUILayout.TextField(userName, GUILayout.Width(Screen.width * 0.4f));
            GUILayout.EndHorizontal();
            if (GUILayout.Button("登录", GUILayout.Width(Screen.width * 0.65f)))
            {
                ConnectServer();
                PlayerPrefs.SetInt("port", port);
                PlayerPrefs.SetString("host", host);
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
            GUI.EndGroup();
        }
    }

    public void ShowLastMessage()
    {
        scrollPosition.y = int.MaxValue;
    }

    public void OnAccidentlyDisconnect()
    {
        messages += "\n已与服务器断开连接！\n";
        connectServer = true;
    }
}
