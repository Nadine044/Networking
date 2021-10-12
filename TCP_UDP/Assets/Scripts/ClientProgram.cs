using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
public class ClientProgram : MonoBehaviour
{

    protected Queue<Action> functionsToRunInMainThread = new Queue<Action>();
    protected string CurrentLog;
    [SerializeField]
    List<GameObject> UI_to_hide;

    [SerializeField]
    GameObject starterPanel;
    [SerializeField]
    private GameObject restartUDPClient;

    [SerializeField]
    protected TextLogControl logControl;

    // Start is called before the first frame update
    void Start()
    {
        restartUDPClient.SetActive(false);

        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(false);
        }
    }

  

    public void StartUDPClient()
    {
        starterPanel.SetActive(false);
        restartUDPClient.SetActive(true);
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(true);
        }
        this.GetComponent<ClientUDP>().enabled = true;

    }
    public void StartSingleTCPClient()
    {
        starterPanel.SetActive(false);
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(true);
        }
        this.GetComponent<ClientTCP>().enabled = true;
        this.GetComponent<ClientTCP>().SetNClients(1);
    }
    public void StartMultipleTCPClient()
    {
        starterPanel.SetActive(false);
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(true);
        }
        this.GetComponent<ClientTCP>().enabled = true;
        this.GetComponent<ClientTCP>().SetNClients(3);
    }

    protected void StartThreadingFunction(Action function)
    {
        Thread t = new Thread(function.Invoke);
        t.Start();
    }

    public void QueueMainThreadFunction(Action someFunction)
    {
        //We need to make sure that some function is running from the main Thread

        //someFunction(); //This isn't okay, if we're in a child thread
        functionsToRunInMainThread.Enqueue(someFunction);
    }
}
