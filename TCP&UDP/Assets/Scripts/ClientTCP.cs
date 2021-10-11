using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
public class ClientTCP : MonoBehaviour
{
    string ping = "ping";
    // Start is called before the first frame update
    public void Start()
    {
        StartThreadingFunction(Client);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartThreadingFunction(Action function)
    {
        Thread t = new Thread(function.Invoke);
        t.Start();
    }
    void Client()
    {
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);

        byte[] data = new byte[1024];
        int count = 0;
        bool connected = false;

        //tries to connect all the time to the server until the client achieve it
        while (!connected)
        {
            try
            {
                server.Connect(ipep);
                connected = true;
                Debug.Log("connected to server");
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode.Equals(10035))
                {
                    Debug.LogWarning("Still Connected, but the Send would block");
                }
                else
                {
                    Debug.LogWarning("Disconnected: error code "+ e.NativeErrorCode);
                }
                Debug.LogWarning("Unable to connect to server  " + e.ToString());

            }

        }

        //Sends the first ping or message to the server
        try
        {
            server.Send(Encoding.ASCII.GetBytes(ping));
            //Debug.Log("Send  client ping ");
        } catch(SocketException e)
        {
            Debug.LogWarning(e.SocketErrorCode);
            
        }

        int recv;
        //Recieves the first message from the server & waits for 500ms
        try
        {
            recv = server.Receive(data);
            Debug.Log("Recieved  Client" + Encoding.ASCII.GetString(data, 0, recv)); 
            Thread.Sleep(500);
        }
        catch(SystemException e)
        {
            Debug.LogWarning("Client coulnd't recieve from server " + e);
        }
        
        count++;
        while (count < 5)
        {
            try //Sends message to server
            {
                count++;
                server.Send(Encoding.ASCII.GetBytes(ping));
                //Debug.Log("Send client ping");
            }
            catch(SystemException e)
            {
                Debug.LogWarning("Client Couldn't send message to server " + e);
                break;
            }


            data = new byte[1024];
            try //Recieves message from server & waits for 500ms
            {
                recv = server.Receive(data);
                Debug.Log("Recived client " + Encoding.ASCII.GetString(data, 0, recv)); //Crashes here in the last update
                Thread.Sleep(500);
            }
            catch (SystemException e)
            {
                Debug.LogWarning("Couldn't recieve from server " + e);
            }

        }

        Debug.Log("Disconnecting From server");
        try
        {
            server.Shutdown(SocketShutdown.Both);
        }
        catch(SystemException e)
        {
            Debug.LogWarning("Couldn't shutdown the server, socket already closed " +e);
        }
        server.Close();
    }
}
