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
    public static NetworkingClient _instance { get; private set; }

    private static ManualResetEvent recieveDone = new ManualResetEvent(false);

    bool close_connection = false;

    public Text logText;
    //int client = 0;
    // Start is called before the first frame update
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
    // Update is called once per frame
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
        QueueMainThreadFunction(s);
        //Here tell player he is connected And launch and open UpdateConnectionWith server
        StartThreadingFunction(UpdatingConnection);
    }

    void CloseConnection()
    {
        Debug.Log("Closing connection");
        Action s = () =>
        {
            logText.text = "closing connection <CloseConnection()>";
        };
        QueueMainThreadFunction(s);
        try
        {
            if(socket.Connected)
                socket.Shutdown(SocketShutdown.Both);

            socket.Close();
        }
        catch(SocketException e)
        {
            Debug.LogWarning(e);
        }
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

            //else
            //    break;

            recieveDone.WaitOne();
        }

        CloseConnection();
    }

   
    void ReadCallback(IAsyncResult ar)
    {
        Debug.Log("Read Callback");
        Action w = () =>
        {
            logText.text = "ReadCallback <ReadCallback()>";
        };
        QueueMainThreadFunction(w);
        OBJ obj = (OBJ)ar.AsyncState;

        int bytesread = 0;

        bytesread = socket.EndReceive(ar);
        //PETA AQUI, mirar com cancelar el begin recieve quan s'ha tancat el socket...
        if(bytesread >0)
        {
            Package package = Deserialize(obj.buffer);

            int turnstep = package.index;
            int[] board_tmp = package.board_array;
            Debug.Log(package.msg_to_log);


            Action UpdatePlayer = () =>
            {
                Debug.Log("Updating player");
                Player._instance.RecieveUpdateFromServer(turnstep, board_tmp, package.card);
                logText.text = "Updating player <ReadCallback()>";
            };
            QueueMainThreadFunction(UpdatePlayer);

            recieveDone.Set();
            logText.text = "RecieveDone.Set()";
            //need to know which is which
        }
        else
        {
            logText.text = "callback close conn";
            //close connection
            close_connection = true;
            recieveDone.Set();
        }
    }

   

    public void SendPackage()//TODO
    {
        byte[] b = Serialize(3, "new move done", Player._instance.GetBoard(),false,-1);
        socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SendCallback), socket);
    }

    public void SendSetUpPackage()
    {
        byte[] b = Serialize(1, "new move done", Player._instance.GetBoard(), false, -1);
        socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SendCallback), socket);
    }

    private void SendCallback(IAsyncResult ar)
    {
        Debug.Log("Send callback");
    }

    public void SetText(string txt)
    {
        logText.text = txt;
    }
}
