using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if UNITY_ANDROID
public class HandController : MoveAndZoomControllerBase, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    float Speed = 0.01f; //移动的速度

    Vector2 oldPosition1;  //记录缩放的位置
    Vector2 oldPosition2;  //记录缩放的位置

    //存储图片中心点与鼠标点击点的偏移量
    private Vector3 m_offset;

    #region 移动、缩放
    private void Update()
    {
        if (targetTrans == null) return;
        //Move();
        Zoom();
    }

    private bool CheckIsMove()
    {
        if (Input.touchCount == 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Moved)//触摸的一根手指滑动
            {
                return true;
            }
        }
        return false;

    }

    private void Move()
    {
        if (CheckIsMove())
        {
            float x = Input.GetAxis("Mouse X") * Speed;
            //获取y轴
            float y = Input.GetAxis("Mouse Y") * Speed;
            //targetTrans.Translate(-x, y, 0);//*Time.deltaTime
            Vector2 pos = ControlPicPosInsideScreen(rectTrans.localPosition + new Vector3(-x, y, 0));
            rectTrans.localPosition = pos;
        }
    }

    //==================================================================


    //拖拽位置
    public void OnDrag(PointerEventData eventData)
    {
        if (targetTrans == null) return;
        SetDraggedPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (targetTrans == null) return;
        SetDraggedPosition(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (targetTrans == null) return;
        //如果精准拖拽则进行计算偏移量操作
        m_offset = rectTrans.anchoredPosition - eventData.position;
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
        if (Input.touchCount > 1)
        {
            //两次触摸都有滑动
            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                return true;
            }
        }
        return false;
    }

    private void Zoom()
    {
        if (CheckIsZoom())
        {
            //获取第一、二次两次触摸的位置
            Vector2 tempPosition1 = Input.GetTouch(0).position;
            Vector2 tempPosition2 = Input.GetTouch(1).position;
            //放大
            if (IsEnlarge(oldPosition1, oldPosition2, tempPosition1, tempPosition2))
            {
                float oldScale = targetTrans.localScale.x;
                float newScale = oldScale * 1.025f;
                newScale = GetControlledZoomPicScale(newScale);
                targetTrans.localScale = new Vector3(newScale, newScale, newScale);
                InstantiatedAcupointsZoomWithParent(newScale);
                rectTrans.anchoredPosition = ControlPicPosInsideScreen(rectTrans.anchoredPosition);
            }
            else//缩小
            {
                float oldScale = targetTrans.localScale.x;
                float newScale = oldScale / 1.025f;
                newScale = GetControlledZoomPicScale(newScale);
                targetTrans.localScale = new Vector3(newScale, newScale, newScale);
                InstantiatedAcupointsZoomWithParent(newScale);
                rectTrans.anchoredPosition = ControlPicPosInsideScreen(rectTrans.anchoredPosition);

            }
            //备份上一次触摸点的位置，用于对比   
            oldPosition1 = tempPosition1;
            oldPosition2 = tempPosition2;
        }

    }

    // 比较两次的位置，大小，来进行放大还是缩小
    bool IsEnlarge(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        //函数传入上一次触摸两点的位置与本次触摸两点的位置计算出用户的手势   
        var leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        var leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));
        if (leng1 < leng2)
        {
            //放大手势   
            return true;
        }
        else
        {
            //缩小手势   
            return false;
        }
    }

    #endregion

}
#endif