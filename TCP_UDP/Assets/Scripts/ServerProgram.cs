using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
public class ServerProgram : MonoBehaviour
{
   protected Queue<Action> functionsToRunInMainThread;


    // Start is called before the first frame update
    void Start()
    {
        for(int i =0; i <3; i++)
        {
            GameObject go = new GameObject();
            go.AddComponent<ClientTCP>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        while (functionsToRunInMainThread.Count > 0)
        {
            //Grab the first/oldest function in the list
            Action someFunc = functionsToRunInMainThread.Peek();
            functionsToRunInMainThread.Dequeue();

            //Now run it;
            someFunc();
        }
    }


    protected void startThreadingFunction(Action someFunction)
    {
        Thread t = new Thread(someFunction.Invoke);
        t.Start();
    }
}
