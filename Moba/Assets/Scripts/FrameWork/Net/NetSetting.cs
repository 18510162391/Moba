using System;



public class NetSetting
{
    private string _ip;

    public string IP
    {
        get { return _ip; }
        set { _ip = value; }
    }

    private int _port;

    public int Port
    {
        get { return _port; }
        set { _port = value; }
    }

}
