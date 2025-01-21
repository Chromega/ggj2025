using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class GillerClient : NetworkBehaviour
{
   [SerializeField]
   GillerPlayer PlayerPrefab;

   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
   {
      if (IsOwner)
      {
         int playersToSpawn = GillerNetworkMgr.I.NumLocalPlayers;
         System.Random rand = new System.Random();
         for (int i = 0; i < playersToSpawn; ++i)
         {
            var instance = Instantiate(PlayerPrefab);
            UnityEngine.Random.InitState(rand.Next());
            float randX = Random.Range(-4f, 4f);
            instance.transform.position = Vector3.right*randX;
            var instanceNetworkObject = instance.GetComponent<NetworkObject>();
            instanceNetworkObject.Spawn();
         }
      }
   }
}
