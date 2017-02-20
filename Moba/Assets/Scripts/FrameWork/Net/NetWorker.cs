using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public delegate void OnReceiveMsg(ref List<byte[]> cache);
public class NetWorker
{
    private Socket socket;

    private static readonly object socketLock = new object();
    private static readonly object sendLock = new object();
    private static readonly object receiveLock = new object();

    private Thread sendThread;
    private Thread receiveThread;

    private byte[] sendBuffer;
    private byte[] receiveBuffer;
    private const int MAX_BUFFER_SIZE = 65535;

    private Queue<byte[]> sendQueue = new Queue<byte[]>();
    private List<byte[]> receiveCache = new List<byte[]>();

    #region Socket Connect

    public NetWorker()
    {
        this.sendBuffer = new byte[MAX_BUFFER_SIZE];
        this.receiveBuffer = new byte[MAX_BUFFER_SIZE];

        //发送线程
        sendThread = new Thread(new ThreadStart(sendAsync));
        if (!sendThread.IsAlive)
        {
            sendThread.Start();
        }

        //接收线程
        receiveThread = new Thread(new ThreadStart(ReveiveAsync));
        if (!receiveThread.IsAlive)
        {
            receiveThread.Start();
        }
    }

    public void Connect(string ip, int port)
    {
        if (this.Connected())
        {
            Debug.LogError("Socket has Connected,can not connect!!");
            return;
        }

        try
        {
            lock (socketLock)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ip, port);
            }

        }
        catch (Exception)
        {
            Debug.LogError("Socket connect Error!!");
        }
    }

    public void DisConnect()
    {

    }

    public bool Connected()
    {
        return socket != null && socket.Connected;
    }
    #endregion

    #region Msg Send

    private void sendAsync()
    {
        while (true)
        {
            lock (socketLock)
            {
                if (this.Connected())
                {
                    int totalLength = 0;
                    lock (sendLock)
                    {
                        //并包处理
                        while (this.sendQueue.Count > 0 && totalLength < MAX_BUFFER_SIZE)
                        {
                            byte[] msg = this.sendQueue.Peek();
                            int msglength = msg.Length;
                            if (totalLength + msglength <= MAX_BUFFER_SIZE)
                            {
                                msg.CopyTo(this.sendBuffer, totalLength);
                                totalLength += msglength;
                                this.sendQueue.Dequeue();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    try
                    {
                        this.socket.Send(this.sendBuffer, totalLength, SocketFlags.None);
                        Array.Clear(this.sendBuffer, 0, totalLength);
                    }
                    catch (Exception)
                    {
                        Debug.LogError("Socket send Error");
                    }
                }
            }
            Thread.Sleep(1000);
        }
    }

    public void Send(byte[] msg)
    {
        lock (sendLock)
        {
            this.sendQueue.Enqueue(msg);
        }
    }
    #endregion

    #region Msg received


    private void ReveiveAsync()
    {
        do
        {
            lock (socketLock)
            {
                if (this.Connected())
                {
                    try
                    {
                        int readBytes = this.socket.Receive(this.receiveBuffer, SocketFlags.None);
                        byte[] msg = new byte[readBytes];
                        Buffer.BlockCopy(this.receiveBuffer, 0, msg, 0, readBytes);

                        //清空缓存
                        Array.Clear(this.receiveBuffer, 0, MAX_BUFFER_SIZE);

                        //分包处理


                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
        } while (true);
    }
    #endregion
}
