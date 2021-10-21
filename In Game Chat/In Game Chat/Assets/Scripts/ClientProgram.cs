using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClientProgram : MonoBehaviour
{
    string client_name;
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
    GameObject warningPanel;

    public UnityEvent closingAppEvent;
    CLIENT_TYPE client_type = CLIENT_TYPE.NONE;
    // Start is called before the first frame update
    void Start()
    {
        if (closingAppEvent == null)
            closingAppEvent = new UnityEvent();


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

    public void StartSingleTCPClient()
    {
        if(client_name == null)
        {
            warningPanel.SetActive(true);
            return;
        }

        client_type = CLIENT_TYPE.TCP;

        starterPanel.SetActive(false);
        foreach (GameObject go in UI_TextLog)
        {
            go.SetActive(true);
        }
        GetComponent<ClientTCP>().enabled = true;
        GetComponent<ClientTCP>().SetClientName(client_name);
        GetComponent<ClientTCP>().SetNClients(1);
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

        }
    }

    public void ReadStringInput(string s)
    {
        client_name = s;
        Debug.Log(s);
    }
}
