using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
public class ServerBase : MonoBehaviour
{
    protected Socket _socket;
    protected IPEndPoint ipep;

    protected Queue<Action> functionsToRunInMainThread = new Queue<Action>();
    protected string CurrentLog;

    [SerializeField]
    protected TextLogControl logControl;

    protected void startThreadingFunction(Action someFunction)
    {
        Thread t = new Thread(someFunction.Invoke);
        t.Start();

    }

    public void QueueMainThreadFunction(Action someFunction)
    {
        //We need to make sure that some function is running from the main Thread

        //someFunction(); //This isn't okay, if we're in a child thread
        functionsToRunInMainThread.Enqueue(someFunction);
    }

}
