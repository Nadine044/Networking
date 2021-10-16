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

    protected Thread temp_thread;
    protected void startThreadingFunction(Action someFunction)
    {
        temp_thread = new Thread(someFunction.Invoke);
        temp_thread.Start();

    }

    public void QueueMainThreadFunction(Action someFunction)
    {
        //We need to make sure that some function is running from the main Thread

        //someFunction(); //This isn't okay, if we're in a child thread
        functionsToRunInMainThread.Enqueue(someFunction);
    }

    public void ClearLog()
    {
        foreach (GameObject go in logControl.textItems)
        {
            Destroy(go);
        }
        logControl.textItems.Clear();
    }

}
