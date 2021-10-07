using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ServerTCP : MonoBehaviour
{
    // Start is called before the first frame update
    int maxClients = 1;
    private Socket _socket;
    private IPEndPoint ipep;

    void Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Any, 27000);

        startThreadingFunction(Server);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //for now programmed for only 1 connection
    void Server() //Need to polish the use of Threads
    {
        bool connected = false;
        int count=0;
        byte[] data = new byte[1024];
        int recv=0;
        Socket client = null;
        string pong = "pong";
        IPEndPoint client_ep= null;
        try
        {
            _socket.Bind(ipep);
            _socket.Listen(maxClients);
            Debug.Log("Waiting for a client");
            client = _socket.Accept();
            client_ep = (IPEndPoint)client.RemoteEndPoint;
            Debug.Log("Connected: " + client_ep.ToString());
            connected = true;
            count++;
            data = Encoding.ASCII.GetBytes(pong);
            client.Send(data, data.Length, SocketFlags.None);
        }
        catch(Exception e)
        {
            Debug.LogError("Connection failed..." + e.ToString());
        }

        while(count <5 && connected)
        {
            count++;
            data = new byte[1024];

            if (client != null)
            {
                Thread.Sleep(1000);
                recv = client.Receive(data);
                Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
                client.Send(data, recv, SocketFlags.None);
            }

        }

        Debug.Log("Disconnected from" + client_ep.Address);
        client.Close();
        _socket.Close();

    }

    public void startThreadingFunction(Action function)
    {
        Thread t = new Thread(function.Invoke);
        t.Start();
    }
}
