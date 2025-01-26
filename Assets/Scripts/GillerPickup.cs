using UnityEngine;
using Unity.Netcode;

public class GillerPickup : NetworkBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Collider>().isTrigger)
            return;
        GillerPlayer gp = other.GetComponent<Collider>().GetComponentInParent<GillerPlayer>();
        if (gp && gp.IsOwner)
        {
            gp.RaiseBreath(1);
            NetworkObject.Despawn(true);
        }
    }

}
