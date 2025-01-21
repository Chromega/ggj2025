using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : class
{
    public static T I {  get; private set; }

   protected virtual void Awake()
   {
      I = this as T;
   }

   protected virtual void OnDestroy()
   {
      if (I == this as T)
         I = null;
   }

}
