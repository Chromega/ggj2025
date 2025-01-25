using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;

public class GillerClient : NetworkBehaviour
{

   public static GillerClient Local { get; private set; }

   public override void OnNetworkSpawn()
   {
      if (IsOwner)
      {
         Local = this;
      }

      DontDestroyOnLoad(gameObject);
   }

   public override void OnDestroy()
   {
      base.OnDestroy();
      if (Local == this)
         Local = null;
   }

   /*void Start()
   {
      if (IsOwner)
      {
         int playersToSpawn = GillerNetworkMgr.I.NumLocalPlayers;
         System.Random rand = new System.Random();
         for (int i = 0; i < playersToSpawn; ++i)
         {
            var instance = Instantiate(PlayerPrefab);
            UnityEngine.Random.InitState(rand.Next());
            float randX = UnityEngine.Random.Range(-4f, 4f);
            instance.transform.position = Vector3.right*randX;
            var instanceNetworkObject = instance.GetComponent<NetworkObject>();
            instanceNetworkObject.Spawn();
         }
      }
   }*/

}
