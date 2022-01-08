using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class TokenScript : MonoBehaviour
{
    private PhotonView photonView;
    private JSONReader.Citizen citizenCard = new JSONReader.Citizen();
    private int id = -1;
    private int boardArrayPos;
    private JSONReader.Citizen citizen;
    private TokenAnimationMovement tokenAnimation;

    [SerializeField] private List<Material> blueMaterialList;
    [SerializeField] private List<Material> redMaterialList;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        tokenAnimation = GetComponent<TokenAnimationMovement>();
    }

    public void SetMaterial(int materialCounter)
    {
        if (UserManager._instance.GetTeam())
            GetComponent<MeshRenderer>().material = blueMaterialList[materialCounter];

        else if (!UserManager._instance.GetTeam())
            GetComponent<MeshRenderer>().material = redMaterialList[materialCounter];
    }

    public void SetID_BoardArrayPos(int id, int boardArrayPos,int materialCounter)
    {
        this.id = id;
        this.boardArrayPos = boardArrayPos;
        LastTokenSetted(materialCounter);
    }

    public int GetID()
    {
        return id;
    }
    public void UpdateBoard(int[] boardArray)
    {

    }

    public void StartIdleAnimation()
    {
        tokenAnimation.SetIdleAnimation(true);
    }

    public void StopIdleAnimation()
    {
        tokenAnimation.SetIdleAnimation(false);
    }

    public void LastTokenSetted(int materialCounter) 
    {
        photonView.RPC(nameof(RPC_LastTokenSetted),RpcTarget.AllBuffered, new object[] { id,boardArrayPos,UserManager._instance.GetTeam(),materialCounter});
    }

    [PunRPC]
    private void RPC_LastTokenSetted(int id,int boardArrayPos,bool team,int materialCounter)
    {
        Debug.LogError($"Instantiated player with id {id}, {boardArrayPos}");
        UserManager._instance.UpdateBoardArray(id, boardArrayPos);
        if(team)
            GetComponent<MeshRenderer>().material = blueMaterialList[materialCounter];
        
        else if(!team)
            GetComponent<MeshRenderer>().material = redMaterialList[materialCounter];

    }

    /// <summary>
    /// This function validates that the clicked board pos is adjacent to the current token, not diagonals though
    /// </summary>
    /// <param name="clicked_pos"></param>
    /// <returns></returns>
    public bool CheckAdjacentSquares(int clicked_pos,int[] boardArray)
    {
        //Get our current pos in the array/board
        int idx = Array.IndexOf(boardArray, id);
        int row_offset = 5;
        int col_offset = 1;

        if ((idx % 5) == 0 && idx - col_offset == clicked_pos) //it means is on the right edge of the board and clicked on the left edge
        {
            Debug.Log("right to left false");
            return false;
        }
        else if (((idx + 1) % 5) == 0 && idx + col_offset == clicked_pos) //it means is on the left edge of the board and clicked on the right edge
        {
            Debug.Log("left to right false");
            return false;
        }

        //here we validate that the clicked position is adjacent to the current token //diagonals don't count
        if (idx + row_offset == clicked_pos || idx - row_offset == clicked_pos ||
            idx + col_offset == clicked_pos || idx - col_offset == clicked_pos)
        {
            //check that we don't jump directly from right to left
            if (idx % 5 == 0 && (idx + row_offset) % 5 != 0)
            {
                Debug.Log("Double check return false 1");
                return false;
            }
            //check that we don't jump directly from left to right
            else if (idx + 1 % 5 == 0 && (idx - row_offset + 1) % 5 != 0)
            {
                Debug.Log("Double check return false 2");
                return false;
            }
            return true;
        }
        return false;
    }

    public void UpdatePosition(Vector3 newPos)
    {
        tokenAnimation.SetDestPos(newPos);
    }

    public void SetCitizenCard(JSONReader.Citizen citizen)
    {
        this.citizen = citizen;
    }
}
