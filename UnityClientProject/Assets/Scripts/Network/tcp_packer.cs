using System;

public class tcp_packer
{
    private const int HEADER_SIZE = 2;

    //打包，在包内容前加上包总体大小（ushort）
    public static byte[] pack(byte[] cmd_data)
    {
        int len = cmd_data.Length;
        if (len > 65535 - 2)
        {
            return null;
        }

        int cmd_len = len + HEADER_SIZE;
        byte[] cmd = new byte[cmd_len];
        data_viewer.write_ushort_le(cmd, 0, (ushort)cmd_len);
        data_viewer.write_bytes(cmd, HEADER_SIZE, cmd_data);

        return cmd;
    }

    //打包，在包内容前加上包总体大小（uint）
    public static byte[] packUint(byte[] cmd_data)
    {
        int HEADER_SIZE = 4; // uint 4个字节

        int len = cmd_data.Length;

        int cmd_len = len + HEADER_SIZE;
        byte[] cmd = new byte[cmd_len];
        data_viewer.write_uint_le(cmd, 0, (uint)cmd_len);
        data_viewer.write_bytes(cmd, HEADER_SIZE, cmd_data);

        return cmd;
    }

    //读取包头大小（ushort）
    public static bool read_header(byte[] data, int data_len, out int pkg_size, out int head_size)
    {
        pkg_size = 0;
        head_size = 0;

        if (data_len < 2)
        {
            return false;
        }

        head_size = 2;
        pkg_size = (data[0] | (data[1] << 8));

        return true;
    }

    /// <summary>
    /// 读取包头大小（uint）
    /// </summary>
    /// <param name="data">socket收到的包</param>
    /// <param name="data_len">收到的包的字节长度</param>
    /// <param name="pkg_size"></param>
    /// <param name="head_size"></param>
    /// <returns></returns>
    public static bool read_header_uint(byte[] data, int data_len, out int pkg_size, out int head_size)
    {
        pkg_size = 0; //包的实际大小，防止分包粘包
        head_size = 0; //头大小，包大小所占的字节数, uint 4个字节

        if (data_len < 4)
        {
            return false;
        }

        head_size = 4;

        byte[] pkg_size_buffer = new byte[4];
        Array.Copy(data, 0, pkg_size_buffer, 0, 4);
        pkg_size = BitConverter.ToInt32(pkg_size_buffer, 0);

        if (pkg_size < data_len)
        {
            //TODO , 粘包的处理
        }

        if (data_len != pkg_size)
            return false;    //不完整的包，分包

        return true;
    }
}
