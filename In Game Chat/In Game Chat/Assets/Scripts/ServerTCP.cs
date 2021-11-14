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


public class ServerTCP : ServerBase
{
    //TODO use the User class to properly delete threads from list, assing color, etc
    #region UserClass
    class User
    {
        public Color username_color = Color.black;
        public Thread user_thread;
        public string name = string.Empty;
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket socket = null;

        public bool end_connection = false;

        public EventWaitHandle recieveDone = new ManualResetEvent(false);
        KeyValuePair<int, string> colors_username;
    }
    #endregion

    //RichTexts default string
    //Example: We are <color = green>green</color> with envy
    string r1 = "<color=";
    string r2 = ">";
    string r3 = "</color>";

    // Start is called before the first frame update
    readonly int maxClients = 3;


    private int current_clients = 0;


    bool listening = true;
    List<Thread> thread_list = new List<Thread>();

    private static EventWaitHandle wh = new AutoResetEvent(false);

    //private static EventWaitHandle recieveDone = new ManualResetEvent(false);

    //In case of suddenly exiting the App
    ConcurrentQueue<Socket> socket_queue = new ConcurrentQueue<Socket>();
    List<User> users_list = new List<User>();
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
        ipep = new IPEndPoint(IPAddress.Any, 27011);

        listener.Bind(ipep);
        listener.Listen(maxClients);
        Debug.Log("Waiting for a client");

        Action WaitingClient = () =>{logControl.LogText("waiting for a client", Color.black);};
        QueueMainThreadFunction(WaitingClient);


        //maybe do a loop here until a maximum capcity of clients has come
        while (listening) //TODO Keep listening while the server is not full or if has been full but the number of clients has decreased
        {
            //Point where the EventWaitHandle restarts the execution
            wh.Reset();

            //neded to exit the loop
            if (!listening)
                break;

            //Async accept incoming socket
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            //listener.BeginAccept(null, 256, new AsyncCallback(AcceptCallback), listener);

            //EventWaintHandle stops the execution of this thread until is changed from another thread
            wh.WaitOne();
        }

        //We pause the execution until is resumed once all the clients are done
       // wh.WaitOne();
        
        Action ClosingServer = () =>
        {
            logControl.LogText("Closing server", Color.black);
        };
        QueueMainThreadFunction(ClosingServer);

