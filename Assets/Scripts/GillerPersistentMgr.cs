using UnityEngine;

public class GillerPersistentMgr : Singleton<GillerPersistentMgr>
{
   protected override void Awake()
   {
      if (I != null)
      {
         Destroy(gameObject);
         return;
      }
      base.Awake();
      DontDestroyOnLoad(gameObject);
   }
}
