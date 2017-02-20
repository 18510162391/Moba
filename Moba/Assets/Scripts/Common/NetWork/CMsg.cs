using System;
using System.IO;

public class CMsg
{
    private int m_nProtocalType;
    private CMsgBuff m_pMsgBuff;

    /// <summary>
    /// 网络传输模型
    /// </summary>
    /// <param name="protocalType">协议类型</param>
    /// <param name="stream">消息体</param>
    public CMsg(int protocalType, MemoryStream stream)
    {
        m_pMsgBuff = new CMsgBuff((int)stream.Length);
        this.m_nProtocalType = protocalType;

        int msgLength = 0;
        this.Add(msgLength);
        this.Add(this.m_nProtocalType);
    }
    public void Add(Int32 value)
    {
        this.m_pMsgBuff.Add(value);
        this.m_pMsgBuff.UpdateBuffSize();
    }

    public void Add(byte[] byts)
    {
        this.m_pMsgBuff.Add(byts);
        this.m_pMsgBuff.UpdateBuffSize();
    }

    public byte[] GetMsgBuffer()
    {
        return this.m_pMsgBuff.GetMsgBuffer();
    }

    public int GetMsgBufferSize()
    {
        return this.m_pMsgBuff.GetMsgBufferSize();
    }
}

