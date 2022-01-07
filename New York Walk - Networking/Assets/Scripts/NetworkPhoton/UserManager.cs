using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    [HideInInspector] public int[] cards = new int[3];

    private int tokenCounter= 0;
    // Start is called before the first frame update
    private GameTurn state;
    private MultiplayerGameController controller;

    private List<GameObject> boardSquares = new List<GameObject>();



    // Update is called once per frame
    void Update()
    {
        if(controller.CanPerformMove()&& Input.GetKeyDown(KeyCode.Mouse0))
        {
            switch(controller.GetGameTurn())
            {
                case GameTurn.MyTurn:
                    Debug.LogError("MyTurn Enter");
                    controller.EndTurn();
                    break;
                case GameTurn.MyTurnSetUp:
                    Debug.LogError("MyTurnSetUp Enter");
                    SetSpaceCubes._instance.EraseRestrictedSpaceCubes(); //TODO token must launch an event to call this funciton
                    controller.EndSetUpTurn();
                    break;
                default:

                    break;
            }
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
        if(tokenCounter >=3)
            return;
        
        JSONReader.Citizen citizen = JSONReader._instance.GetCitizenCardInfo(cards[tokenCounter]);
        Debug.LogError($"setup token {citizen.citizen}");
        SetSpaceCubes._instance.SetRestrictedSpaceCubes(citizen.unavailableSquares);
        tokenCounter++;
    }

    public void FillBoardSquares(BoxCollider[] boxCollider)
    {
        for(int i =0; i < boxCollider.Length; i++)
        {
            boardSquares.Add(boxCollider[i].gameObject);
        }
        SetSpaceCubes._instance.DefineBoardPos(boardSquares);
    }

    //maybe later pass the token list.Count()
    public int GetTokenCounter()
    {
        return tokenCounter;
    }

    public void UpdateToken()
    {
        Debug.LogError($"UpdatePhase");
        //how can we no the token maybe with a list
        //current token = 
        //for(int i =0; i < tokenList.Count(); i++)
        //{
        //  if(current_token = tokenList[i])
        //  {    
        //if (i == 2)
        //{
        //    currentToken = tokenList[0];
        //    return;
        //}
        //else
        //{
        //    currentToken = tokenList[i + 1];
        //    return;
        //}
        //  }
        //}
        //
    }
}
