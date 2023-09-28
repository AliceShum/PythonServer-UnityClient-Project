using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MessageFormat;

//弹窗控制脚本
public class InfoBoxPanel : MonoBehaviour
{
    private GameObject bg;
    private Text title;
    private Text content;
    private Text confirm_btn_name;
    private string confirm_btn_event_name;

    private void Start()
    {
        bg = transform.Find("bg").gameObject;
        title = bg.transform.Find("box/top_bg/title").GetComponent<Text>();
        content = bg.transform.Find("box/content").GetComponent<Text>();
        confirm_btn_name = bg.transform.Find("box/confirm_btn/name").GetComponent<Text>();
        bg.transform.Find("box/confirm_btn").GetComponent<Button>().onClick.AddListener(OnConfirmBtnClick);
        bg.transform.Find("box/close_btn").GetComponent<Button>().onClick.AddListener(HideInfoBox);
        bg.SetActive(false);
        EventManager.Instance.AddListener("显示提示框", ShowInfoBox);
    }

    void ShowInfoBox(string eventname = null, object udata = null)
    {
        InfoBoxMsg msg = (InfoBoxMsg)udata;
        if (!string.IsNullOrEmpty(msg.title_txt))
        {
            title.text = msg.title_txt;
        }
        if (!string.IsNullOrEmpty(msg.content_txt))
        {
            content.text = msg.content_txt;
        }
        if (!string.IsNullOrEmpty(msg.confirm_btn_txt))
        {
            confirm_btn_name.text = msg.confirm_btn_txt;
        }
        if (!string.IsNullOrEmpty(msg.confirm_btn_event_name))
        {
            this.confirm_btn_event_name = msg.confirm_btn_event_name;
        }
        bg.SetActive(true);
    }

    void HideInfoBox()
    {
        bg.SetActive(false);
    }

    void OnConfirmBtnClick()
    {
        if (!string.IsNullOrEmpty(confirm_btn_event_name))
        {
            EventManager.Instance.DispatchEvent(this.confirm_btn_event_name, null);
        }
        this.confirm_btn_event_name = null;
        HideInfoBox();
    }
}
