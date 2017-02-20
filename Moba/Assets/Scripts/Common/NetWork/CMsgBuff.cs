using System;
using System.Collections.Generic;
using System.IO;

public class CMsgBuff
{
    private MemoryStream m_pStream;
    private int m_nBuffSize;
    private int m_nWritePos;

    public CMsgBuff(int size)
    {
        m_pStream = new MemoryStream(size);
        this.m_nBuffSize = size;
    }

    public void Add(Int32 value)
    {
        int size = sizeof(Int32);
        byte[] arrayBuff = BitConverter.GetBytes(value);
        this.AddData(arrayBuff, size);
    }
    public void Add(byte value)
    {
        int size = sizeof(byte);
        byte[] arrayBuff = BitConverter.GetBytes(value);
        this.AddData(arrayBuff, size);
    }

    public void Add(byte[] byts)
    {
        int length = byts.Length;
        if (length <= 0)
        {
            return;
        }

        for (int i = 0; i < length; i++)
        {
            this.Add(byts[i]);
        }
    }

    private void AddData(byte[] arrayBuff, int arraySize)
    {
        if (this.m_nWritePos + arraySize > this.m_nBuffSize)
        {
            int newBuffSize = this.m_nBuffSize + arraySize;
            this.ReSize(newBuffSize);
        }
        this.m_pStream.Seek((int)this.m_nWritePos, SeekOrigin.Begin);
        this.m_pStream.Write(arrayBuff, this.m_nWritePos, arraySize);
        this.m_nWritePos += arraySize;
    }

    private void ReSize(int newBuffSize)
    {
        if (newBuffSize <= 0)
        {
            return;
        }

        if (this.m_nWritePos > newBuffSize)
        {
            return;
        }

        MemoryStream oldStream = this.m_pStream;
        this.m_pStream = new MemoryStream(newBuffSize);
        this.m_pStream.Write(oldStream.GetBuffer(), 0, this.m_nWritePos);
        this.m_nBuffSize = newBuffSize;
    }

    public void UpdateBuffSize()
    {
        int size = sizeof(Int32);
        byte[] buff = BitConverter.GetBytes(this.m_nWritePos);
        this.m_pStream.Write(buff, 0, size);
    }

    public byte[] GetMsgBuffer()
    {
        return this.m_pStream.GetBuffer();
    }

    public int GetMsgBufferSize()
    {
        return BitConverter.ToInt32(this.m_pStream.GetBuffer(), 0);
    }
}
