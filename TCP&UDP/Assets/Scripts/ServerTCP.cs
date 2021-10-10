using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
public class ServerTCP : MonoBehaviour
{
    // Start is called before the first frame update
    readonly int maxClients = 1;
    private Socket _socket;
    private IPEndPoint ipep;

    private int current_clients = 0;
    bool listening = true;

    Queue<Thread> thread_queue = new Queue<Thread>();
    void Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Any, 27000);

        startThreadingFunction(MultipleServer);

    }

    // Update is called once per frame
    void Update()
    {
        if(listening && current_clients == maxClients)
        {
            listening = false;
        }

        if (thread_queue.Count > 0 && !thread_queue.Peek().IsAlive) //fails the first must be alread started
        {
            thread_queue.Dequeue();
            Debug.Log("thread dequeued");
            if (thread_queue.Count > 0)
                thread_queue.Peek().Start();
        }


    }


    //Maybe use thread pool
    void MultipleServer()
    {
        Socket client=null;

        _socket.Bind(ipep);
        _socket.Listen(maxClients);
        Debug.Log("Waiting for a client");
        //maybe do a loop here until a maximum capcity of clients has come
        while (listening)
        {

            client = _socket.Accept();

            //  ThreadPool.QueueUserWorkItem(ClientHandler, client); //Need to use some thread at a time
            //Thread.Sleep(500);

            thread_queue.Enqueue(new Thread(() => ClientHandler(client)));
            if (thread_queue.Count == 1)
                thread_queue.Peek().Start();


           // new Thread(() => ClientHandler(client)).Start();
        }

        
        Debug.Log("Closing server");
        _socket.Close();
    }


    //async void ManageClientsQueue()
    //{
    //    //for(int i =0; i <= thread_queue.Count; i++)
    //    //{
    //    //    thread_queue.GetEnumerator().Current.Start();
    //    //    if(thread_queue.)
    //    //}
    //    while(thread_queue.Count !=0)
    //    {
    //        thread_queue.GetEnumerator().Current.Start();
    //        if (thread_queue.)
    //    }
    //}

    void ClientHandler(object c)
    {
        current_clients++;
        Debug.Log("Client accepted" + current_clients);

        Socket client = (Socket)c;
        IPEndPoint client_ep;
        string pong = "pong";
        byte[] data = new byte[1024];
        int count = 0;
        int recv = 0;

        client_ep = (IPEndPoint)client.RemoteEndPoint;
        Debug.Log("Connected: " + client_ep.ToString());
        data = Encoding.ASCII.GetBytes(pong);
        client.Send(data, data.Length, SocketFlags.None);
        Debug.Log("Send server " + Encoding.ASCII.GetString(data));
        count++;

        Thread.Sleep(2000);

        while(count < 5)
        {
            count++;

            data = new byte[1024];
            recv = client.Receive(data);
            Debug.Log("Recieved server " + Encoding.ASCII.GetString(data));
            Thread.Sleep(2000);
            client.Send(Encoding.ASCII.GetBytes(pong));
            Debug.Log("Send server" + Encoding.ASCII.GetString(data, 0, recv));
        }
        client.Close();
    }

    public void startThreadingFunction(Action function)
    {
        Thread t = new Thread(function.Invoke);
        t.Start();
    }
    ////for now programmed for only 1 connection
    //void Server() //Need to polish the use of Threads
    //{
    //    IPEndPoint client_ep = null;
    //    bool connected = false;
    //    int count=0;
    //    byte[] data = new byte[1024];
    //    int recv=0;
    //    Socket client = null;
    //    string pong = "pong";
    //    try
    //    {
    //        _socket.Bind(ipep);
    //        _socket.Listen(maxClients);
    //        Debug.Log("Waiting for a client");
    //        client = _socket.Accept(); //Blocking execution
    //       // ClientHandler(client);

    //        client_ep = (IPEndPoint)client.RemoteEndPoint;
    //        Debug.Log("Connected: " + client_ep.ToString());
    //        connected = true;
    //        count++;
    //        //Send Pong Message
    //        data = Encoding.ASCII.GetBytes(pong);
    //        client.Send(data, data.Length, SocketFlags.None);
    //        Thread.Sleep(500);
    //    }
    //    catch(Exception e)
    //    {
    //        Debug.LogError("Connection failed..." + e.ToString());
    //        Thread.CurrentThread.Abort();

    //    }
    //    while (count <5 && connected)
    //    {
    //        count++;
    //        data = new byte[1024];

    //        if (client != null)
    //        {
    //            recv = client.Receive(data);
    //            Thread.Sleep(500);
    //            client.Send(Encoding.ASCII.GetBytes(pong));
    //            Debug.Log(Encoding.ASCII.GetString(data,0,recv));
    //        }

    //    }

    //    Debug.Log("Disconnected from" + client_ep.Address);
    //    client.Close();
    //    _socket.Close();

    //}


}
