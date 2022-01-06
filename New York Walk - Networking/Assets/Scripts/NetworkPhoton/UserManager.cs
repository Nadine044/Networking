using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    [HideInInspector] public int[] cards = new int[3];

    private int turnCounter= 0;
    // Start is called before the first frame update
    private GameTurn state;
    private MultiplayerGameController controller;

    private List<GameObject> boardSquares = new List<GameObject>();



    // Update is called once per frame
    void Update()
    {
        if(controller.CanPerformMove()&& Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.LogError("Playing");
            SetSpaceCubes._instance.EraseRestrictedSpaceCubes();
            controller.EndTurn();
        }
    }

    public void SetController(MultiplayerGameController controller)
    {
        this.controller = controller;
    }
    public void SetCards(List<int> cards)
    {
        this.cards = cards.ToArray();
    }
    public void SetState(GameTurn state)
    {
        this.state = state;
    }

    public void SetUpToken()
    {
        if(turnCounter >=3)
            return;
        
        JSONReader.Citizen citizen = JSONReader._instance.GetCitizenCardInfo(cards[turnCounter]);
        Debug.LogError($"setup token {citizen.citizen}");
        SetSpaceCubes._instance.SetRestrictedSpaceCubes(citizen.unavailableSquares);
        turnCounter++;
    }

    public void FillBoardSquares(BoxCollider[] boxCollider)
    {
        for(int i =0; i < boxCollider.Length; i++)
        {
            boardSquares.Add(boxCollider[i].gameObject);
        }
        SetSpaceCubes._instance.DefineBoardPos(boardSquares);
    }
}
