using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
public class ClientUDP : MonoBehaviour
{
    // Start is called before the first frame update
    //IPAddress.Parse("127.0.0.1")

    private Socket server;
    byte[] data = new byte[1024];

    //bool testing = false;
    void Start()
    {

        //c.Client("127.0.0.1", 27000);
        startThreadingFunction(ExampleClient);
    }

    public void startThreadingFunction(Action function)
    {
        Thread t = new Thread(function.Invoke);
        t.Start();
    }


    //Maybe not do everything in the thread only the blocking things
    void ExampleClient()
    {
        int count = 0;

        //Creates Sockets
        server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        EndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);

        string ping = "ping";
        count++;
        data = Encoding.ASCII.GetBytes(ping);
        //Sends ping string to the server & waits for response// the thread blocks here
        server.SendTo(data, data.Length, SocketFlags.None, ipep);

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)sender;

        //we receieve the the message from the server
        data = new byte[1024];
        int recv = server.ReceiveFrom(data, ref Remote);

       // Debug.Log("Message recieved from " + Remote.ToString());
        Debug.Log("Recieved UDP Client " + Encoding.ASCII.GetString(data, 0, recv));
        Thread.Sleep(500);

        //Until we sent 5 messages to the server the thread will continue running 
        while (count <5)
        {
            count++;

            server.SendTo(Encoding.ASCII.GetBytes(ping), Remote);

            data = new byte[1024];
            recv = server.ReceiveFrom(data, ref Remote);
            Debug.Log("Recieved UDP Client " + Encoding.ASCII.GetString(data, 0, recv));
            Thread.Sleep(500);

        }

        server.Close();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.Space) && !testing)
    //    {
    //        testing = true;
    //        startThreadingFunction(ExampleClient);
    //    }
    //    if(Input.GetKeyDown(KeyCode.Escape) && testing)
    //    {
    //        Application.Quit();
    //    }
       
    //}

    
    
}
