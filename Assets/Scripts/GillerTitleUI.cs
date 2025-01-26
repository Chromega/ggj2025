using System.Linq;
using Unity.Multiplayer.Playmode;
using UnityEngine;

public class GillerTitleUI : MonoBehaviour
{

   [SerializeField]
   TMPro.TMP_InputField RoomCode;

   public GameObject DisconnectedRoot;
   public GameObject ConnectingRoot;

   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
   {
      //RoomCode.text = GillerNetworkMgr.I.RoomCode;

      GillerNetworkMgr.OnConnectionStateChanged.AddListener(OnConnectionStateChanged);

      if (CurrentPlayer.ReadOnlyTags().Contains("2Local") || !string.IsNullOrEmpty(GillerSceneMgr.sTestScene))
      {
         OnLocalMultiplayerPressed();
      }
      if (CurrentPlayer.ReadOnlyTags().Contains("3Local"))
      {
         OnLocalMultiplayerPressed();
      }
      if (CurrentPlayer.ReadOnlyTags().Contains("4Local"))
      {
         OnLocalMultiplayerPressed();
      }
      if (CurrentPlayer.ReadOnlyTags().Contains("1Local_1Network_A") || CurrentPlayer.ReadOnlyTags().Contains("1Local_1Network_B"))
      {
         GillerNetworkMgr.I.RoomCode = SystemInfo.deviceName;
         OnNetworkMultiplayerPressed();
      }
      if (CurrentPlayer.ReadOnlyTags().Contains("2Local_2Network_A") || CurrentPlayer.ReadOnlyTags().Contains("2Local_2Network_B"))
      {
         GillerNetworkMgr.I.RoomCode = SystemInfo.deviceName;
         OnNetworkMultiplayerPressed();
      }
      DisconnectedRoot.SetActive(true);
      ConnectingRoot.SetActive(false);
   }

   private void OnDestroy()
   {
      GillerNetworkMgr.OnConnectionStateChanged.RemoveListener(OnConnectionStateChanged);
   }

   public void OnLocalMultiplayerPressed()
   {
      GillerNetworkMgr.I.StartLocalMultiplayerSession();
   }

   public void OnNetworkMultiplayerPressed()
   {
      GillerNetworkMgr.I.StartNetworkSession();
   }

   public void OnRoomCodeChanged(string code)
   {
      GillerNetworkMgr.I.RoomCode = code.ToUpper();
   }

   void OnConnectionStateChanged()
   {
      DisconnectedRoot.SetActive(GillerNetworkMgr.I.State == GillerNetworkMgr.ConnectionState.Disconnected);
      ConnectingRoot.SetActive(GillerNetworkMgr.I.State != GillerNetworkMgr.ConnectionState.Disconnected);
   }
}
