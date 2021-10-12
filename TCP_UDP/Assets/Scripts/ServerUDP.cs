using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
public class ServerUDP : ServerBase //Not working with ui
{

    int recv;
    byte[] data = new byte[1024];
    private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);

    public void Start()
    {
        startThreadingFunction(Server);
    }
    private void Update()
    {
        while (functionsToRunInMainThread.Count > 0)
        {
            //Grab the first/oldest function in the list
            Action someFunc = functionsToRunInMainThread.Peek();
            functionsToRunInMainThread.Dequeue();

            //Now run it;
            someFunc();
        }
    }


    //Maybe not do everything in the thread only the blocking things
    void Server()
    {
        int count = 0;
        Debug.Log("Hey");
        //Create socket
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);
        //Bind Socket to server addres
        _socket.Bind(ipep);
        Debug.Log("Binded");
        Action Initial_Info = () => { logControl.LogText("Socket Binded", Color.black); };
        QueueMainThreadFunction(Initial_Info);

        //wait Until datagram packet arrives from client
        //Recieve message from client & wait 500ms
        recv = _socket.ReceiveFrom(data, ref epFrom);

        Action WaitingClient = () => { logControl.LogText("Recieved UDP Server" + Encoding.ASCII.GetString(data, 0, recv), Color.black); };
        QueueMainThreadFunction(WaitingClient);

        Debug.Log("Welcome to the UDP server " + epFrom.ToString());
        Debug.Log("Recieved UDP Server" + Encoding.ASCII.GetString(data, 0, recv));

        Thread.Sleep(500);
        count++;
        string pong = "pong";

        //Send message to client
        data = Encoding.ASCII.GetBytes(pong);
        _socket.SendTo(data, data.Length, SocketFlags.None, epFrom);


        while (count <5 ) //number of sended messages to client
        {
            count++;
            data = new byte[1024];
            recv = _socket.ReceiveFrom(data, ref epFrom);
            Debug.Log("Recieved UDP Server " + Encoding.ASCII.GetString(data, 0, recv));

            Action RecievingMsg = () => { logControl.LogText(Encoding.ASCII.GetString(data, 0, recv), Color.black); };
            QueueMainThreadFunction(RecievingMsg);

            Thread.Sleep(500);
            data = Encoding.ASCII.GetBytes(pong);
            _socket.SendTo(data, recv, SocketFlags.None, epFrom);
        }

        _socket.Close();
        Action SocketClosed = () => { logControl.LogText("Socket Closed", Color.black); };
        QueueMainThreadFunction(SocketClosed);
    }

}
