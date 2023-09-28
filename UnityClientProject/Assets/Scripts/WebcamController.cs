using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebcamController : MonoBehaviour
{
    //当前相机索引
    private int webcam_index = 0;

    //当前运行的相机
    private WebCamTexture current_webCam;
    private int webcam_width; //相机需要的宽度
    private int webcam_height;

    private int actual_tex_width;
    private int actual_tex_height;

    private RawImage webcam_img; //动态相机画面
    private RawImage screenshot; //截取的相机画面
    private RawImage webcam_frame; //相机视频流画面
    private RawImage import_pic; //导入的图片
    private Transform screenshot_reddot_parent; //screenshot下的红点父节点
    private GameObject screenshot_reddot; //screenshot下的红点demo
    private Transform webcam_frame_reddot_parent; //webcam_frame下的红点父节点
    private GameObject webcam_frame_reddot; //webcam_frame下的红点demo
    private Transform import_pic_reddot_parent;  //import_pic下的红点父节点
    private GameObject import_pic_reddot; //import_pic下的红点demo
    private List<GameObject> reddot_list = new List<GameObject>();

    void Start()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        Transform webcam_area = canvas.transform.Find("bg/webcam_area");
        webcam_img = webcam_area.Find("webcam_img").GetComponent<RawImage>();
        screenshot = webcam_area.Find("screenshot").GetComponent<RawImage>();
        webcam_frame = webcam_area.Find("webcam_frame").GetComponent<RawImage>();
        import_pic = webcam_area.Find("import_pic").GetComponent<RawImage>();
        screenshot_reddot_parent = screenshot.transform.Find("parent").transform;
        screenshot_reddot = screenshot_reddot_parent.Find("demo").gameObject;
        screenshot_reddot.SetActive(false);
        webcam_frame_reddot_parent = webcam_frame.transform.Find("parent").transform;
        webcam_frame_reddot = webcam_frame_reddot_parent.Find("demo").gameObject;
        webcam_frame_reddot.SetActive(false);
        import_pic_reddot_parent = import_pic.transform.Find("parent").transform;
        import_pic_reddot = import_pic_reddot_parent.Find("demo").gameObject;
        import_pic_reddot.SetActive(false);
#if UNITY_EDITOR
        screenshot.gameObject.AddComponent<MouseController>();
        import_pic.gameObject.AddComponent<MouseController>();
#elif UNITY_ANDROID
        screenshot.gameObject.AddComponent<HandController>();
        import_pic.gameObject.AddComponent<HandController>();
#endif

        EventManager.Instance.AddListener("选择了toggle", OnToggleValueChanged);
        EventManager.Instance.AddListener("开始背部拍照计算", TakePhoto);
        EventManager.Instance.AddListener("收到穴位点位置", OnReceiveAcupointsResult);
        EventManager.Instance.AddListener("放大某个穴位", EnlargeOneAcupoint);

        ChangeRawImageSize(1280, 720);
        OnStartBtnClick();
    }

    /// <summary>
    /// 调整显示相机镜头的UI组件大小, 注意：可能会调整失败，使得图片看上去是拉伸的样子! 
    /// </summary>
    /// <param name="requestWidth">UI调整为特定的宽度</param>
    /// <param name="requestHeight">UI调整为特定的高度</param>
    /// <param name="ratio">设置raw image相对于屏幕的大小</param>
    void ChangeRawImageSize(float requestWidth = 0, float requestHeight = 0, float ratio = 1)
    {
        if (requestWidth <= 0 && requestHeight <= 0)
        {
            requestWidth = Screen.width;
            requestHeight = Screen.height;
        }

        webcam_width = Mathf.FloorToInt(requestWidth);
        webcam_height = Mathf.FloorToInt(requestHeight);
    }

    public void OnStartBtnClick()
    {
        StartCoroutine(Call());
    }

    public IEnumerator Call()
    {
        // 请求权限
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam) && WebCamTexture.devices.Length > 0)
        {
            ShowWebcamOnUI();
            EventManager.Instance.DispatchEvent("成功生成相机画面", null);
        }
    }

    //切换前后摄像头
    public void SwitchCamera(string event_name = null, object udata = null)
    {
        if (WebCamTexture.devices.Length < 1)
            return;

        if (current_webCam != null)
            current_webCam.Stop();

        webcam_index++;
        webcam_index = webcam_index % WebCamTexture.devices.Length;

        ShowWebcamOnUI();
    }

    //创建相机贴图，并在UI显示
    void ShowWebcamOnUI()
    {
        // 创建相机贴图
        current_webCam = new WebCamTexture(WebCamTexture.devices[webcam_index].name, webcam_width, webcam_height, 60);

        webcam_img.texture = current_webCam;
        current_webCam.Play();

        float angle = -current_webCam.videoRotationAngle;
        webcam_img.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
    
        webcam_frame.texture = current_webCam;
        webcam_frame.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
    }

    //停止拍照
    void StopCamera(string event_name = null, object udata = null)
    {
        //如果用户允许访问 
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            if (current_webCam != null && current_webCam.isPlaying)
                current_webCam.Stop();
        }
    }

    //获取实际展示到画面的像素
    void GetActualResolution()
    {
        float wantedRatio = 1.0f * webcam_width / webcam_height;

        actual_tex_width = current_webCam.width;
        actual_tex_height = (int)(actual_tex_width / wantedRatio * 1.0f);
        if (actual_tex_height > current_webCam.height)
        {
            actual_tex_height = current_webCam.height;
            actual_tex_width = (int)(actual_tex_height * wantedRatio * 1.0f);
        }

        UnityEngine.Debug.Log("获取实际展示到画面的像素 actualTexWidth: " + actual_tex_width + "   actualTexHeight: " + actual_tex_height);
    }

    // 获取背部截图，发送处理
    public void TakePhoto(string event_name = null, object udata = null)
    {
        Texture2D destTex = TakeScreenshotAndSendToServer();
        screenshot.texture = destTex;

        EventManager.Instance.DispatchEvent("显示浮动提示", "截图成功，开始处理!");

        ResetCreatedReddot(screenshot_reddot_parent);

        EventManager.Instance.DispatchEvent("发送背部截图", destTex);
    }

    //截取相机画面，发送图片到服务端
    Texture2D TakeScreenshotAndSendToServer()
    {
        int width = 0;
        int height = 0;
        int texWidth = 0;
        int texHeight = 0;
        if (Application.platform == RuntimePlatform.Android)
        {
            width = current_webCam.height;
            height = current_webCam.width;
            texWidth = height;
            texHeight = width;
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            width = current_webCam.width;
            height = current_webCam.height;
            texWidth = width;
            texHeight = height;
        }

        UnityEngine.Debug.Log("texWidth:" + texWidth + "_" + texHeight + "。currentWebcamWidth:" + current_webCam.width + "_" + current_webCam.height);
        Color[] pix = current_webCam.GetPixels(0, 0, texWidth, texHeight);
        Texture2D destTex = new Texture2D(texWidth, texHeight); // (width, height);
        destTex.SetPixels(pix);
        destTex.Apply();

        CheckShouldCollectGC();

        return destTex;
    }

    void CheckShouldCollectGC()
    {
        CommonParams.Instance.current_gc_count++;
        if (CommonParams.Instance.current_gc_count > CommonParams.Instance.collect_gc_count)
        {
            CommonParams.Instance.current_gc_count = 0;
            System.GC.Collect();
        }
    }

    //ui的显示隐藏
    void OnToggleValueChanged(string event_name = null, object udata = null)
    {
        int index = (int)udata;
        CurrentTopBarToggleIndex toggle_index = (CurrentTopBarToggleIndex)index;

        webcam_img.gameObject.SetActive(toggle_index == (CurrentTopBarToggleIndex)0);
        screenshot.gameObject.SetActive(toggle_index == (CurrentTopBarToggleIndex)1);
        webcam_frame.gameObject.SetActive(toggle_index == (CurrentTopBarToggleIndex)2);
        import_pic.gameObject.SetActive(toggle_index == (CurrentTopBarToggleIndex)3);

        OpenWebcamFrameMode(toggle_index == (CurrentTopBarToggleIndex)2);
    }

    //检测是否开启了动态检测,进行相关操作
    void OpenWebcamFrameMode(bool isModeOpen)
    {
        CommonParams.Instance.isFrameDetectMode = isModeOpen;
        TakeCamFrame();
    }

    //截取视频流画面，发送处理 
    void TakeCamFrame(string event_name = null, object udata = null)
    {
        if (!CommonParams.Instance.isFrameDetectMode) return;
        Texture2D destTex = TakeScreenshotAndSendToServer();
        EventManager.Instance.DispatchEvent("发送背部截图", destTex);
    }

    #region 穴位红点
    //生成红点
    void OnReceiveAcupointsResult(string event_name = null, object udata = null)
    {
        try
        {
            Transform parent = null;
            GameObject child = null;

            switch (CommonParams.Instance.current_toggle_index)
            {
                case (CurrentTopBarToggleIndex)1:
                    parent = screenshot_reddot_parent;
                    child = screenshot_reddot;
                    break;
                case (CurrentTopBarToggleIndex)2:
                    parent = webcam_frame_reddot_parent;
                    child = webcam_frame_reddot;
                    ResetCreatedReddot(webcam_frame_reddot_parent);
                    break;
                case (CurrentTopBarToggleIndex)3:
                    parent = import_pic_reddot_parent;
                    child = import_pic_reddot;
                    break;
            }

            RawImage tex_ui = parent.parent.GetComponent<RawImage>();

            //截图的长宽
            float picWidth = tex_ui.texture.width;
            float picHeight = tex_ui.texture.height;

            //屏幕的长宽
            float width = tex_ui.rectTransform.sizeDelta.x; // webcam_width;
            float height = tex_ui.rectTransform.sizeDelta.y; // webcam_height;

            //屏幕长宽和截图长宽的缩放比例
            float ratioWidth = width / picWidth;
            float ratioHeight = height / picHeight;

            //穴位红点的生成
            MessageFormat.COTR2DData data = (MessageFormat.COTR2DData)udata;
            Debug.Log("点的个数：" + data.arr.Length + "。点的名字个数：" + CommonParams.Instance.acupoint_str.Length);
            for (int i = 0; i < data.arr.Length; i++)
            {
                if (i >= CommonParams.Instance.acupoint_str.Length) break;
                float x = (float)data.arr[i].X;
                float y = (float)data.arr[i].Y;

                GameObject go = ObjectPool.Instance.GetObj(child, parent);
                go.GetComponent<RectTransform>().localPosition = new Vector3(ratioWidth * x, ratioHeight * (picHeight - y), 0); // x * picWidth, y * picHeight
                go.transform.localScale = Vector3.one;
                go.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
                go.name = i.ToString();
                go.transform.GetComponentInChildren<Text>().text = CommonParams.Instance.acupoint_str[i];
                reddot_list.Add(go);
            }

            parent.GetComponent<RectTransform>().localEulerAngles = -tex_ui.rectTransform.localEulerAngles;
            EventManager.Instance.DispatchEvent("背部穴位点生成完毕", null);
            TakeCamFrame(); //注意：在PC使用本地的模型计算，因为计算时间太短会报错！！！
        }
        catch (System.Exception e)
        {
        }
    }

    //清除红点
    void ResetCreatedReddot(Transform parent)
    {
       
        if (CommonParams.Instance.current_acupoint_index >= 0 && reddot_list.Count > CommonParams.Instance.current_acupoint_index)
            reddot_list[CommonParams.Instance.current_acupoint_index].transform.localScale = Vector3.one;
        ObjectPool.Instance.RecycleAllChildren(parent.gameObject);
        reddot_list.Clear();
        CommonParams.Instance.current_acupoint_index = -1;
        EventManager.Instance.DispatchEvent("重置旧的穴位点", null);
    }

    //放大选中的点
    void EnlargeOneAcupoint(string event_name = null, object udata = null)
    {
        reddot_list[CommonParams.Instance.current_acupoint_index].transform.localScale = new Vector3(2, 2, 2);
    }

    #endregion
}