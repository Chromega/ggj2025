using UnityEngine;

public class GillerSceneMgr : Singleton<GillerSceneMgr>
{

   private void Start()
   {
      GillerNetworkMgr.I.OnConnectionStateChanged.AddListener(OnConnectionStateChanged);
      OnConnectionStateChanged();
   }

   void OnConnectionStateChanged()
   {
      if (GillerNetworkMgr.I.State == GillerNetworkMgr.ConnectionState.Connected)
      {
         Debug.Log("Checking ArtTest...");
         if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "ArtTest")
         {
            Debug.Log("Loading ArtTest!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("ArtTest");
         }
      }
      else if (GillerNetworkMgr.I.State == GillerNetworkMgr.ConnectionState.Disconnected)
      {
         Debug.Log("Checking Main...");
         if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "main")
         {
            UnityEngine.SceneManagement.SceneManager.LoadScene("main");
            Debug.Log("Loading Main!");
         }
      }
   }
}
