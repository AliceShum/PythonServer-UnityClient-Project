using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//连接服务端弹窗的控制脚本
public class ConnectPanel : MonoBehaviour
{
    private GameObject bg;
    private InputField server_ip;
    private InputField server_port;
    private GameObject content_not_connect;
    private GameObject content_connecting;
    private GameObject content_connected;

    private bool isCheckingServerConnected = true; //是否在监听是否连上了服务端?

    private void Start()
    {
        bg = transform.Find("bg").gameObject;
        content_not_connect = bg.transform.Find("box/not_content").gameObject;
        content_connecting = bg.transform.Find("box/content_connecting").gameObject;
        content_connected = bg.transform.Find("box/content_connected").gameObject;
        server_ip = content_not_connect.transform.Find("server_ip").GetComponent<InputField>();
        server_port = content_not_connect.transform.Find("server_port").GetComponent<InputField>();
        content_not_connect.transform.Find("confirm_btn").GetComponent<Button>().onClick.AddListener(OnConfirmBtnClick);
        content_connected.transform.Find("close_btn").GetComponent<Button>().onClick.AddListener(HideConnectBox);
        content_not_connect.SetActive(true);
        content_connecting.SetActive(false);
        content_connected.SetActive(false);
    }

    void ShowConnectBox(string eventname = null, object udata = null)
    {
        bg.SetActive(true);
    }

    void HideConnectBox()
    {
        bg.SetActive(false);
    }

    //点击了连接按钮
    void OnConfirmBtnClick()
    {
        CommonParams.Instance.ServerIp = this.server_ip.text;
        CommonParams.Instance.ServerPort = int.Parse(this.server_port.text);
        content_not_connect.SetActive(false);
        content_connecting.SetActive(true);
        content_connected.SetActive(false);
        EventManager.Instance.DispatchEvent("开始连接服务端", null);
    }

    void ConnectToServerSuccessful(string eventname = null, object udata = null)
    {
        content_not_connect.SetActive(false);
        content_connecting.SetActive(false);
        content_connected.SetActive(true);
        StartCoroutine("ClosePanelAfterConnected");
    }

    IEnumerator ClosePanelAfterConnected()
    {
        yield return new WaitForSeconds(2f);
        HideConnectBox();
    }

    private void Update()
    {
        if (!isCheckingServerConnected)
            return;
        if (!CommonParams.Instance.isConnectedToServer)
            return;
        isCheckingServerConnected = false;
        ConnectToServerSuccessful();
    }
}
