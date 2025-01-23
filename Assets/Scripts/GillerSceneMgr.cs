using UnityEngine;

public class GillerSceneMgr : Singleton<GillerSceneMgr>
{

   private void Start()
   {
      GillerNetworkMgr.I.OnConnectionStateChanged.AddListener(OnConnectionStateChanged);
   }

   void OnConnectionStateChanged()
   {
      if (GillerNetworkMgr.I.State == GillerNetworkMgr.ConnectionState.Connected)
      {
         UnityEngine.SceneManagement.SceneManager.LoadScene("ArtTest");
      }
      else if (GillerNetworkMgr.I.State == GillerNetworkMgr.ConnectionState.Disconnected)
      {
         UnityEngine.SceneManagement.SceneManager.LoadScene("main");
      }
   }
}
