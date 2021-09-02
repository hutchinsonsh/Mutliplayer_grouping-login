using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState state;

    public static event Action<GameState> OnGameStateChanged;

    public enum GameState
    {
        LogIn,
        Lobby,
        Rooms
    }

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateGameStates(GameState.LogIn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateGameStates(GameState newState)
    {
        state = newState;
        switch (newState)
        {
            case GameState.LogIn:
                Debug.Log("LogInState");
                break;
            case GameState.Lobby:
                Debug.Log("LobbyState");
                break;
            case GameState.Rooms:
                Debug.Log("RoomState");
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }
}