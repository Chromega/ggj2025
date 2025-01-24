using System.Collections.Generic;
using UnityEngine;

public class GillerPlayerMgr : Singleton<GillerPlayerMgr>
{
   List<GillerPlayer> _players = new List<GillerPlayer>();

   public void RegisterPlayer(GillerPlayer player)
   {
      _players.Add(player);
   }

   public void UnregisterPlayer(GillerPlayer player)
   {
      _players.Remove(player);
   }

   public List<GillerPlayer> GetPlayers() {  return _players; }
}
