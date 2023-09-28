using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MessageFormat;

public class PythonClientController : MonoBehaviour
{
    //===============================TCP相关=============================================

    private Socket client_socket = null;
    private bool is_connect = false;
    private Thread recv_thread = null;

    private const int RECV_LEN = 8192; //最大接收的字节流长度  
    private byte[] recv_buf = new byte[RECV_LEN]; //收到的字节流（多次）
    private int recved; //已经收到的字节长度（多次）
    private byte[] long_pkg = null;
    private int long_pkg_size = 0;

    public State Connect_State
    {
        set
        {
            if (value == State.Disconnect) { CommonParams.Instance.isConnectedToServer = false; }
            if (value == State.Connecting) { CommonParams.Instance.isConnectedToServer = false; }
            if (value == State.Connected) { CommonParams.Instance.isConnectedToServer = true; }
            connect_state = value;
        }
        get { return this.connect_state; }
    }
    private State connect_state;

    private Queue<cmd_msg> net_events = new Queue<cmd_msg>(); //主线程记录要处理的cmd_msg

    //===============================心跳包相关=============================================
    public bool isUsePing = false; //是否启用心跳
    public int pingInterval = 30; //心跳间隔时间
    public float lastPingTime = 0; //上一次发送Ping时间
    public float lastPongTime = 0; //上一次接收Pong时间

    private void Start()
    {
        this.Connect_State = State.Disconnect;
        Debug.Log(Connect_State + " --------connect state----------");
    }

    void OnDestroy()
    {
        this.close();
    }

    void OnApplicaitonQuit()
    {
        this.close();
    }

    void Update()
    {
        if (this.Connect_State == State.Disconnect)
        {
            //this.connect_to_server();
            return;
        }

        lock (this.net_events)
        {
            while (this.net_events.Count > 0)
            {
                cmd_msg msg = this.net_events.Dequeue();
                EventManager.Instance.DispatchEvent(msg);
            }
        }

        PingUpdate();
    }

    void on_conntect_timeout()
    {
    }

    void on_connect_error(string err)
    {
        this.Connect_State = State.Disconnect;
    }

