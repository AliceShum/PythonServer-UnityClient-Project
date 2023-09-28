using System;

public enum CurrentTopBarToggleIndex
{
    动态区 = 0,
    静态背部区 = 1,
    动态检测区 = 2,
    导入背部区 = 3,
    未选择 = -1,
}

namespace MessageFormat
{

    [Serializable]
    public struct VertexDataDouble
    {
        public double X;
        public double Y;
        public VertexDataDouble(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    [Serializable]
    //COTR数据 穴位点2D坐标+名字+穴位点对应的图片大小
    public class COTR2DData
    {
        public VertexDataDouble[] arr;
        public string[] names;
        public VertexDataDouble picSize;
    }

    //客户端信息
    [Serializable]
    public class ClientInfo
    {
        public ClientPlatform platform; //平台信息
        public string ipAddr; //客户端ip地址
        public int port;//客户端端口号

        public override string ToString()
        {
            return "platform:" + platform.ToString() + "; ipAddr:" + ipAddr + "; port:" + port;
        }
    }

    [Serializable]
    public enum ClientPlatform
    {
        Unidentified = 0,
        Hololens = 1,
        Android = 2,
        WSL = 3,
    }

    public class InfoBoxMsg
    {
        public string title_txt;
        public string content_txt;
        public string confirm_btn_txt;
        public string confirm_btn_event_name;
    }
}