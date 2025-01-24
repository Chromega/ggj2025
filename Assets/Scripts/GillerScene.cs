using UnityEngine;
using UnityEngine.SceneManagement;

public class GillerScene : MonoBehaviour
{
   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
   {
      if (!GillerPersistentMgr.I)
      {
         GillerSceneMgr.sTestScene = SceneManager.GetActiveScene().name;
         SceneManager.LoadScene("main");
      }
   }
}