        //close listener socket
        listener.Close();
        Debug.Log("Closing server");


    }
    public  void AcceptCallback(IAsyncResult ar)
    {




        if (!listening)
        {
            //Resume the EventWaitHandle to keep exit the recieving client loop
            wh.Set();
            return;
        }


       //Cast the object callback to a socket
        Socket listener = (Socket)ar.AsyncState;

        //TODO HOW TO GET MULTIPLE CONNECTIONS with bytes in client acceptance
        //byte[] buffer;
        //int bytesTransferred;
        ////Asynchronously accepts an incoming connection attempt.
        //Socket tmp_handler = listener.EndAccept(out buffer,out bytesTransferred,ar);
        //string requesting_user_name = Encoding.ASCII.GetString(buffer, 0, bytesTransferred);
        //Debug.Log(requesting_user_name);

        ////If the new user name matches some name of the current users its request is dennied;
        //foreach (User u in users_list)
        //{
        //    if(requesting_user_name == u.name)
        //    {
        //        //TODO SEND A MSG BACK TO CLIENT SAYING WHY ACCES IS DENIED
        //        Debug.Log("new user trying to connect with the name " + requesting_user_name + " that matches another client name");
        //        tmp_handler.Close();
        //        wh.Set();                              ---------
        //        return;                                ---------
        //    }                                          ---------
        //}                                              ---------
        //        user.socket = tmp_handler;             ---------
        //.-------------------------------------------------------
        //.-------------------------------------------------------


        User user = new User();
        //Asynchronously accepts an incoming connection attempt.
        user.socket = listener.EndAccept(ar);
        users_list.Add(user);

        byte[] b = Serialize("welcome to the server","Server","MSG");
        Send(user.socket, b);
        //Starts a thread to handle the incoming client
        user.user_thread = new Thread(HandleClient);
        user.user_thread.Start(user);

        //Quarantine check later
        thread_list.Add(user.user_thread);

        //Resume the EventWaitHandle to keep recieving incoming clients in the Server() function 
        wh.Set();

    } 


    void HandleClient(object c)
    {
        User user = (User)c;


        while (!user.end_connection) //aixo peta
        {
            //Point where the EventWaitHandle restarts the execution
            user.recieveDone.Reset();

            //to exit the loop
            if (user.end_connection)
                break;

            //Async recieve bytes from the client
            user.socket.BeginReceive(user.buffer, 0, User.BufferSize, 0,
               new AsyncCallback(ReadCallback), user);


            //https://stackoverflow.com/questions/722240/instantly-detect-client-disconnection-from-server-socket
            user.recieveDone.WaitOne();
        }

        Debug.Log("closing handler");
        user.socket.Close();
        user = null;      
    }
    public  void ReadCallback(IAsyncResult ar)
    {

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        User user = (User)ar.AsyncState;
        

        // Read data from the client socket.


        int bytesRead = user.socket.EndReceive(ar);



        if (bytesRead > 0)
        {

            // There  might be more data, so store the data received so far.  
            user.sb.Append(Encoding.ASCII.GetString(
                user.buffer, 0, bytesRead));//These may be wrong case we process twice the data the first in append second in deserialize

            string content = string.Empty;
            content = Encoding.ASCII.GetString(user.buffer, 0, bytesRead);
            Debug.Log(content);




            // Check for end-of-file tag. If it is not there, read more data.  
            if (content.IndexOf("__END") > -1)
            {
                //Al data has been read 
                Message msg = Deserialize(user.buffer);

                //Temporal until we solve the beginconnect error ----------------------------
                //---------------------------------------------------------------------------
                if (user.name == string.Empty)
                    user.name = msg.name_;

                foreach(User u in users_list)
                {
                    if(u != user && u.name == user.name)
                    {
                        EventWaitHandle exit_server = new ManualResetEvent(false);

                        byte[] bytesback = Encoding.ASCII.GetBytes("Name already used please restart client and change name");
                        user.socket.BeginSend(bytesback, 0, bytesback.Length,0, new AsyncCallback(AccesDenied),user.socket);
                        Debug.Log("Bye bye");
                        user.socket.Close();
                        user.end_connection = false;
                        users_list.Remove(user);
                        user.user_thread.Abort();
                        user.recieveDone.Set();
                    }
                }
                //TEMPORAL-------------------------------------------
                //TEMPORAL-------------------------------------------
                //if (!users_dictionary.ContainsKey(user.socket)) //Chekc if there is already added the client to dictionary
                //    users_dictionary.Add(user.socket, msg.name_);


                //Check for commands
                if (msg.prefix != "MSG")
                {
                    CommandHandler(msg);
                    user.recieveDone.Set();
                    return;
                }

                string s = msg.name_ + ": " + msg.message;

                Action RecieveMsg = () => { logControl.LogText(s, Color.black); };
                QueueMainThreadFunction(RecieveMsg);


                byte[] bytestoSend = Serialize(msg.message, user.name,"MSG");

                //Send message to the rest of clients
                foreach (User u in users_list)
                {
                    if(u != user)//Check
                    {
                        Send(u.socket, bytestoSend);
                    }
                }
                //maybe we have to empty the buffer here
                user.recieveDone.Set();

            }
            //keeps recieving the rest of the message
            else 
            {
                user.socket.BeginReceive(user.buffer, 0, User.BufferSize, 0, new AsyncCallback(ReadCallback), user);
            }

        }
        else
        {
            //Here pasess a lot of times sometimes with the same user //TO FIX
            users_list.Remove(user);

            byte[] b = Serialize("User " + user.name + " disconnected", user.name,"MSG");
            
            //Send message to all other clients that someone has disconnected
            foreach (User u in users_list)
            {
                Send(u.socket, b);
            }


            Action RecieveMsg = () => { logControl.LogText("User " +user.name + " disconnected", Color.black); };
            QueueMainThreadFunction(RecieveMsg);

            //close socket with client
            user.socket.Close();


            Debug.Log("Something Happened, didnt recieved any bytes");
            user.end_connection = true;

            //Remove thread from list
           // thread_list.Remove(Thread.CurrentThread);

            user.user_thread.Abort();

        }
    }

    private void CommandHandler(Message msg)
    {
        switch(msg.prefix)
        {
            case "BAN":
                BanUser(msg.message, msg.name_);
                break;
        }
    }

    private void BanUser(string message, string name_)
    {
        User user = null;
        foreach (User u in users_list)
        {
            if (u.name == message && u.name != name_)
            {
                //Ban user
                user = u;
                break;
            }
        }
        if(user!=null)
        {
            users_list.Remove(user);

            byte[] b = Serialize(user.name + " has been kicked out by " + name_, "Server", "MSG");
            foreach(User u in users_list)
            {
                Send(u.socket, b);
            }

            user.socket.Close();

            Action RecieveMsg = () => { logControl.LogText(user.name + " has been kicked out by " + name_, Color.black); };
            QueueMainThreadFunction(RecieveMsg);

            user.end_connection = true;

            //user.user_thread.Abort();
        }
    }

    void AccesDenied(IAsyncResult ar)
    {
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





    byte[] Serialize(string message,string client_name,string prefix)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(prefix);
        writer.Write(client_name);
        writer.Write(message);
        writer.Write(users_list.Count);
        foreach (User u in users_list)
        {
                writer.Write(u.name);
        }
        
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
        msg.prefix = reader.ReadString();
        msg.name_ = reader.ReadString();
        msg.message = reader.ReadString();
        msg.n_users = reader.ReadInt32();
        for(int i =0; i< msg.n_users; i++)
        {
            msg.current_users.Add(reader.ReadString());
        }
        msg.finalofmsg = reader.ReadString();
        Debug.Log(msg.n_users);
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
    Dictionary<int, string> colors = new Dictionary<int, string>()
    {
        {1, "aqua"},
        {2, "black"},
        {3, "brown"},
        {4, "cyan"},
        {5, "darkblue"},
        {6, "fuchsia"},
        {7, "green"},
        {8, "grey"},
        {9, "lightblue"},
        {10, "lime"},
        {11, "magenta"},
        {12, "maroon"},
        {13, "navy"},
        {14, "olive"},
        {15, "orange"},
        {16, "purple"},
        {17, "purple"},
        {18, "red"},
        {19, "silver"},
        {20, "teal"},
        {21, "white"},
        {22, "yellow"},
    };
}
