using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class ThreadingExample : MonoBehaviour
{
    //Remember we can't modify Unity object inside threads like a Transform
    //but we can extract their values modify them in a thread and then assign later

    //Know using Actions and List we can Modify Unity Objects, what we actually can't do is read from them in the secondary thread
    //for that we will need a lock to have a bundle of data safely

    //What we do here is pass a function that we can't run on a secondary theard 
    //and we copy it to a list that will be iterated on the main thread so our function in the end will run on the main thread

    List<Action> functionsToRunInMainThread;
    string text = "que pasa";

    object FrontDoor = new object();

    private void Start()
    {
        Debug.Log("Start() -- Started");
        functionsToRunInMainThread = new List<Action>();

        Thread t2 = new Thread(new ThreadStart(ExampleLock));
        t2.Start();

        startThreadingFunction( SlowfunctionThatDoesUnityThing);

    }

    void ExampleLock()
    {
        Debug.Log("ExampleLock -- Started");
        //If this is locked when in other thread it reaches the lock it will wait until this lock is finished
        lock(text)
        {
            text = "lock_ex";
            Debug.Log(text);
        }
        Debug.Log("ExampleLock -- Ended");

    }
    private void Update()
    {
     //Update() always runs in the main thread
     
        while(functionsToRunInMainThread.Count > 0)
        {
            //Grab the first/oldest function in the list
            Action someFunc = functionsToRunInMainThread[0];
            functionsToRunInMainThread.RemoveAt(0);

            //Now run it;
            someFunc();
        }
    }

    //Action is similar as a delegate, isn't generic so we can don Action<int> for instance
    public void startThreadingFunction(Action someFunction)
    {
        //Two ways to Assing a thread constructec with an Action
        //1
        Thread t = new Thread(someFunction.Invoke);
        t.Start();

        //2
        //Thread t2 = new Thread(new ThreadStart(someFunction));
        //t2.Start();
    }

    public void QueueMainThreadFunction(Action someFunction)
    {
        //We need to make sure that some function is running from the main Thread

        //someFunction(); //This isn't okay, if we're in a child thread
        functionsToRunInMainThread.Add(someFunction);
    }
    void SlowfunctionThatDoesUnityThing()
    {
        //first we do a reallly slow thing
       // Thread.Sleep(2000);

        //Now we need to modify a Unity gameObject
        Action aFunciton = () => {
            Debug.Log("The results of the child thread are being applied to a Unity GameObject Safely");
            this.transform.position = new Vector3(1, 1, 1); //NOT ALLOWED FROM A CHILD THREAD
        };

        lock (text) //if text is already locked, the thread will PAUSE until text is not locked
        {
            text = "slow function";
            Debug.Log(text);
        }
        //NOTE: We still aren't allowed to call this form a child thread
        //aFunciton();

        QueueMainThreadFunction(aFunciton); //I need you to run this function on the main thread
    }


   
}
