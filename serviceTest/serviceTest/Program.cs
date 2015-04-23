using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//添加Socket类
using System.Net;
using System.Net.Sockets;
using System.Data;

namespace SockeConsoleServer
{
    //class Program
    //{
        
    //    static void Main(string[] args)
    //    {
    //        List<string> ChatList = new List<string>();
    //        Dictionary<Socket, string> ClientList = new Dictionary<Socket, string>();

    //        int port = 2000;
    //        string host = "127.0.0.1";

    //        //创建终结点
    //        IPAddress ipAddress = IPAddress.Parse(host);
    //        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);

    //        //创建Socket并开始监听

    //        Socket serviseSocketIns = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //创建一个Socket对象，如果用UDP协议，则要用SocketTyype.Dgram类型的套接字
    //        serviseSocketIns.Bind(ipEndPoint);    //绑定EndPoint对象(2000端口和ip地址)
    //        serviseSocketIns.Listen(0);    //开始监听

    //        Console.WriteLine("等待客户端连接");

    //        //接受到Client连接，为此连接建立新的Socket，并接受消息
    //        Socket temp = serviseSocketIns.Accept();   //为新建立的连接创建新的Socket
    //        Console.WriteLine("客户端接入...");
    //        string recvStr = "";
    //        byte[] recvBytes = new byte[1024];
    //        int recvBytesLength;
    //        recvBytesLength = temp.Receive(recvBytes, recvBytes.Length, 0); //从客户端接受消息
    //        //recvStr += Encoding.ASCII.GetString(recvBytes, 0, recvBytesLength);
    //        string userName = Encoding.UTF8.GetString(recvBytes, 0, recvBytesLength);
    //        //给Client端返回信息
    //        Console.WriteLine("用户“{0}”已接入服务器。", userName);    //把客户端传来的信息显示出来

    //        ClientList.Add(temp, userName);

    //        string sendStr = "【系统提示】:" + userName + "，您已与服务器连接！";
    //        byte[] bs = Encoding.UTF8.GetBytes(sendStr);
    //        temp.Send(bs, bs.Length, 0);  //返回信息给客户端

    //        do
    //        {
    //            recvBytesLength = temp.Receive(recvBytes, recvBytes.Length, 0); //从客户端接受消息
    //            recvStr = Encoding.ASCII.GetString(recvBytes, 0, recvBytesLength);
    //            var newLine = string.Format("{0}:{1}", userName, recvStr);
    //            var newLineBytes=Encoding.UTF8.GetBytes(newLine);
    //            //ChatList.Add(newLine);
    //            foreach(var client in ClientList)
    //            {
    //                client.Key.Send(newLineBytes, newLineBytes.Length, 0);
    //            }
    //        }
    //        while (recvStr != "");

    //        temp.Close();
            
    //        Console.ReadLine();
    //        serviseSocketIns.Close();

    //    }

    //}



    public class AppTCPServer
    {
        public Socket socketMain = new Socket(AddressFamily.InterNetwork,
                                                        SocketType.Stream,
                                                        ProtocolType.Tcp);
        public AppTCPServer(string localIP, int port)
        {
            
            EndPoint localEP = new IPEndPoint(IPAddress.Parse(localIP), port);
            socketMain.Bind(localEP);
            socketMain.Listen(100);
            socketMain.BeginAccept(AcceptAsync, socketMain);
        }

        byte[] recvBytes = new byte[1024];

        private void AcceptAsync(IAsyncResult ar)
        {
            Socket socket = ar.AsyncState as Socket;
            try
            {
                Socket client = socket.EndAccept(ar);
                Console.WriteLine("客户端连接成功！客户端：{0}", client.RemoteEndPoint.ToString());
                if (OnConnected != null)
                {
                    OnConnected(this, new ClientConnectedEventArgs(client));
                }

                
                client.BeginReceive(recvBytes, 0, recvBytes.Length, 0, ReceiveAsync, client);
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptAsync发生异常，异常信息:\n{0}", e.Message);
            }
            finally
            {
                //继续异步Accept
                socket.BeginAccept(AcceptAsync, socket);
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
            //StateObject socketObj = (StateObject)ar.AsyncState;
            //Socket socket = socketObj.workSocket;
            StateObject socketObj = new StateObject();
            Socket socket = ar.AsyncState as Socket;
            socketObj.buffer = recvBytes;
            string strContent=null;
            try
            {
                
                
                int read = socket.EndReceive(ar);

                if (read > 0)
                {
                    socketObj.sb.Append(Encoding.UTF8.GetString(socketObj.buffer, 0, read));
                    
                    if(!Program.ClientList.Keys.Any(user=>user==socket))
                    {
                        Program.ClientList.Add(socket, socketObj.sb.ToString());
                        strContent = string.Format("用户【{0}】上线。", socketObj.sb.ToString());
                    }
                    else
                        strContent = string.Format("{0}：{1}", Program.ClientList[socket] , socketObj.sb.ToString());
                    
                }
                else
                {
                    if(Program.ClientList.Keys.Any(user=>user==socket))
                        strContent = string.Format("用户【{0}】已登出系统。", Program.ClientList[socket]);
                    socket.Close();
                }
                if(strContent!=null)
                {
                    var sendMsg = Encoding.UTF8.GetBytes(strContent);
                    var newUserList = new Dictionary<Socket, string>();
                    foreach (var user in Program.ClientList)
                    {
                        if (user.Key != null && user.Key.Connected)
                        {
                            newUserList.Add(user.Key, user.Value);
                            user.Key.Send(sendMsg, sendMsg.Length, 0);
                        }
                        else
                            user.Key.Close();
                    }
                    Program.ClientList = newUserList;
                }

                if (socket != null && socket.Connected)
                    socket.BeginReceive(recvBytes, 0, StateObject.BUFFER_SIZE, 0,
                                              new AsyncCallback(ReceiveAsync), socket);
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptAsync发生异常，异常信息:\n{0}", e.Message);
            }
        }

      
        //客户端连接后的事件 OnConnect
        public event EventHandler<ClientConnectedEventArgs> OnConnected;

    }

    /// <summary>
    /// 事件参数：接收客户端连接后的事件参数
    /// </summary>
    public class ClientConnectedEventArgs : EventArgs
    {
        public ClientConnectedEventArgs(Socket clientSocket)
        {
            ClientSocket = clientSocket;
        }
        public Socket ClientSocket { get; private set; }
    }

    class Program
    {
        public static List<string> ChatList = new List<string>();
        public static Dictionary<Socket, string> ClientList = new Dictionary<Socket, string>();
        static void Main(string[] args)
        {
            AppTCPServer server = new AppTCPServer("127.0.0.1", 2333);
            server.OnConnected += server_OnConnected;
            
            Console.WriteLine("按任意键结束程序……");
            Console.ReadKey();
            foreach(var user in ClientList.Keys)
            {
                user.Close();
            }
            server.socketMain.Close();
        }

        static void server_OnConnected(object sender, ClientConnectedEventArgs e)
        {
            Socket client = e.ClientSocket;
            string hello = "你已连接上服务器，请输入用户名：\n";
            client.Send(Encoding.UTF8.GetBytes(hello));
        }
    }
}