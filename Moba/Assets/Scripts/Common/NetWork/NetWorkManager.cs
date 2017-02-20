using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

class NetWorkManager : UnitySingleton<NetWorkManager>
{
    private string m_strIP = "127.0.0.0";
    private int m_nPort = 45621;
    private bool m_bCanConnect = true;

    private byte[] m_pReceBuff = new byte[2 * 1024 * 1024];
    private int m_nRecePos = 0;

    private IAsyncResult m_pConnectResult;
    private IAsyncResult m_pReceResult;

    private List<int> m_pProtocalID = new List<int>();
    private List<MemoryStream> m_pMsgStream = new List<MemoryStream>();
    private List<MemoryStream> m_pMsgStreamPool;

    private MemoryStream m_pSendStream = new MemoryStream();

    /// <summary>
    /// 正在连接中的tcpclient
    /// </summary>
    private TcpClient m_pConnecting;

    /// <summary>
    /// 连接成功后的tcpClient
    /// </summary>
    private TcpClient m_pClient;

    void Awake()
    {
        m_pMsgStreamPool = new List<MemoryStream>();

        for (int i = 0; i < 50; i++)
        {
            MemoryStream stream = new MemoryStream();
            this.m_pMsgStreamPool.Add(stream);
        }
    }

    public void Connect()
    {
        if (!m_bCanConnect)
        {
            Debug.LogError("当前状态不允许连接");
            return;
        }

        if (this.m_pConnecting != null)
        {
            return;
        }

        if (this.m_pClient != null)
        {
            return;
        }

        try
        {
            IPAddress ip = IPAddress.Parse(this.m_strIP);

            this.m_pConnecting = new TcpClient();
            this.m_pConnectResult = this.m_pConnecting.BeginConnect(ip, m_nPort, null, null);
        }
        catch (Exception)
        {
            this.m_pClient = this.m_pConnecting;
            this.OnConnectError();
            throw;
        }
    }

    void Update()
    {
        if (this.m_pClient != null && this.m_pClient.Connected)
        {
            this.DealMessage();

            if (this.m_pReceResult != null)
            {
                if (this.m_pReceResult.IsCompleted)
                {
                    try
                    {
                        int nReceLength = this.m_pClient.GetStream().EndRead(this.m_pReceResult);
                        this.m_nRecePos += nReceLength;

                        this.OnReceData();

                        this.m_pReceResult = this.m_pClient.GetStream().BeginRead(this.m_pReceBuff, this.m_nRecePos, this.m_pReceBuff.Length - this.m_nRecePos, null, null);
                    }
                    catch (Exception)
                    {
                        this.OnConnectError();
                        throw;
                    }
                }
            }
        }
        else if (this.m_pConnecting != null)
        {
            if (this.m_pConnectResult.IsCompleted && this.m_pConnecting.Connected)
            {
                m_pReceResult = this.m_pClient.GetStream().BeginRead(this.m_pReceBuff, 0, this.m_pReceBuff.Length, null, null);

                this.OnConnectSucceed();
            }
        }
    }

    private void DealMessage()
    {
        while (this.m_pMsgStream.Count > 0 && this.m_pProtocalID.Count > 0)
        {
            int nType = this.m_pProtocalID[0];
            MemoryStream stream = this.m_pMsgStream[0];

            this.m_pProtocalID.RemoveAt(0);
            this.m_pMsgStream.RemoveAt(0);

            MsgHander.Instance.HandleNetMsg(nType, stream);

            if (this.m_pMsgStreamPool.Count < 50)
            {
                this.m_pMsgStreamPool.Add(stream);
            }
            else
            {
                stream = null;
            }
        }
    }

    private void OnReceData()
    {
        int nReadPos = 0;
        while (this.m_nRecePos - nReadPos > 8)
        {
            //包体长度
            int nLength = BitConverter.ToInt32(this.m_pReceBuff, 0);
            //协议号
            int protocalType = BitConverter.ToInt32(this.m_pReceBuff, 4);

            if (nLength < 8)
            {
                Debug.LogError("消息错误，，包体长度小于8");
                break;
            }
            if (nLength > this.m_nRecePos)
            {
                //等待接收完整的包体
                break;
            }
            if (nLength > this.m_nRecePos - nReadPos)
            {
                //等待接收完整的包体
                break;
            }

            MemoryStream pTempStream = null;
            if (this.m_pMsgStreamPool.Count > 0)
            {
                pTempStream = this.m_pMsgStreamPool[0];

                pTempStream.SetLength(0);
                pTempStream.Position = 0;

                this.m_pMsgStreamPool.RemoveAt(0);
            }
            else
            {
                pTempStream = new MemoryStream();
            }

            this.m_pProtocalID.Add(protocalType);
            pTempStream.Write(this.m_pReceBuff, nReadPos + 8, nLength - nReadPos - 8);
            this.m_pMsgStream.Add(pTempStream);

            nReadPos += nLength;
        }

        if (nReadPos > 0 && nReadPos <= this.m_nRecePos)
        {
            this.m_nRecePos -= nReadPos;
            if (this.m_nRecePos > 0)
            {
                Buffer.BlockCopy(this.m_pReceBuff, nReadPos, this.m_pReceBuff, 0, this.m_nRecePos);
            }
        }
    }

    public void SendMsg(IExtensible extensible, int protocalType)
    {
        this.m_pSendStream.SetLength(0);
        this.m_pSendStream.Position = 0;

        ProtoBuf.Serializer.Serialize(this.m_pSendStream, extensible);
        //构建传输模型
        CMsg msg = new CMsg(protocalType, this.m_pSendStream);
        msg.Add(this.m_pSendStream.ToArray());

        this.m_pClient.GetStream().Write(msg.GetMsgBuffer(), 0, msg.GetMsgBufferSize());
    }


    public void Close()
    {
        if (this.m_pClient != null)
        {
            try
            {
                this.m_pClient.Client.Shutdown(SocketShutdown.Both);
                this.m_pClient.GetStream().Close();
                this.m_pClient.Close();
                this.m_pClient = null;

                this.Reset();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
    public void Reset()
    {
        this.m_pClient = null;
        this.m_pConnecting = null;
        this.m_pConnectResult = null;
    }


    private void OnConnectError()
    {
        this.Close();
    }

    private void OnConnectSucceed()
    {
        this.m_pClient = this.m_pConnecting;
        this.m_pConnecting = null;
        this.m_bCanConnect = false;
    }

    private void OnConnectClosed()
    {

    }
}
