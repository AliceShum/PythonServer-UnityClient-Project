using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MessageFormat;

public class UIController : MonoBehaviour
{
    private Dropdown acupoints_dropdown; //下拉的选择穴位点
    private Slider progress_slider1; //进度条1
    private Slider progress_slider2; //进度条2
    private ToggleGroup toggleGroup;
    private Toggle toggle_webcam; //动态区
    private Toggle toggle_pic_back; //静态背部区
    private Toggle toggle_frame_back; //动态检测区
    private Toggle toggle_import_back; //导入背部区
    private Text cost_time_txt; //花费的时间

    private float deal_pic_start_time = 0; //开始运算的时间

    private void Start()
    {
        InitUI();
        BindEvent();
    }

    void InitUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        cost_time_txt = canvas.transform.Find("bg/right_bg/cost_time").GetComponent<Text>();
        Transform operation = canvas.transform.Find("bg/operation");
        operation.Find("setting_btn").GetComponent<Button>().onClick.AddListener(OnSettingBtnClick);
        operation.Find("about_btn").GetComponent<Button>().onClick.AddListener(OnAboutBtnClick);
        operation.Find("find_acupoint_btn").GetComponent<Button>().onClick.AddListener(OnFindAcupointBtnClick);
        operation.Find("send_btn").GetComponent<Button>().onClick.AddListener(OnSendBtnClick);
        operation.Find("back_screenshot_btn").GetComponent<Button>().onClick.AddListener(OnBackScreenshotBtnClick);
        operation.Find("import_back_btn").GetComponent<Button>().onClick.AddListener(OnImportBackBtnClick);
        operation.Find("acupoints_dropdown/switch").GetComponent<Button>().onClick.AddListener(OnAcupointsDropdownSwitchBtnClick);

        acupoints_dropdown = operation.Find("acupoints_dropdown/dropdown").GetComponent<Dropdown>();
        progress_slider1 = operation.Find("progress_bar/Slider1").GetComponent<Slider>();
        progress_slider2 = operation.Find("progress_bar/Slider2").GetComponent<Slider>();
        progress_slider1.gameObject.AddComponent<ProgressSlider>();
        progress_slider2.gameObject.AddComponent<ProgressSlider>();

        toggleGroup = operation.Find("top_buttons/toggle_group").GetComponent<ToggleGroup>();
        toggle_webcam = operation.Find("top_buttons/toggle_group/Toggle1").GetComponent<Toggle>();
        toggle_pic_back = operation.Find("top_buttons/toggle_group/Toggle2").GetComponent<Toggle>();
        toggle_frame_back = operation.Find("top_buttons/toggle_group/Toggle3").GetComponent<Toggle>();
        toggle_import_back = operation.Find("top_buttons/toggle_group/Toggle4").GetComponent<Toggle>();

        toggle_webcam.onValueChanged.AddListener(OnToggleWebcamValueChanged);
        toggle_pic_back.onValueChanged.AddListener(OnTogglePicBackValueChanged);
        toggle_frame_back.onValueChanged.AddListener(OnToggleFrameBackValueChanged);
        toggle_import_back.onValueChanged.AddListener(OnToggleImportBackValueChanged);

        OnAcupointsDropdownSwitchBtnClick();
        InitAcupointDropdownList();

