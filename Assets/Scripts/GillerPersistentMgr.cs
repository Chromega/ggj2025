using UnityEngine;

public class GillerPersistentMgr : Singleton<GillerPersistentMgr>
{
   protected override void Awake()
   {
      base.Awake();
      DontDestroyOnLoad(gameObject);
   }
}
