using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System;
using System.Threading;
using System.Net.Sockets;
using UnityEngine.UI;

public class NetworkingClient : Networking
{
    private class OBJ
    {
        public const int buffersize = 1024;
        public byte[] buffer = new byte[buffersize];
    }
    public static NetworkingClient _instance { get; private set; }

    private static ManualResetEvent recieveDone = new ManualResetEvent(false);

    bool close_connection = false;

    public Text logText;

    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _instance = this;
        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 26003);
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

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CloseConnection();
            Application.Quit();
        }
        while (functionsToRunInMainThread.Count > 0)
        {
            //Grab the first/oldest function in the list
            Action someFunc;
            functionsToRunInMainThread.TryDequeue(out someFunc);
            //Now run it;
            someFunc();
        }
    }

    void ConnectCallback(IAsyncResult ar)
    {
        Debug.Log("Connect callback");
        Action s = () =>
        {
            logText.text = "Connect callback";
        };

        try {
            Socket client_c = (Socket)ar.AsyncState;
            client_c.EndConnect(ar);
            QueueMainThreadFunction(s);
            StartThreadingFunction(UpdatingConnection);
        }
        catch (SocketException e)
        {
            Debug.LogWarning("Couldn't connect to the server " + e);
        }
    }

    public void CloseConnection()
    {
        Debug.Log("Closing connection");
        Action s = () =>
        {
            logText.text = "closing connection <CloseConnection()>";
        };
        QueueMainThreadFunction(s);

        try
        {
            socket.Shutdown(SocketShutdown.Both);
        }
        catch(SocketException e)
        {
            Debug.LogWarning("ShutDown with server failed " + e);
        }

        try
        {
            socket.Close();
        }
        catch(SocketException e)
        {
            Debug.Log("Couldn't close the socket connection with server " + e);
        }
        Debug.Log("ConnectionClosed");
        logText.text = "ConnectionClosed";
    }

    void UpdatingConnection()
    {
        byte[] data = new byte[1024];
        while(!close_connection)
        {
            recieveDone.Reset();

            if (close_connection)
                break;

            OBJ obj  = new OBJ(); //TODO make sure socket is connected
            if (socket.Connected)
            {
                socket.BeginReceive(obj.buffer, 0, OBJ.buffersize, 0, new AsyncCallback(ReadCallback), obj);
            }
            else
                break;

            recieveDone.WaitOne();
        }
    }
   
    void ReadCallback(IAsyncResult ar)
    {
        if (ar == null)
        {
            Debug.Log("async resul is null, closing connection & quitting application");
            close_connection = true;
            recieveDone.Set();
            CloseConnection();
            Application.Quit();
            return;
        }
        Debug.Log("Read Callback");
        OBJ obj = (OBJ)ar.AsyncState;

        int bytesread = 0;
        if(socket.Connected)
            bytesread = socket.EndReceive(ar);
        if(bytesread >0)
        {
            Package package = Deserialize(obj.buffer);

            Action UpdatePlayer = () =>
            {
                logText.text = "Updating player <ReadCallback()>";
                Debug.Log("Updating player");
                switch(package.index)
                {
                    case -1:
                        User._instance.AwaitForClientReconnection();
                        break;
                    case -2:
                        User._instance.RecieveReconnectionUpdateFromServerNoMove(package.board_array, package.token_list_id,package.win_counter);
                        break;
                    case -3:
                        User._instance.RecieveReconnectionUpdateFromServerMove(package.index,package.board_array, package.token_list_id,package.card, package.win_counter);
                        break;
                    case -4:
                        User._instance.RecieveReconnectionUpdateFromServerMoveSetUp(package.index, package.board_array, package.token_list_id, package.card);
                        break;
                    case -5:
                        User._instance.ResumePlay();
                        break;
                    case 3:
                        Debug.Log(package.msg_to_log);
                        User._instance.RecieveUpdateFromServer(package.index, package.board_array, package.card);
                        break;
                    case 5:
                        //End Game
                        GameManager._instance.SetLosePanel();
                        logText.text = "The other player won :(";
                        CloseConnection();
                        break;
                    case 1:
                        Debug.Log(package.msg_to_log);
                        User._instance.RecieveUpdateFromServerSetUp(package.index, package.board_array, package.card);
                        break;
                }
            };
            QueueMainThreadFunction(UpdatePlayer);
            recieveDone.Set();
            logText.text = "RecieveDone.Set()";
        }
        else
        {
            logText.text = "callback close conn";
            close_connection = true;
            recieveDone.Set();
        }
    }

    public void SendPackage()
    {
        byte[] b = Serialize(3, "new move done", User._instance.GetBoard(),-1);
        if (socket.Connected)
        {
            socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SendCallback), socket);
        }
        else
        {
            logText.text = "Socket Isn't Connected";
            Debug.LogWarning("Trying to send data to server but socket isn't connected");
        }
    }

    public void SendSetUpPackage(int index_type)
    {
        byte[] b = Serialize(index_type, "new move done", User._instance.GetBoard(), -1);
        if (socket.Connected)
        {
            socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SendCallback), socket);
        }
        else
        {
            logText.text = "Socket Isn't Connected";
            Debug.LogWarning("Trying to send data to server but socket isn't connected");
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        Debug.Log("Send callback");
    }

    public void SetText(string txt)
    {
        logText.text = txt;
    }

    public void SendWinPackage(int token_id)
    {
        byte[] b = Serialize(4,User._instance.GetBoard(),token_id);
        if (socket.Connected)
        {
            socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SendCallback), socket);
        }
    }
}
