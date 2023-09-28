using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
public class MouseController : MoveAndZoomControllerBase, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("是否精准拖拽")]
    public bool m_isPrecision = true;

    //存储图片中心点与鼠标点击点的偏移量
    private Vector3 m_offset;

    #region 移动、缩放
    private void Update()
    {
        if (targetTrans == null) return;
        Zoom();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetTrans == null) return;
        SetDraggedPosition(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (targetTrans == null) return;
        //如果精准拖拽则进行计算偏移量操作
        if (m_isPrecision)
        {
            m_offset = rectTrans.anchoredPosition - eventData.position;
        }
        //否则 默认偏移量为0
        else
        {
            m_offset = Vector3.zero;
        }
        SetDraggedPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (targetTrans == null) return;
        SetDraggedPosition(eventData);
    }

    // 设置图片位置方法
    private void SetDraggedPosition(PointerEventData eventData)
    {
        Vector2 pos = ControlPicPosInsideScreen(eventData.position + new Vector2(m_offset.x, m_offset.y));
        rectTrans.anchoredPosition = pos;
    }

    private bool CheckIsZoom()
    {
        if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0)
            return true;
        return false;
    }

    private void Zoom()
    {
        if (CheckIsZoom())
        {
            float scale = targetTrans.localScale.x;
            scale += Input.GetAxis("Mouse ScrollWheel");
            scale = GetControlledZoomPicScale(scale);
            targetTrans.localScale = new Vector3(scale, scale, scale);
            InstantiatedAcupointsZoomWithParent(scale);
            rectTrans.anchoredPosition = ControlPicPosInsideScreen(rectTrans.anchoredPosition);
        }
    }

    #endregion

}
#endif
