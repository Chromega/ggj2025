using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class GillerPlayer : NetworkBehaviour
{

   Vector3 _velocity;
   bool _isBeingPushed = false;
   Coroutine _getPushedCoroutine;

   [SerializeField]
   float SinkAcceleration;

   [SerializeField]
   float UpSwimBoost;

   [SerializeField]
   float HorizontalSwimSpeed;



   // Update is called once per frame
   void Update()
   {
      // IsOwner will also work in a distributed-authoritative scenario as the owner 
      // has the Authority to update the object.
      if (!IsOwner || !IsSpawned) return;

      if (!_isBeingPushed)
      {
         float xInput = Input.GetAxis("Horizontal");
         //float yInput = Input.GetAxis("Vertical");

         _velocity.x = xInput * HorizontalSwimSpeed;
         _velocity += new Vector3(0, SinkAcceleration, 0) * Time.deltaTime;
         if (Input.GetKeyDown(KeyCode.Space))
         {
            _velocity.y = UpSwimBoost;
         }
      }

      Vector3 newPosition = transform.position + _velocity * Time.deltaTime;
      if (newPosition.y < 0)
         newPosition.y = 0;
      transform.position = newPosition;

      if (Input.GetKeyDown(KeyCode.F))
      {
         PushAway();
      }
   }

   void PushAway()
   {
      Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
      for (int i = 0; i < colliders.Length; i++)
      {
         Collider c = colliders[i];
         GillerPlayer gp = c.GetComponentInParent<GillerPlayer>();
         if (gp && gp != this)
         {
            gp.ReceivePushRpc(transform.position);
         }
      }
   }

   [Rpc(SendTo.Owner)]
   void ReceivePushRpc(Vector3 source)
   {
      
      _velocity = (transform.position - source).normalized * 4f;
      if (_getPushedCoroutine!=null)
         StopCoroutine(_getPushedCoroutine);
      _getPushedCoroutine = StartCoroutine(DoReceivePush());
   }

   IEnumerator DoReceivePush()
   {
      _isBeingPushed = true;
      yield return new WaitForSeconds(.5f);
      _isBeingPushed = false;
   }
}
