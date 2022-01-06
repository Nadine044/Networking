using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    [Header("Scene Dependencies")]
    [SerializeField] private NetworkManager networkManager;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;

    [Header("Texts")]
   // [SerializeField] private Text resultText;
    [SerializeField] private Text connectionStatusText;
    [SerializeField] private Text turnStatusText;

    [Header("Screen GameObjects")]
    [SerializeField] private GameObject gameoverScreen;
    [SerializeField] private GameObject connectScreen;
    [SerializeField] private GameObject whosTurn;

    private void Awake()
    {
        OnGameLaunched();
    }

    private void OnGameLaunched()
    {
        DisableAllScreens();
        connectScreen.SetActive(true);
    }

    private void DisableAllScreens()
    {
        gameoverScreen.SetActive(false);
        connectScreen.SetActive(false);
        whosTurn.SetActive(false);
    }

    public void OnConnect()
    {
        networkManager.Connect();
    }

    public void SetConnectionStattus(string status)
    {
        connectionStatusText.text = status;
    }
    
    public void Connected()
    {
        connectScreen.SetActive(false);
        whosTurn.SetActive(true);
    }

    public void SetTurnType(bool turn)
    {
        turnStatusText.text = turn ? "My Turn" : "Not My Turn";
    }
}
