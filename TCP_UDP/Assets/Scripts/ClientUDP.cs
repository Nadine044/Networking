using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
public class ClientUDP : ClientBase
{
    // Start is called before the first frame update
    //IPAddress.Parse("127.0.0.1")

    Socket server;
    //bool testing = false;
    void Start()
    {
        GetComponent<ClientProgram>().closingAppEvent.AddListener(CloseApp);
        StartThreadingFunction(Client);
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
    void Client()
    {
        int count = 0;
        byte[] data = new byte[1024];

        //Creates Sockets
        server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        EndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);

        Action Connected = () => { logControl.LogText("Created Socket", Color.black); };
        QueueMainThreadFunction(Connected);

        string ping = "ping";
        count++;
        data = Encoding.ASCII.GetBytes(ping);


        //Sends ping string to the server & waits for response// the thread blocks here
           
        server.SendTo(data, data.Length, SocketFlags.None, ipep);
        
      
        
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)sender;

        //we receieve the the message from the server
        data = new byte[1024];
        int recv=0;
        bool connected = false;
        while (!connected)
        {
            try
            {
                recv = server.ReceiveFrom(data, ref Remote);
                Debug.Log("First message recieved from server");

                connected = true;
                Action Recieved = () => { logControl.LogText(Encoding.ASCII.GetString(data, 0, recv), Color.black); };
                QueueMainThreadFunction(Recieved);
                Thread.Sleep(500);
            }
            catch (SocketException e)
            {
                Debug.Log("Couldn't recive from server" + e);
                connected = false;
                Action RecievedError = () =>
                {
                    logControl.LogText("Couldn't connect to server" + e, Color.black);
                    logControl.LogText("Try restarting the client & connecting the server first" + e, Color.black);
                };
                QueueMainThreadFunction(RecievedError);
                //Maybe
                server.Close();
                Thread.CurrentThread.Abort();
            }
        }

        // Debug.Log("Message recieved from " + Remote.ToString());
        Debug.Log("Recieved UDP Client " + Encoding.ASCII.GetString(data, 0, recv));
      

        //Until we sent 5 messages to the server the thread will continue running 
        while (count <5)
        {
            count++;

            server.SendTo(Encoding.ASCII.GetBytes(ping), Remote);

            data = new byte[1024];
            recv = server.ReceiveFrom(data, ref Remote);
            Action Recieved = () => { logControl.LogText(Encoding.ASCII.GetString(data, 0, recv), Color.black); };
            QueueMainThreadFunction(Recieved);
            Debug.Log("Recieved UDP Client " + Encoding.ASCII.GetString(data, 0, recv));
            Thread.Sleep(500);

        }

        Action ClosingSocket = () => { logControl.LogText("Closing Socket", Color.black); };
        QueueMainThreadFunction(ClosingSocket);

        server.Close();
        temp_threads.Dequeue();
    }


    public void RestartClient()
    {
        StartThreadingFunction(Client);

    }

    void CloseApp()
    {
        while(temp_threads.Count >0)
        {
            try
            {
                server.Close();
            }
            catch(SystemException e)
            {
                Debug.Log("Couldn't Close socket" + e);
            }
            temp_threads.Dequeue().Abort();
        }

    }


}
