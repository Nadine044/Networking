using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System;
public class NetworkingClient : Networking
{
    // Start is called before the first frame update
    void Start()
    {
        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 26002);
    }
    void ConnectToServer()
    {
        socket.BeginConnect(ipep, new AsyncCallback(ConnectCallback), socket);
    }
    private void OnConnectedToServer()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void ConnectCallback(IAsyncResult ar)
    {
        //Here tell player he is connected And launch and open UpdateConnectionWith server
        StartThreadingFunction(UpdatingConnection);
    }


    void UpdatingConnection()
    {

    }
}
