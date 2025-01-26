using Unity.Netcode;
using UnityEngine;

public class OwnerAnimationEnabler : NetworkBehaviour
{
   public override void OnNetworkSpawn()
   {
      GetComponent<Animator>().enabled = IsOwner;
   }
}
