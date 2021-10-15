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
    List<GameObject> UI_to_hide;

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

        foreach (GameObject go in UI_to_hide)
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


    public void BackToMenu()
    {
        closingAppEvent.Invoke();

        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(false);
        }
        starterPanel.SetActive(true);


        switch (client_type)
        {
            case CLIENT_TYPE.TCP:
                GetComponent<ClientTCP>().enabled = false;
                break;

            case CLIENT_TYPE.UDP:
                restartUDPClient.SetActive(false);
                GetComponent<ClientUDP>().enabled = false;
                break;
        }
    }
}
