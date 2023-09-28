using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MoveAndZoomControllerBase : MonoBehaviour
{
    protected Transform targetTrans; //移动、缩放的目标
    protected RectTransform rectTrans;

    private void Start()
    {
        InitBtnEvents();
        EventManager.Instance.AddListener("背部穴位点生成完毕", ShowControlBtns);
        EventManager.Instance.AddListener("发送背部截图", HideControlBtns);
        HideControlBtns();
    }

    void SetControlTarget(Transform targetTrans)
    {
        this.targetTrans = targetTrans;
        this.rectTrans = targetTrans.GetComponent<RectTransform>();
    }

    private void ResetTargetTransform()
    {
        if (targetTrans == null) return;
        targetTrans.localPosition = Vector3.zero;
        targetTrans.localScale = Vector3.one;
        targetTrans = null;
    }

    #region 按钮
    void InitBtnEvents()
    {

    }

    void ShowControlBtns(string event_name = null, object udata = null)
    {
        SetControlTarget(this.transform);
    }
    void HideControlBtns(string event_name = null, object udata = null)
    {
        ResetTargetTransform();
    }

    #endregion

    #region 图片的缩放限定范围

    //图片local scale 最小不能低于1，最大不能超过5
    protected float GetControlledZoomPicScale(float scale)
    {
        float minScale = 1.0f;
        if (scale < minScale) return minScale;

        float maxScale = 5.0f;
        if (scale > maxScale) return maxScale;

        return scale;
    }

    //图片缩放时，图片下的点和文字跟着缩放 (local scale不能低于0.3, 不能超过1)
    protected void InstantiatedAcupointsZoomWithParent(float picScale)
    {
        float maxScale = 1.0f;
        float targetScale = maxScale / picScale;
        if (targetScale < 0.3f) targetScale = 0.3f;
        if (targetScale > maxScale) targetScale = maxScale;

        List<Transform> acupointList = ObjectPool.Instance.GetAllChildren(this.transform.GetChild(0));
        foreach (Transform child in acupointList)
        {
            child.localScale = new Vector3(targetScale, targetScale, targetScale);
        }
    }

    //图片移动时，不能超出画面
    //x: picWidth * picScale  * 0.5 - screenWidth * 0.5 = a；在 -a 到 +a 之间。  
    //y:  screenHeight* 0.5 - picHeight* picScale  * 0.5 = a；在 -a 到 +a 之间。  
    protected Vector2 ControlPicPosInsideScreen(Vector2 pos)
    {
        Vector2 picSize = rectTrans.sizeDelta;
        Vector2 picScale = rectTrans.localScale;
        float restrictedX = Mathf.Abs(picSize.x * picScale.x * 0.5f - Screen.width * 0.5f);
        float restrictedY = Mathf.Abs(Screen.height * 0.5f - picSize.y * picScale.y * 0.5f);

        if (pos.x < -restrictedX)
            pos.x = -restrictedX;
        else if (pos.x > restrictedX)
            pos.x = restrictedX;

        if (pos.y < -restrictedY)
            pos.y = -restrictedY;
        else if (pos.y > restrictedY)
            pos.y = restrictedY;

        return pos;
    }

    #endregion
}
