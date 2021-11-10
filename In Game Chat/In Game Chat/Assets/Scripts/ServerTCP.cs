using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;

public class StateObject
{
    // Size of receive buffer.  
    public const int BufferSize = 1024;

    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];

    // Received data string.
    public StringBuilder sb = new StringBuilder();

    // Client socket.
    public Socket workSocket = null;

    public bool endC = false;
}
public class ServerTCP : ServerBase
{
    //TODO use the User class to properly delete threads from list, assing color, etc
    #region UserClass
    class User
    {
        public Color username_color = Color.black;
        public Thread user_thread;
        public string name = string.Empty;
    }
    #endregion
    // Start is called before the first frame update
    readonly int maxClients = 3;


    private int current_clients = 0;


    bool listening = true;
    List<Thread> thread_list = new List<Thread>();

    private static EventWaitHandle wh = new AutoResetEvent(false);

    private static EventWaitHandle recieveDone = new ManualResetEvent(false);

    Socket client;
    Dictionary<Socket,string> users = new Dictionary<Socket,string>();
    //In case of suddenly exiting the App
    ConcurrentQueue<Socket> socket_queue = new ConcurrentQueue<Socket>();
    void Start()
    {
        //Application.quitting += CloseApp;
        GetComponent<ServerProgram>().closingAppEvent.AddListener(CloseApp);
    }

    public void StartServer()
    {
        startThreadingFunction(Server);
    }

    // Update is called once per frame
    void Update()
    {
        //WE execute the functions created in the secondary threads, otherwise we couldn't modify UnityObjects through threads
        while (functionsToRunInMainThread.Count > 0)
        {
            //Grab the first/oldest function in the list
            Action someFunc;
            functionsToRunInMainThread.TryDequeue(out someFunc);

            //Now run it;
            someFunc();
        }

    }


    void Server()
    {
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Any, 27001);

        listener.Bind(ipep);
        listener.Listen(maxClients);
        Debug.Log("Waiting for a client");

        Action WaitingClient = () =>{logControl.LogText("waiting for a client", Color.black);};
        QueueMainThreadFunction(WaitingClient);
        //maybe do a loop here until a maximum capcity of clients has come
        while (listening) //TODO Keep listening while the server is not full or if has been full but the number of clients has decreased
        {
            wh.Reset();

            if (!listening)
                break;

            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            wh.WaitOne();
        }

        //We pause the execution until is resumed once all the clients are done
       // wh.WaitOne();
        
        Action ClosingServer = () =>
        {
            logControl.LogText("Closing server", Color.black);
        };
        QueueMainThreadFunction(ClosingServer);

        listener.Close();
        Debug.Log("Closing server");


    }
    public  void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  

        // Get the socket that handles the client request.  
        if (!listening)
        {
            wh.Set();
            return;
        }

        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        //CRASH When closes server application --> check 

        Thread t = new Thread(HandleClient);
        t.Start(handler);
        thread_list.Add(t);

        wh.Set();

    } //Keep Listening


    void HandleClient(object c)
    {
        Socket handler = (Socket)c;

        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;
        while (!state.endC) //aixo peta
        {
            recieveDone.Reset();
            if (state.endC)
                break;

            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
               new AsyncCallback(ReadCallback), state);
            //while (handler.Available == 0)
            //{
            //    Thread.Sleep(1);
            //}
            //CHECK THIS KEEP ALIVE SOCKET

            //https://stackoverflow.com/questions/722240/instantly-detect-client-disconnection-from-server-socket
            recieveDone.WaitOne();
           // break;
        }

        Debug.Log("closing handler");
        handler.Close();

         
    }
    public  void ReadCallback(IAsyncResult ar)
    {
        string content = string.Empty;

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        

        // Read data from the client socket.


        int bytesRead = state.workSocket.EndReceive(ar);

        if(bytesRead ==0)
        {
            Debug.Log(state);
        }


        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.  
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            content = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
            Debug.Log(content);





            if (content.IndexOf("__END") > -1)
            {
                //Al data has been read 
                Message msg = Deserialize(state.buffer);

                if (!users.ContainsKey(state.workSocket)) //Chekc if there is already added the client to dictionary
                    users.Add(state.workSocket, msg.name_);


                string s = msg.name_ + ": " + msg.message;
                // Check for end-of-file tag. If it is not there, read
                // more data.  
                Action RecieveMsg = () => { logControl.LogText(s, Color.black); };
                QueueMainThreadFunction(RecieveMsg);

                //Send message to the rest of clients
                foreach (KeyValuePair<Socket, string> entry in users)
                {
                    if (entry.Key != state.workSocket)
                    {
                        Send(entry.Key, state.buffer);
                    }

                }
                recieveDone.Set();
            }
            else
            {
                state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }



        }
        else
        {
            //Here pasess a lot of times sometimes with the same user //TO FIX
            string disconnecting_user_name="";
            users.TryGetValue(state.workSocket, out disconnecting_user_name);
            users.Remove(state.workSocket);

            byte[] b = Encoding.ASCII.GetBytes("User " + disconnecting_user_name + " disconnected");
            //Send message to all other clients that someone has disconnected
            foreach (KeyValuePair<Socket, string> entry in users)
            {
                    Send(entry.Key, b);              
            }

            Action RecieveMsg = () => { logControl.LogText("User " +disconnecting_user_name + " disconnected", Color.black); };
            QueueMainThreadFunction(RecieveMsg);

            //close socket with client
            state.workSocket.Close();


            Debug.Log("Something Happened, didnt recieved any bytes");
            state.endC = true;

            //Remove thread from list
            thread_list.Remove(Thread.CurrentThread);

            //
            Thread.CurrentThread.Abort();

        }
        



    }

    private static void Send(Socket handler, byte[] data)
    {
        // Convert the string data to byte data using ASCII encoding.  


        // Begin sending the data to the remote device.  
        handler.BeginSend(data, 0, data.Length, 0,
            new AsyncCallback(SendCallback), handler);

        //handler.Send(data);
    }

    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        try
        {
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }
        catch(SystemException e)
        {
            Debug.LogWarning(e);
        }
    }
    private static void SendCallback(IAsyncResult ar)
    {
        Debug.Log("Here");
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
           Debug.Log("Sent " + bytesSent +" bytes to client.");


        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }





    byte[] Serialize(string message,string client_name)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(client_name);
        writer.Write(message);
        writer.Write("__END");
        byte[] b = stream.GetBuffer();
        //writer.Close();
        stream.Close();
        GC.SuppressFinalize(stream);
        return b;
    }

    Message Deserialize(byte[] data)
    {
        Message msg = new Message();
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        msg.name_ = reader.ReadString();
        msg.message = reader.ReadString();
        msg.finalofmsg = reader.ReadString();
      //  reader.Close();
        stream.Close();
        //GC.SuppressFinalize(stream);
        return msg;
    }
    void CloseApp()
    {
        //abort all the threads
        foreach(Thread t in thread_list)
        {
            t.Abort();
        }

        //Empty the thread list
        thread_list.Clear();

        //close the main socket
        try
        {
            listener.Close();
        }
        catch (SocketException e)
        {
            Debug.Log("Couldn't Close socket while exiting info" + e);
        }


        //TODO CHECK THIS
        if (temp_thread.ThreadState == ThreadState.Running)
            temp_thread.Abort();


        current_clients = 0;
        listening = false;
        wh.Set();

    }

}