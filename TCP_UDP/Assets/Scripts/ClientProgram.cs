using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
public class ClientProgram : MonoBehaviour
{

    [SerializeField]
    List<GameObject> UI_to_hide;

    [SerializeField]
    GameObject starterPanel;

    [SerializeField]
    private GameObject restartUDPClient;


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


}
