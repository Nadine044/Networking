using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System;
using System.Threading;
using System.Net.Sockets;

public class NetworkingClient : Networking
{
    public static NetworkingClient _instance { get; private set; }

    private static ManualResetEvent recieveDone = new ManualResetEvent(false);

    bool close_connection = false;

    //int client = 0;
    // Start is called before the first frame update
    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _instance = this;
        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 26002);
        StartThreadingFunction(ConnectToServer);
    }
    void ConnectToServer()
    {
        socket.BeginConnect(ipep, new AsyncCallback(ConnectCallback), socket);
    }
    private void OnConnectedToServer()
    {
        Debug.Log("hey");
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CloseConnection();
            Application.Quit();
        }
    }

    void ConnectCallback(IAsyncResult ar)
    {
        //Here tell player he is connected And launch and open UpdateConnectionWith server
        StartThreadingFunction(UpdatingConnection);
    }

    void CloseConnection()
    {
        socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
        socket.Close();
    }
    void UpdatingConnection()
    {
        byte[] data = new byte[1024];
        while(!close_connection)
        {
            recieveDone.Reset();

            if (close_connection)
                break;

            OBJ obj  = new OBJ();

            socket.BeginReceive(obj.buffer, 0, OBJ.buffersize, 0, new AsyncCallback(ReadCallback), obj);

            recieveDone.WaitOne();
        }

        CloseConnection();
    }

    void ReadCallback(IAsyncResult ar)
    {
        OBJ obj = (OBJ)ar.AsyncState;

        int bytesread = 0;

        bytesread = socket.EndReceive(ar);

        if(bytesread >0)
        {
            Package package = Deserialize(obj.buffer);

            int client_n = package.client;
            int turnstep = package.index;
            int[] board_tmp = package.board_array;
            Debug.Log(package.msg_to_log);

           // Player._instance.RecieveUpdateFromServer(client_n, turnstep, board_tmp);

            Action UpdatePlayer = () =>
            {
                Player._instance.RecieveUpdateFromServer(client_n, turnstep, board_tmp);
            };
            QueueMainThreadFunction(UpdatePlayer);

            //not thread safe


            recieveDone.Set();
            //need to know which is which
        }
        else
        {
            //close connection
            close_connection = true;
            recieveDone.Set();
        }
    }


    public void SendPackage()
    {
        byte[] b = Serialize(2, "new move done",Player._instance.GetBoard(), Player._instance.client_n);
        socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SendCallback), socket);
    }

    private void SendCallback(IAsyncResult ar)
    {
        Debug.Log("Send to server");
    }
}
