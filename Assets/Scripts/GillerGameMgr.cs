using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class GillerGameMgr : NetworkBehaviour
{

   public static GillerGameMgr I { get; private set; }

   NetworkVariable<int> playerCount = new NetworkVariable<int>(0);

   public Transform[] SpawnPositions;

   public enum GameState
   {
      Lobby,
      Countdown,
      Playing
   }

   NetworkVariable<GameState> _state = new NetworkVariable<GameState>(GameState.Lobby);

   public UnityEvent<GameState> OnGameStateChanged;

   private void Awake()
   {
      I = this;
   }


   public GameState GetGameState()
   {
      return _state.Value;
   }

   public void RegisterPlayer(GillerPlayer gp)
   {
      gp.SetPlayerIdxRpc(playerCount.Value);
      playerCount.Value = playerCount.Value + 1;
   }

   public override void OnNetworkSpawn()
   {
      if (IsOwner)
      {
         foreach (GillerPlayer gp in GillerPlayerMgr.I.GetPlayers())
            RegisterPlayer(gp);
      }

      _state.OnValueChanged += OnChangeState;
   }

   private void OnChangeState(GameState previousValue, GameState newValue)
   {
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
