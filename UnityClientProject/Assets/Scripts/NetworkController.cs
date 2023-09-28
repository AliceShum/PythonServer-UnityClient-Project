using System;
using UnityEngine;
using MessageFormat;
using System.Collections.Generic;
using System.Text;

public class NetworkController : MonoBehaviour
{
    PythonClientController client; 

    #region 服务号的处理
    private int stype; //当前处理的服务号
    public delegate void ctype_handler(cmd_msg msg);
    protected Dictionary<int, ctype_handler> ctype_listeners;

    protected void InitStypeListeners()
    {
        stype = 5; //接收YOLO的结果
        EventManager.Instance.AddListener(stype, NetMessageHandler); //添加监听
        AddStypeListeners();
    }

    //处理不同的命令号
    protected void NetMessageHandler(cmd_msg msg)
    {
        if (stype != msg.stype)
            return;
        if (ctype_listeners == null)
            return;
        if (!ctype_listeners.ContainsKey(msg.ctype))
            return;
        ctype_listeners[msg.ctype](msg);
    }

    protected void AddStypeListeners()
    {
        ctype_listeners = new Dictionary<int, ctype_handler>
        {
            {1, ReceiveAcupointsResult},
            {4, ReceiveAcupointsError},
            {2, ReceiveAcupointsResultFromPythonServer},
        };
    }
    #endregion

    private void Start()
    {
        client = GetComponent<PythonClientController>(); 
        InitStypeListeners();
        EventManager.Instance.AddListener("发送背部截图", SendBackScreenshot);
        EventManager.Instance.AddListener("开始连接服务端", (eventmane, udata) => { client.connect_to_server(); });
    }

    //发送背部图片
    void SendBackScreenshot(string event_name = null, object udata = null)
    {
        //这个方法是通过unity服务端座位中转站的
        Texture2D pic = (Texture2D)udata;
        byte[] bytesArr = pic.EncodeToJPG(50);
        //下面的方法是把stype\ctype也加上(格式有问题)
        string strbaser64 = Convert.ToBase64String(bytesArr);
        client.send_json_cmd(13, 2, strbaser64);
    }

    //成功计算完图片
    void ReceiveAcupointsResult(cmd_msg msg)
    {
        COTR2DData data = JsonUtility.FromJson(Encoding.UTF8.GetString(msg.body), typeof(COTR2DData)) as COTR2DData; 
        UnityEngine.Debug.Log("收到COTR的json结果:" + Encoding.UTF8.GetString(msg.body));
        EventManager.Instance.DispatchEvent("收到穴位点位置", data);
    }

    //计算图片出错
    void ReceiveAcupointsError(cmd_msg msg)
    {
        InfoBoxMsg msg1 = new InfoBoxMsg();
        msg1.content_txt = "服务端计算出错！！！";
        msg1.confirm_btn_txt = "确定";
        msg1.confirm_btn_event_name = null;
        EventManager.Instance.DispatchEvent("显示提示框", msg1);
    }

    //收到从python服务端发送过来的yolo结果
    void ReceiveAcupointsResultFromPythonServer(cmd_msg msg)
    {
        string jsonCOTR2D = Encoding.UTF8.GetString(msg.body);
        UnityEngine.Debug.Log("收到yolo的json结果:" + jsonCOTR2D);
        COTR2DData data = JsonUtility.FromJson(jsonCOTR2D, typeof(COTR2DData)) as COTR2DData;
        EventManager.Instance.DispatchEvent("收到穴位点位置", data);
    }
}
