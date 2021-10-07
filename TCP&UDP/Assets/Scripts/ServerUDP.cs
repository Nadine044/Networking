using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
public class ServerUDP : MonoBehaviour
{
    public Socket _socket;
    int recv;
    byte[] data = new byte[1024];
    private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);


    public void Start()
    {
        startThreadingFunction(Server);
        //StartCoroutine(Server());
    }


    public void startThreadingFunction(Action function)
    {
        Thread t = new Thread(function.Invoke);
        t.Start();
    }


    void Server()
    {
        int count = 0;
        //Create socket
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);
        //Bind Socket to server addres
        _socket.Bind(ipep);


        //wait Until datagram packet arrives from client
        recv = _socket.ReceiveFrom(data, ref epFrom);

        //when this arrives 

        //we process it and send to the client

        Debug.Log("Welcome to the server " + epFrom.ToString());
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));

        Thread.Sleep(500);
        count++;
        string pong = "pong";
        data = Encoding.ASCII.GetBytes(pong);
        _socket.SendTo(data, data.Length, SocketFlags.None, epFrom);


        while (count <5 )
        {
            count++;
            data = new byte[1024];
            recv = _socket.ReceiveFrom(data, ref epFrom);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));

            Thread.Sleep(500);
            data = Encoding.ASCII.GetBytes(pong);
            _socket.SendTo(data, recv, SocketFlags.None, epFrom);
        }

        _socket.Close();
    }


}
