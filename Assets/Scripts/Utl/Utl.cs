using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Utl : MonoBehaviour
{
   public class TimedAction
   {
      float TotalTime;
      float CurrentTime;
      float DT;

      public TimedAction(float totalTime)
      {
         TotalTime = totalTime;
         CurrentTime = 0;
      }

      public bool IsDone()
      {
         return CurrentTime >= TotalTime;
      }

      public float GetCurrentFrameAmount()
      {
         if (DT == 0)
            return 0;
         return Mathf.Clamp01(1 - (CurrentTime - TotalTime) / DT);
      }

      public void NextFrame(float dt)
      {
         DT = dt;
         CurrentTime += DT;
      }
   }
   public static IEnumerator LerpFunction(float time, System.Action<float> action)
   {
      float t = 0;
      while (t < time)
      {
         action(t / time);
         yield return null;
         t += Time.deltaTime;
      }
      action(1);
   }
   public static IEnumerator DoFunctionForTime(float time, System.Action action)
   {
      float t = 0;
      while (t < time)
      {
         action();
         yield return null;
         t += Time.deltaTime;
      }
   }

   static Collider[] sOutputColliders = new Collider[32];
   static float[] sOutputDistances = new float[32];
   static Vector3[] sOutputDirections = new Vector3[32];
   static Collider[] sCandidateColliders = new Collider[32];

   /*public static int OverlapRigidbody(Rigidbody rb, out Collider[] outColliders)
   {
      outColliders = sOutputColliders;

      Collider[] rbColliders = rb.GetComponentsInChildren<Collider>();
      int outCount = 0;
      for (int i = 0; i < rbColliders.Length; ++i)
      {

      }
   }*/

   public static int OverlapCollider(Collider rbCollider, out Collider[] outColliders, out float[] outDistances, out Vector3[] outDirections)
   {
      int outCount = 0;
      outColliders = sOutputColliders;
      outDistances = sOutputDistances;
      outDirections = sOutputDirections;

      Bounds bounds = rbCollider.bounds;
      Debug.Log(bounds);

      int count = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, sCandidateColliders);
      Debug.Log(count);
      for (int j = 0; j < count; ++j)
      {
         Collider possibleCollider = sCandidateColliders[j];

         if (possibleCollider == rbCollider)
            continue;

         Vector3 direction;
         float distance;
         bool overlapped = Physics.ComputePenetration(rbCollider, rbCollider.transform.position, rbCollider.transform.rotation,
            possibleCollider, possibleCollider.transform.position, possibleCollider.transform.rotation,
            out direction, out distance);

         if (overlapped)
         {
            outColliders[outCount] = possibleCollider;
            outDistances[outCount] = distance;
            outDirections[outCount] = direction;
            ++outCount;
         }
      }
      return outCount;
   }

   public static float TimeInvariantExponentialLerpFactor(float progressPerSecond)
   {
      return 1-Mathf.Pow(1-progressPerSecond, Time.deltaTime);
   }
}
