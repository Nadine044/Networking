using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Dependant Objects")]
    [SerializeField] private MultiplayerBoard multiplayerBoardPrefab;

    [Header("Scene references")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Transform BoardAnchor;
    public void CreateMultiplayerBoard()
    {
        if(!networkManager.IsRoomFull()) //only the first playe instantiates the board
        {
            PhotonNetwork.Instantiate(multiplayerBoardPrefab.name, BoardAnchor.position, BoardAnchor.rotation);
        }
    }

}
