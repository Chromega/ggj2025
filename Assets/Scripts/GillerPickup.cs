using UnityEngine;

public class GillerPickup : MonoBehaviour
{
    
    void OnTriggerEnter(Collider other)
    {
        GillerPlayer otherPlayer = other.gameObject.GetComponentInParent<GillerPlayer>();
        if (otherPlayer != null )
        {
            Destroy(gameObject);
        }
    }

}
