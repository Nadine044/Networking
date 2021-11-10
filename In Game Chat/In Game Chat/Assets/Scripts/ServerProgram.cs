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
            //The applications doesn't closes
            

        }
    }


    public void StartTCPServer()
    {
        server_type = SERVER_TYPE.TCP;
        starterPanel.SetActive(false);
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(true);
        }
        GetComponent<ServerTCP>().enabled = true;
        GetComponent<ServerTCP>().StartServer();
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
                GetComponent<ServerTCP>().ClearLog();
                GetComponent<ServerTCP>().enabled = false;
                break;

        }
    }

}
