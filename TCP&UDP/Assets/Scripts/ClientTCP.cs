using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
public class ClientTCP : MonoBehaviour
{
    string ping = "ping";
    // Start is called before the first frame update
    public void Start()
    {
        StartThreadingFunction(Client);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartThreadingFunction(Action function)
    {
        Thread t = new Thread(function.Invoke);
        t.Start();
    }
    void Client()
    {
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);

        byte[] data = new byte[1024];
        int count = 0;
        bool connected = false;
        try
        {
            server.Connect(ipep);
            connected = true;
            Debug.Log("connected to server");
        }
        catch (SocketException e)
        {
            Debug.Log("Unable to connect to server  " + e.ToString());
            Thread.CurrentThread.Abort();
        }

        //Recieve is blocking
        server.Send(Encoding.ASCII.GetBytes(ping));
        Debug.Log("ping");
        count++;
        Thread.Sleep(500);
        int recv = server.Receive(data);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv)); //Peta

        while(count < 5 && connected )
        {
            count++;
            Thread.Sleep(500);
            server.Send(Encoding.ASCII.GetBytes(ping));
            data = new byte[1024];
            recv = server.Receive(data);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv)); //Crashes here in the last update

        }

        Debug.Log("Disconnecting From server");
        server.Shutdown(SocketShutdown.Both);
        
        server.Close();
        Thread.CurrentThread.Abort();
    }
}