    public void connect_to_server(string event_name = null, object udata = null)
    {
        if (this.Connect_State != State.Disconnect)
        {
            return;
        }

        //上次发送Ping时间
        lastPingTime = Time.time;
        //上次收到Pong时间
        lastPongTime = Time.time;

        this.Connect_State = State.Connecting;

        try
        {
            this.client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse(CommonParams.Instance.ServerIp);
            IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, CommonParams.Instance.ServerPort);

            this.client_socket.BeginConnect(ipEndpoint, new AsyncCallback(this.on_connected), this.client_socket);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
            this.on_connect_error(e.ToString());
        }
    }

    void on_recv_cmd(byte[] data, int start, int data_len)
    {
        cmd_msg msg;
        proto_man.unpack_cmd_msg(data, start, data_len, out msg);
        if (msg != null)
        {
            lock (this.net_events)
            { // recv thread
                this.net_events.Enqueue(msg);
            }
        }
    }

    void on_recv_tcp_data()
    {
        byte[] pkg_data = (this.long_pkg != null) ? this.long_pkg : this.recv_buf;
        while (this.recved > 0)
        {
            int pkg_size = 0;
            int head_size = 0;

            if (!tcp_packer.read_header_uint(pkg_data, this.recved, out pkg_size, out head_size)) //解析不出来
            {
                break;
            }

            if (this.recved < pkg_size) //没接收完整
            {
                break;
            }

            int raw_data_start = head_size;
            int raw_data_len = pkg_size - head_size;

            on_recv_cmd(pkg_data, raw_data_start, raw_data_len);

            if (this.recved > pkg_size)
            {
                this.recv_buf = new byte[RECV_LEN];
                Array.Copy(pkg_data, pkg_size, this.recv_buf, 0, this.recved - pkg_size);
                pkg_data = this.recv_buf;
            }

            this.recved -= pkg_size;

            if (this.recved == 0 && this.long_pkg != null)
            {
                this.long_pkg = null;
                this.long_pkg_size = 0;
            }
        }
    }

    void thread_recv_worker()
    {
        if (this.is_connect == false)
        {
            return;
        }

        while (true)
        {
            if (!this.client_socket.Connected)
            {
                break;
            }

            try
            {
                int recv_len = 0;
                if (this.recved < RECV_LEN)
                {
                    recv_len = this.client_socket.Receive(this.recv_buf, this.recved, RECV_LEN - this.recved, SocketFlags.None);
                    UnityEngine.Debug.Log("接收到的数据长度：" + recv_len);
                }
                else //上面放满的，放去long_pkg
                {
                    if (this.long_pkg == null)
                    {
                        int pkg_size;
                        int head_size;
                        tcp_packer.read_header_uint(this.recv_buf, this.recved, out pkg_size, out head_size);
                        this.long_pkg_size = pkg_size;
                        this.long_pkg = new byte[pkg_size];
                        Array.Copy(this.recv_buf, 0, this.long_pkg, 0, this.recved);
                    }
                    recv_len = this.client_socket.Receive(this.long_pkg, this.recved, this.long_pkg_size - this.recved, SocketFlags.None);
                }

                if (recv_len > 0)
                {
                    this.recved += recv_len;
                    this.on_recv_tcp_data();
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
                //关闭连接
                if (this.client_socket != null && this.client_socket.Connected)
                {
                    if (this.client_socket.Connected)
                    {
                        this.client_socket.Disconnect(true);
                    }
                    // this.client_socket.Shutdown(SocketShutdown.Both);
                    this.client_socket.Close();
                }
                this.client_socket = null;
                this.is_connect = false;
                this.Connect_State = State.Disconnect;
                break;
            }
        }

        Debug.Log("exit recv thread");
    }

    void on_connected(IAsyncResult iar)
    {
        try
        {
            Socket client = (Socket)iar.AsyncState;
            client.EndConnect(iar);

            this.Connect_State = State.Connected;

            this.is_connect = true;
            this.recv_thread = new Thread(new ThreadStart(this.thread_recv_worker));
            this.recv_thread.Start();

            AfterConnected();

            Debug.LogError("connect to server success" + CommonParams.Instance.ServerIp + ":" + CommonParams.Instance.ServerPort + "!");
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
            this.on_connect_error(e.ToString());
            this.is_connect = false;
        }
    }

    void close()
    {
        if (!this.is_connect)
        {
            return;
        }

        this.is_connect = false;
        this.Connect_State = State.Disconnect;

        // abort recv thread
        if (this.recv_thread != null)
        {
            this.recv_thread.Interrupt();
            this.recv_thread.Abort();
            this.recv_thread = null;
        }
        // end

        if (this.client_socket != null && this.client_socket.Connected)
        {
            this.client_socket.Close();
            this.client_socket = null;
        }
    }

    private void on_send_data(IAsyncResult iar)
    {
        try
        {
            Socket client = (Socket)iar.AsyncState;
            client.EndSend(iar);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void send_json_cmd(int stype, int ctype, string json_body)
    {
        byte[] cmd_data = proto_man.pack_json_cmd(stype, ctype, json_body);
        if (cmd_data == null)
        {
            return;
        }

        byte[] tcp_pkg = tcp_packer.packUint(cmd_data);
        Debug.Log("发送的长度：" + tcp_pkg.Length);

        // end 
        this.client_socket.BeginSend(tcp_pkg, 0, tcp_pkg.Length, SocketFlags.None, new AsyncCallback(this.on_send_data), this.client_socket);
        // end 
    }

    public void send_buffer_cmd(int stype, int ctype, byte[] body)
    {
        byte[] cmd_data = proto_man.pack_buffer_cmd(stype, ctype, body);
        if (cmd_data == null)
        {
            return;
        }
        byte[] tcp_pkg = tcp_packer.packUint(cmd_data);
        Debug.Log("发送的长度：" + tcp_pkg.Length);
        // end 
        this.client_socket.BeginSend(tcp_pkg, 0, tcp_pkg.Length, SocketFlags.None, new AsyncCallback(this.on_send_data), this.client_socket);
        // end 
    }

    public void send_buffer_straight(byte[] body)
    {
        this.client_socket.BeginSend(body, 0, body.Length, SocketFlags.None, new AsyncCallback(this.on_send_data), this.client_socket);
    }

    /// <summary>
    /// 发送Ping协议
    /// </summary>
    private void PingUpdate()
    {
        if (!isUsePing)
        {
            return;
        }
        if (!this.is_connect)
        {
            return;
        }
        //发送Ping. TODO!
        if (Time.time - lastPingTime > pingInterval)
        {
            send_json_cmd(2, 1, "Msg Ping!");
            lastPingTime = Time.time;
        }
        if (Time.time - lastPongTime > pingInterval * 4) //连接不上服务端，不做特定反应，一直尝试连接
        {
            //this.close();
        }
    }

    //刚连接上服务端之后的事件
    void AfterConnected()
    {
        /*ClientInfo info = new ClientInfo();
        string[] s = client_socket.LocalEndPoint.ToString().Split(":");
        info.ipAddr = s[0];
        info.port = int.Parse(s[1]);
        info.platform = CommonParams.Instance.platform;
        string json = JsonUtility.ToJson(info); // LitJson.JsonMapper.ToJson(info)
        send_json_cmd(4, 1, json);*/
    }

}
