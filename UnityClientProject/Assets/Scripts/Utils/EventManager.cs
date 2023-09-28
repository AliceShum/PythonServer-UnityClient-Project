using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessageFormat;

public class EventManager : Singleton<EventManager>
{
    #region 下面的方法用于在接收到服务端消息时，传播需要执行的方法
    public delegate void net_message_handler(cmd_msg msg);

    private Dictionary<int, net_message_handler> dic = new Dictionary<int, net_message_handler>();

    public void init()
    {

    }

    public void AddListener(int stype, net_message_handler h)
    {
        if (this.dic.ContainsKey(stype))
        {
            this.dic[stype] += h;
        }
        else
        {
            this.dic.Add(stype, h);
        }
    }

    //不同脚本自己定义不同的处理方法，一般不取消
    public void RemoveListener(int stype, net_message_handler h)
    {
        if (!this.dic.ContainsKey(stype))
        {
            return;
        }

        this.dic[stype] -= h;

        if (this.dic[stype] == null)
        {
            this.dic.Remove(stype);
        }
    }

    public void DispatchEvent(cmd_msg msg)
    {
        if (!this.dic.ContainsKey(msg.stype))
        {
            return;
        }

        this.dic[msg.stype](msg);
    }

    #endregion

    #region 下面的方法处理在本机范围内，不同脚本之间传输的事件
    public delegate void event_handler(string event_name = null, object udata = null);

    private Dictionary<string, event_handler> eventDic = new Dictionary<string, event_handler>();

    public void AddListener(string event_name, event_handler h)
    {
        Debug.Log("接收事件：" + event_name);
        if (this.eventDic.ContainsKey(event_name))
        {
            this.eventDic[event_name] += h;
        }
        else
        {
            this.eventDic.Add(event_name, h);
        }
    }

    public void RemoveListener(string event_name, event_handler h)
    {
        if (!this.eventDic.ContainsKey(event_name))
        {
            return;
        }

        this.eventDic[event_name] -= h;

        if (this.eventDic[event_name] == null)
        {
            this.eventDic.Remove(event_name);
        }
    }

    public void DispatchEvent(string event_name, object udata)
    {
        Debug.Log("发送事件：" + event_name);
        if (!this.eventDic.ContainsKey(event_name))
        {
            return;
        }

        this.eventDic[event_name](event_name, udata);
    }
    #endregion
}
