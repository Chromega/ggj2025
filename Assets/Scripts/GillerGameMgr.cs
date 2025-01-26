using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class GillerGameMgr : NetworkBehaviour
{

   public static GillerGameMgr I { get; private set; }


   NetworkVariable<int> playerCount = new NetworkVariable<int>(0);
   const int kMaxPlayers = 4;
   int playersAtStart;

   public Transform[] SpawnPositions;

   public enum GameState
   {
      Lobby,
      Countdown,
      Playing,
      GameOver
   }

   NetworkVariable<GameState> _state = new NetworkVariable<GameState>(GameState.Lobby);

   public UnityEvent<GameState> OnGameStateChanged;

   private void Awake()
   {
      I = this;
      Debug.Log("Awake");
   }


   public GameState GetGameState()
   {
      return _state.Value;
   }
   /*
   public void RegisterPlayer(GillerPlayer gp)
   {
      gp.SetPlayerIdxRpc(playerCount.Value);
      playerCount.Value = playerCount.Value + 1;
   }*/



   [Rpc(SendTo.Server)]
   public void RequestPlayerSpawnRpc(ulong clientId, int inputId)
   {
      Debug.Log("Received spawn request");
      if (_state.Value != GameState.Lobby)
         return;

      if (playerCount.Value < kMaxPlayers)
      {
         //client.ReceivePlayerAssignmentRpc(playerCount.Value, inputId);
         //playerCount.Value = playerCount.Value + 1;

         int id = playerCount.Value;

         GillerPlayer instance = Instantiate(GillerPlayerMgr.I.PlayerPrefab);
         instance.transform.position = SpawnPositions[id].position;
         instance._playerIdx.Value = id;
         instance._localInputIdx.Value = inputId;
         var instanceNetworkObject = instance.GetComponent<NetworkObject>();
         if (clientId == NetworkManager.LocalClientId)
         {
            instanceNetworkObject.Spawn();
         }
         else
         {
            instanceNetworkObject.SpawnWithOwnership(clientId);
         }

         playerCount.Value = playerCount.Value + 1;
      }
   }

   public override void OnNetworkSpawn()
   {
      Debug.Log("Spawned");
      _state.OnValueChanged += OnChangeState;
      StartCoroutine(DoSpawnPlayers());
   }

   IEnumerator DoSpawnPlayers()
   {
      yield return new WaitForSeconds(1f);

      GillerPlayerMgr.I.SpawnPlayers();
   }

   private void OnChangeState(GameState previousValue, GameState newValue)
   {
      if (previousValue == GameState.Lobby)
         playersAtStart = GillerPlayerMgr.I.GetPlayers().Count;
      OnGameStateChanged?.Invoke(newValue);
   }

   public override void OnDestroy()
   {
      base.OnDestroy();
      if (I == this)
      {
         I = null;
      }
   }

   private void Update()
   {
      if (_state.Value == GameState.Lobby)
      {
         if (Input.GetKeyDown(KeyCode.Space))
         {
            StartCountdownRpc();
         }
      }
      else if (_state.Value == GameState.Playing)
      {
         int victoryPlayers = (playersAtStart > 1) ? 1 : 0;
         if (IsOwner && GillerPlayerMgr.I.GetPlayers().Count <= victoryPlayers)
         {
            _state.Value = GameState.GameOver;
         }
      }
      else if (_state.Value == GameState.GameOver)
      {
         if (Input.GetKeyDown(KeyCode.Space))
         {
            GillerSceneMgr.I.RestartGame();
         }
         /*else if (Input.GetKeyDown(KeyCode.Escape))
         {
            GillerNetworkMgr.I.Disconnect();
         }*/
      }
   }

   [Rpc(SendTo.Owner)]
   void StartCountdownRpc()
   {
      if (_state.Value == GameState.Lobby)
      {
         _state.Value = GameState.Countdown;
         StartCoroutine(DoCountdown());
      }
   }

   IEnumerator DoCountdown()
   {
      yield return new WaitForSeconds(3.0f);
      _state.Value = GameState.Playing;
   }
}
