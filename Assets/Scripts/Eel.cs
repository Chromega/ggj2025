using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Eel : NetworkBehaviour
{
   Animator _animator;
   NetworkAnimator _networkAnimator;

   GillerPlayer _targetPlayer;


   float _timeSinceLastAttack = 999f;

   float TimeBetweenAttacks = 2.0f;

   bool _isMirrored;
   public GameObject BreakableWall;

   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
   {
      _animator = GetComponent<Animator>();
      _networkAnimator = GetComponent<NetworkAnimator>();
      _isMirrored = transform.localScale.x < 0;
      StartCoroutine(EmergeCoroutine());
   }

   IEnumerator EmergeCoroutine()
   {
      yield return new WaitForSeconds(5f);
      if (IsOwner)
         _animator.SetBool("Emerged", true);

      foreach (Rigidbody rb in BreakableWall.GetComponentsInChildren<Rigidbody>())
      {
         rb.isKinematic = false;
         Vector3 sourcePos = transform.position + 2*Vector3.right * (_isMirrored ? 1 : -1);
         Vector3 direction = (rb.position - sourcePos).normalized;
         rb.linearVelocity = direction * 50.0f;
      }
   }

   // Update is called once per frame
   void Update()
   {
      if (IsOwner)
      {
         float bestDistance = Mathf.Infinity;
         GillerPlayer bestPlayer = _targetPlayer;
         foreach (GillerPlayer player in GillerPlayerMgr.I.GetPlayers())
         {
            Vector3 displacement = player.transform.position - transform.position;
            float distance = displacement.magnitude;
            if (distance < bestDistance)
            {
               bestDistance = distance;
               bestPlayer = player;
            }
         }

         _targetPlayer = bestPlayer;

         float targetPitch = 0;
         float targetJaw = 0f;
         bool stillAttacking = _timeSinceLastAttack < TimeBetweenAttacks;
         if (_targetPlayer && bestDistance < 30f)
         {
            Vector3 displacement = (_targetPlayer.transform.position - transform.position);
            displacement.x *= _isMirrored?-1:1;
            targetPitch = Mathf.Atan2(displacement.y, displacement.x) * Mathf.Rad2Deg;
            targetPitch += 10;
            targetJaw = .5f;

            if (bestDistance < 15f && !stillAttacking)
            {
               targetJaw = 1f;
               _networkAnimator.SetTrigger("Attack");
               AttackRpc();
            }
         }
         if (stillAttacking)
         {
            if (_timeSinceLastAttack < .05f)
               targetJaw = 1.0f;
            else
               targetJaw = 0.0f;
         }

         if (_isMirrored)
            targetPitch *= -1;
         //float targetPitch = (_targetPlayer != null)?Vector3.SignedAngle(Vector3.right, (_targetPlayer.transform.position-transform.position), Vector3.forward):0;
         //Debug.Log(targetPitch);
         float currentPitch = transform.localRotation.eulerAngles.z;
         float lerpFactor = Utl.TimeInvariantExponentialLerpFactor(.95f);
         currentPitch += 360;
         currentPitch += 180;
         currentPitch %= 360;
         currentPitch -= 180;

         targetPitch += 360;
         targetPitch += 180;
         targetPitch %= 360;
         targetPitch -= 180;
         transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(currentPitch, targetPitch, lerpFactor));


         float currentJaw = _animator.GetFloat("JawOpen");

         float jawLerpFactor;
         if (stillAttacking)
            jawLerpFactor = Utl.TimeInvariantExponentialLerpFactor(.99999f);
         else
            jawLerpFactor = Utl.TimeInvariantExponentialLerpFactor(.95f);
         _animator.SetFloat("JawOpen", Mathf.Lerp(currentJaw, targetJaw, jawLerpFactor));
         //transform.localRotation = Quaternion.Euler(0, 0, targetPitch);
      }

      _timeSinceLastAttack += Time.deltaTime;
   }

   [Rpc(SendTo.Everyone)]
   void AttackRpc()
   {
      _timeSinceLastAttack = 0.0f;
   }

   private void OnCollisionEnter(Collision collision)
   {
      if (collision.collider.isTrigger)
         return;

      GillerPlayer gp = collision.collider.GetComponentInParent<GillerPlayer>();
      if (gp)
      {
         gp.ReceiveEelHitRpc(NetworkObject);
      }
   }
}
