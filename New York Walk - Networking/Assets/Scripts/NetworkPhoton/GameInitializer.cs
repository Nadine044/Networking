using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Dependant Objects")]
    [SerializeField] private MultiplayerBoard multiplayerBoardPrefab;
    [SerializeField] private MultiplayerGameController MultiplayerGameControllerPrefab;

    [Header("Scene references")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Transform BoardAnchor;
    [SerializeField] private UIManager UIManager;

    private User user; //for now we leave it like this

    private void Awake()
    {
        user = GetComponent<User>();
    }
    public void CreateMultiplayerBoard()
    {
        if(!networkManager.IsRoomFull()) //only the first playe instantiates the board
        {
            PhotonNetwork.Instantiate(multiplayerBoardPrefab.name, BoardAnchor.position, BoardAnchor.rotation);
        }
    }

    public void InitializeMultiplayerGameController()
    {
        MultiplayerBoard mBoard = FindObjectOfType<MultiplayerBoard>();//try not to use findobject
        MultiplayerGameController controller = Instantiate(MultiplayerGameControllerPrefab);
        controller.SetDependencies(UIManager,user, mBoard);
    }

}
