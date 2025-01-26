using UnityEngine;

public class GillerPersistentActivator : MonoBehaviour
{
   public GameObject[] objsToActivate;

   void Start()
   {
      if (GillerPersistentMgr.I == null)
      {
         foreach (GameObject go in objsToActivate)
         {
            go.SetActive(true);
         }
      }
   }
}
