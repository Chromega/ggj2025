using System.Collections.Generic;
using UnityEngine;

public class GillerPlayerMgr : Singleton<GillerPlayerMgr>
{
   List<GillerPlayer> _players = new List<GillerPlayer>();
   List<GillerPlayer> _localPlayers = new List<GillerPlayer>();

   public GillerPlayer PlayerPrefab;

   public void RegisterPlayer(GillerPlayer player)
   {
      _players.Add(player);
      if (player.IsOwner)
         _localPlayers.Add(player);

      UpdateInputAssignments();
   }

   public void UnregisterPlayer(GillerPlayer player)
   {
      _players.Remove(player);
      _localPlayers.Remove(player);
   }


   public void UpdateInputAssignments()
   {
      List<GillerPlayerInput> playerInputs = GillerInputMgr.I.GetPlayerInputs();
      foreach (GillerPlayer player in _localPlayers)
      {
         if (player.Input == null)
         {
            if (player._localInputIdx.Value < playerInputs.Count)
            {
               playerInputs[player._localInputIdx.Value].Player = player;
            }
         }
      }
      /*
      List<GillerPlayerInput> playerInputs = GillerInputMgr.I.GetPlayerInputs();
      for (int i = 0; i < playerInputs.Count; ++i)
      {
         if (playerInputs[i].Player == null)
         {
            for (int j = 0; j < _localPlayers.Count; ++j)
            {
               if (_localPlayers[j].Input == null)
               {
                  playerInputs[i].Player = _localPlayers[j];
                  Debug.Log("Made an assignment!");
                  break;
               }
            }
         }
      }*/
   }

   public void SpawnPlayers()
   {
      List<GillerPlayerInput> playerInputs = GillerInputMgr.I.GetPlayerInputs();
      int playersToSpawn = playerInputs.Count;
      if (playersToSpawn < 1) playersToSpawn = 1;

      for (int i = 0; i < playersToSpawn; ++i)
      {
         GillerGameMgr.I.RequestPlayerSpawnRpc(GillerNetworkMgr.I.NetworkManager.LocalClientId, i);
      }

   }

   public void OnPlayerInputAdded(GillerPlayerInput input)
   {
      UpdateInputAssignments();
      if (input.Player == null)
      {
         List<GillerPlayerInput> playerInputs = GillerInputMgr.I.GetPlayerInputs();
         GillerGameMgr.I.RequestPlayerSpawnRpc(GillerClient.Local.OwnerClientId, playerInputs.IndexOf(input));
      }
   }

   public List<GillerPlayer> GetPlayers() {  return _players; }
}
