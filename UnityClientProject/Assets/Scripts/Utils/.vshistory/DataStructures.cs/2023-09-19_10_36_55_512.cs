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

    #region 一般
    //旧：
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
    public struct Vector3Double
    {
        public double x;
        public double y;
        public double z;
        public Vector3Double(double X, double Y, double Z)
        {
            this.x = X;
            this.y = Y;
            this.z = Z;
        }
    }

    [Serializable]
    public struct Vector4Double
    {
        public double x;
        public double y;
        public double z;
        public double w;
        public Vector4Double(double X, double Y, double Z, double W)
        {
            this.x = X;
            this.y = Y;
            this.z = Z;
            this.w = W;
        }
    }

    //物体世界坐标位置 
    [Serializable]
    public class ObjPosition
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
    }

    [Serializable]
    //物体屏幕坐标位置
    public class ObjScreenPosition
    {
        public float x = 0;
        public float y = 0;
    }

    [Serializable]
    //相机UI长宽
    public class WebcamUISize
    {
        public float width = 0;
        public float height = 0;
    }

    #endregion

    #region COTR相关

    [Serializable]
    public enum BodyPart
    {
        未选择 = 0,
        背部 = 1,
        手背 = 2,
    }

    [Serializable]
    //身体部位照片
    public class BodyPartPic
    {
        public byte[] texBuffer; //图片的字节流
        public int texWidth; //图片的宽度
        public int texHeight; //图片的高度
        public bool isLeft; // 部位是左边还是右边
        public BodyPart bodyPart;
        public bool isTakingVideo = false;
    }

    [Serializable]
    //COTR数据 穴位点2D坐标+名字+穴位点对应的图片大小
    public class COTR2DData
    {
        public VertexDataDouble[] arr;
        public string[] names;
        public VertexDataDouble picSize;
    }

    #endregion

    #region 客户端相关
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
    #endregion

    //摄像头画面分辨率
    public enum WebcamResolution
    {
        正方形1x1,
        矩形1280x720,
        全屏,
        未定义,
        矩形720x1280,
    }

    public class cmd_msg
    {
        public int stype; //服务号
        public int ctype; //命令号
        public byte[] body; // protobuf, utf8 string json byte;
    }

    public class InfoBoxMsg
    {
        public string title_txt;
        public string content_txt;
        public string confirm_btn_txt;
        public string confirm_btn_event_name;
    }
}