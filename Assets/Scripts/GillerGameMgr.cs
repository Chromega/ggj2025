using Unity.Netcode;
using UnityEngine;

public class GillerGameMgr : NetworkBehaviour
{

   public static GillerGameMgr I { get; private set; }

   NetworkVariable<int> playerCount = new NetworkVariable<int>(0);

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

         I = this;
      }
   }

   public override void OnDestroy()
   {
      base.OnDestroy();
      if (I == this)
      {
         I = null;
      }
   }
}