        canvas.transform.Find("bg/info_box").gameObject.AddComponent<InfoBoxPanel>();
        canvas.transform.Find("bg/connect_panel").gameObject.AddComponent<ConnectPanel>();
        canvas.transform.Find("bg/prompt_panel").gameObject.AddComponent<PromptPanel>();

    }

    void BindEvent()
    {
        EventManager.Instance.AddListener("发送背部截图", OnStartCalculateTime);
        EventManager.Instance.AddListener("收到穴位点位置", OnReceiveAcupointsResult);
        EventManager.Instance.AddListener("重置旧的穴位点", (eventname, udata) =>
        {
            acupoints_dropdown.value = 0;
        });
        EventManager.Instance.AddListener("成功生成相机画面", (eventname, udata) =>
        {
            OnToggleWebcamValueChanged(true);
        });
    }

    //初始化下拉穴位点列表
    void InitAcupointDropdownList()
    {
        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
        for (int i = 0; i < CommonParams.Instance.acupoint_str.Length; i++)
        {
            Dropdown.OptionData d = new Dropdown.OptionData();
            d.text = CommonParams.Instance.acupoint_str[i];
            list.Add(d);
        }
        acupoints_dropdown.ClearOptions();
        acupoints_dropdown.options = list;
        acupoints_dropdown.RefreshShownValue();
        acupoints_dropdown.onValueChanged.AddListener(OnAcupointDropDownValueChanged);
    }

    //设置按钮 TODO
    void OnSettingBtnClick()
    {
        InfoBoxMsg msg = new InfoBoxMsg();
        msg.title_txt = "设置";
        msg.content_txt = "该功能正在开发中，敬请期待！";
        msg.confirm_btn_txt = "确定";
        msg.confirm_btn_event_name = null;
        EventManager.Instance.DispatchEvent("显示提示框", msg);
    }

    //关于按钮 TODO
    void OnAboutBtnClick()
    {
        InfoBoxMsg msg = new InfoBoxMsg();
        msg.title_txt = "关于";
        msg.content_txt = "=== 这是一个人体的穴位点导航系统 ===";
        msg.confirm_btn_txt = "确定";
        msg.confirm_btn_event_name = null;
        EventManager.Instance.DispatchEvent("显示提示框", msg);
    }

    //查找穴位按钮
    void OnFindAcupointBtnClick()
    {
        EventManager.Instance.DispatchEvent("放大某个穴位", null);
    }

    //发送按钮 TODO
    void OnSendBtnClick()
    {
        InfoBoxMsg msg = new InfoBoxMsg();
        msg.title_txt = "提示";
        msg.content_txt = "该功能正在开发中，敬请期待！";
        msg.confirm_btn_txt = "确定";
        msg.confirm_btn_event_name = null;
        EventManager.Instance.DispatchEvent("显示提示框", msg);
    }

    //背部拍照按钮
    void OnBackScreenshotBtnClick()
    {
        toggle_pic_back.isOn = true;
        OnStartCalculateTime();
        EventManager.Instance.DispatchEvent("开始背部拍照计算", null);
    }

    //导入背部图片
    void OnImportBackBtnClick()
    {
        toggle_import_back.isOn = true;
        EventManager.Instance.DispatchEvent("开始导入背部图片计算", null);
    }

    //穴位下拉开关按钮 
    void OnAcupointsDropdownSwitchBtnClick()
    {
        bool isOn = acupoints_dropdown.gameObject.activeInHierarchy;
        acupoints_dropdown.gameObject.SetActive(!isOn);
    }

    //动态区开关的值发生变化  0
    void OnToggleWebcamValueChanged(bool isOn)
    {
        if (isOn)
            CheckCanChangeCurrentToggleValue(0);
    }

    //静态背部区开关的值发生变化   1
    void OnTogglePicBackValueChanged(bool isOn)
    {
        if (isOn)
            CheckCanChangeCurrentToggleValue(1);
    }

    //静态背部区开关的值发生变化   2
    void OnToggleFrameBackValueChanged(bool isOn)
    {
        if (isOn)
            CheckCanChangeCurrentToggleValue(2);
    }

    //导入背部区开关的值发生变化   3
    void OnToggleImportBackValueChanged(bool isOn)
    {
        if (isOn)
            CheckCanChangeCurrentToggleValue(3);
    }

    //改变当前的toggle的值
    bool CheckCanChangeCurrentToggleValue(int new_value)
    {
        if (new_value == ((int)GetCurrentToggleIndex()))
            return false;
        CommonParams.Instance.current_toggle_index = (CurrentTopBarToggleIndex)new_value;
        EventManager.Instance.DispatchEvent("选择了toggle", GetCurrentToggleIndex());
        Debug.Log("新的toglle选项：" + CommonParams.Instance.current_acupoint_index);
        return true;
    }

    //选择的穴位点有改动
    void OnAcupointDropDownValueChanged(int value)
    {
        if (CommonParams.Instance.current_acupoint_index == value) return;
        acupoints_dropdown.value = value;
        CommonParams.Instance.current_acupoint_index = value;
        Debug.Log("选择了第几个穴位点：" + value);
    }

    //对外接口：提供当前toggle index
    public CurrentTopBarToggleIndex GetCurrentToggleIndex()
    {
        return (CurrentTopBarToggleIndex)CommonParams.Instance.current_toggle_index;
    }

    //收到穴位点结果
    void OnReceiveAcupointsResult(string event_name = null, object udata = null)
    {
        decimal past_time = System.Math.Round((decimal)(Time.realtimeSinceStartup - deal_pic_start_time), 2, System.MidpointRounding.AwayFromZero);
        cost_time_txt.text = past_time + "秒";
    }

    //记录开始计算的时间
    void OnStartCalculateTime(string event_name = null, object udata = null)
    {
        deal_pic_start_time = Time.realtimeSinceStartup;
    }

}
