using MessageFormat;

public class CommonParams : Singleton<CommonParams>
{
    public string ServerIp = "127.0.0.1"; //服务端ip地址
    public int ServerPort = 10086; //服务端端口
    public bool isConnectedToServer = false;//是否连上了服务端

    public readonly bool isUsingPCWebcam = false; //是否在电脑上测试，使用电脑摄像头？
    public readonly float PCWebcamRotationZ = -90f; //使用电脑摄像头时，截图在UI上显示的组件Z值要旋转多少度(按照安卓的图片旋转角度)
    public readonly bool isSendLocalPicToServer = false; //是否发送预定的本地图片给服务端测试？
    public readonly string localPicFilePath = "Res/myhand_square 000"; //测试发送本地图片时，要发送的Resources文件夹下的图片名称

    public int current_gc_count = 0;
    public readonly int collect_gc_count = 100; //获取多少次图片之后回收GC

    public readonly ClientPlatform platform = ClientPlatform.Android; //当前平台

    public readonly string[] acupoint_str = { "点1", "点2", "点3", "点4", "点5"};

    public CurrentTopBarToggleIndex current_toggle_index = CurrentTopBarToggleIndex.未选择; //选中的顶部toggle
    public int current_acupoint_index = -1; //选中了的对应acupoint_str的index

    public bool isFrameDetectMode = false; //是否开启了动态检测？

    //打印当前的时间（测试每个步骤需要的时长，需要优化）
    public void PrintCurrentTime(string prefix = "")
    {
        System.DateTime nowTime = System.DateTime.Now;
        UnityEngine.Debug.Log(prefix + " 当前时、分、秒、毫秒：" + nowTime.Hour + ":" + nowTime.Minute + ":" + nowTime.Second + "-" + nowTime.Millisecond);
    }

    public string GetCurrentTime(string prefix = "")
    {
        System.DateTime nowTime = System.DateTime.Now;
        string a = prefix + " 当前时、分、秒、毫秒：" + nowTime.Hour + ":" + nowTime.Minute + ":" + nowTime.Second + "-" + nowTime.Millisecond;
        return a;
    }
}
