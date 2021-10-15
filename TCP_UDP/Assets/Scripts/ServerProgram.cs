using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class ServerProgram : MonoBehaviour
{
    enum SERVER_TYPE
    {
        NONE,
        TCP,
        UDP
    }

    [SerializeField]
    List<GameObject> UI_to_hide;

    [SerializeField]
    GameObject starterPanel;

    public UnityEvent closingAppEvent;


    SERVER_TYPE server_type = SERVER_TYPE.NONE;
    // Start is called before the first frame update
    void Start()
    {
        if (closingAppEvent == null)
            closingAppEvent = new UnityEvent();

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

    public void StartUDPServer()
    {
        server_type = SERVER_TYPE.UDP;
        starterPanel.SetActive(false);
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(true);
        }
        this.GetComponent<ServerUDP>().enabled = true;

    }
    public void StartTCPServer()
    {
        server_type = SERVER_TYPE.TCP;
        starterPanel.SetActive(false);
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(true);
        }
        this.GetComponent<ServerTCP>().enabled = true;

    }

    
    public void BackToMenu()
    {
        closingAppEvent.Invoke();

        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(false);
        }
        starterPanel.SetActive(true);

        
        switch (server_type)
        {
            case SERVER_TYPE.TCP:
                GetComponent<ServerTCP>().enabled = false;
                break;

            case SERVER_TYPE.UDP:
                GetComponent<ServerUDP>().enabled = false;
                break;
        }
    }

}
