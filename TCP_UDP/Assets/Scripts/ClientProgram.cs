using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Events;

public class ClientProgram : MonoBehaviour
{
    enum CLIENT_TYPE
    {
        NONE,
        TCP,
        UDP
    }

    [SerializeField]
    List<GameObject> UI_TextLog;

    [SerializeField]
    GameObject starterPanel;

    [SerializeField]
    private GameObject restartUDPClient;

    public UnityEvent closingAppEvent;
    CLIENT_TYPE client_type = CLIENT_TYPE.NONE;
    // Start is called before the first frame update
    void Start()
    {
        if (closingAppEvent == null)
            closingAppEvent = new UnityEvent();

        restartUDPClient.SetActive(false);

        foreach (GameObject go in UI_TextLog)
        {
            go.SetActive(false);
        }
    }


    private void Update()
    {
        //Invoke Event and makes sure every thread and socket has ended/closed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            closingAppEvent.Invoke();
            Application.Quit();

        }
    }
    public void StartUDPClient()
    {
        client_type = CLIENT_TYPE.UDP;

        starterPanel.SetActive(false);
        restartUDPClient.SetActive(true);
        foreach (GameObject go in UI_TextLog)
        {
            go.SetActive(true);
        }
        GetComponent<ClientUDP>().enabled = true;
        GetComponent<ClientUDP>().StartClient();


    }
    public void StartSingleTCPClient()
    {
        client_type = CLIENT_TYPE.TCP;

        starterPanel.SetActive(false);
        foreach (GameObject go in UI_TextLog)
        {
            go.SetActive(true);
        }
        GetComponent<ClientTCP>().enabled = true;
        GetComponent<ClientTCP>().SetNClients(1);
        GetComponent<ClientTCP>().StartClient();
    }
    public void StartMultipleTCPClient()
    {
        client_type = CLIENT_TYPE.TCP;

        starterPanel.SetActive(false);
        foreach (GameObject go in UI_TextLog)
        {
            go.SetActive(true);
        }
        GetComponent<ClientTCP>().enabled = true;
        GetComponent<ClientTCP>().SetNClients(3);
        GetComponent<ClientTCP>().StartClient();

    }


    public void BackToMenu()
    {
        closingAppEvent.Invoke();

        foreach (GameObject go in UI_TextLog)
        {
            go.SetActive(false);
        }
        starterPanel.SetActive(true);


        switch (client_type)
        {
            case CLIENT_TYPE.TCP:
                GetComponent<ClientTCP>().ClearLog();
                GetComponent<ClientTCP>().enabled = false;
                break;

            case CLIENT_TYPE.UDP:
                restartUDPClient.SetActive(false);
                GetComponent<ClientUDP>().ClearLog();
                GetComponent<ClientUDP>().enabled = false;
                break;
        }
    }
}
