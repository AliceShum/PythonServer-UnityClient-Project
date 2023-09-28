using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//弹出“已经截图”提示的控制脚本（没做多个提示信息的处理）
public class PromptPanel : MonoBehaviour
{
    private GameObject bg;
    private Text content;

    private Coroutine current;

    //private Queue<string> show_list = new Queue<string>();

    private void Start()
    {
        bg = transform.Find("bg").gameObject;
        content = bg.transform.Find("box/content").GetComponent<Text>();
        bg.SetActive(false);
        EventManager.Instance.AddListener("显示浮动提示", ShowPrompt);
    }

    void ShowPrompt(string eventname = null, object udata = null)
    {
        if (current != null)
        {
            StopCoroutine(current);
        }
        content.text = (string)udata;
        bg.SetActive(true);
        current = StartCoroutine("AutomaticallyHideLater");
    }

    void HideInfoBox()
    {
        bg.SetActive(false);
    }

    IEnumerator AutomaticallyHideLater()
    {
        yield return new WaitForSeconds(3f);
        HideInfoBox();
        current = null;
    }
}
