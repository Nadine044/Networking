using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ClientBase : MonoBehaviour
{
    protected Queue<Action> functionsToRunInMainThread = new Queue<Action>();
    protected string CurrentLog;


    [SerializeField]
    protected TextLogControl logControl;

    [SerializeField]
    protected TextLogControl users_log;

    protected void StartThreadingFunction(Action someFunction)
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

    public void ClearLog()
    {

        foreach(GameObject go in logControl.textItems)
        {
            Destroy(go);
        }
        logControl.textItems.Clear();
    }
}
