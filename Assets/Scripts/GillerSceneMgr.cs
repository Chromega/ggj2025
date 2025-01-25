using Unity.Netcode;
using UnityEngine;

public class GillerSceneMgr : Singleton<GillerSceneMgr>
{
   [SerializeField]
   NetworkManager NetworkManager;

   public static string sTestScene;

   private void Start()
   {
      GillerNetworkMgr.I.OnConnectionStateChanged.AddListener(OnConnectionStateChanged);
      OnConnectionStateChanged();
   }

   /*private void Update()
   {
      if (Input.GetKeyDown(KeyCode.R))
      {
         NetworkManager.SceneManager.LoadScene("gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
      }
   }*/

   public void RestartGame()
   {
      NetworkManager.SceneManager.LoadScene("gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
   }

   public void OnConnectionStateChanged()
   {
      if (GillerNetworkMgr.I.State == GillerNetworkMgr.ConnectionState.Connected)
      {
         if (!NetworkManager.LocalClient.IsSessionOwner)
            return;
         if (!string.IsNullOrEmpty(sTestScene))
         {
            NetworkManager.SceneManager.LoadScene(sTestScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
            sTestScene = null;
         }
         else
         {
            Debug.Log("Checking gameplay...");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "gameplay")
            {
               Debug.Log("Loading gameplay!");
               NetworkManager.SceneManager.LoadScene("gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
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
