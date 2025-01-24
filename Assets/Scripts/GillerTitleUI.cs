using System.Linq;
using Unity.Multiplayer.Playmode;
using UnityEngine;

public class GillerTitleUI : MonoBehaviour
{
   [SerializeField]
   TMPro.TMP_Dropdown PlayerCountDropdown;

   [SerializeField]
   TMPro.TMP_InputField RoomCode;

   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
   {
      RoomCode.text = GillerNetworkMgr.I.RoomCode;
      PlayerCountDropdown.value = PlayerCountDropdown.options.FindIndex((x) => { return x.text == GillerNetworkMgr.I.NumLocalPlayers.ToString(); });

      GillerNetworkMgr.I.OnConnectionStateChanged.AddListener(OnConnectionStateChanged);

      if (CurrentPlayer.ReadOnlyTags().Contains("2Local") || !string.IsNullOrEmpty(GillerSceneMgr.sTestScene))
      {
         GillerNetworkMgr.I.NumLocalPlayers = 2;
         OnLocalMultiplayerPressed();
      }
      if (CurrentPlayer.ReadOnlyTags().Contains("3Local"))
      {
         GillerNetworkMgr.I.NumLocalPlayers = 3;
         OnLocalMultiplayerPressed();
      }
      if (CurrentPlayer.ReadOnlyTags().Contains("4Local"))
      {
         GillerNetworkMgr.I.NumLocalPlayers = 4;
         OnLocalMultiplayerPressed();
      }
      if (CurrentPlayer.ReadOnlyTags().Contains("1Local_1Network_A") || CurrentPlayer.ReadOnlyTags().Contains("1Local_1Network_B"))
      {
         GillerNetworkMgr.I.NumLocalPlayers = 1;
         GillerNetworkMgr.I.RoomCode = SystemInfo.deviceName;
         OnNetworkMultiplayerPressed();
      }
      if (CurrentPlayer.ReadOnlyTags().Contains("2Local_2Network_A") || CurrentPlayer.ReadOnlyTags().Contains("2Local_2Network_B"))
      {
         GillerNetworkMgr.I.NumLocalPlayers = 2;
         GillerNetworkMgr.I.RoomCode = SystemInfo.deviceName;
         OnNetworkMultiplayerPressed();
      }
   }

   private void OnDestroy()
   {
      if (GillerNetworkMgr.I)
      {
         GillerNetworkMgr.I.OnConnectionStateChanged.RemoveListener(OnConnectionStateChanged);
      }
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
      GillerNetworkMgr.I.RoomCode = code;
   }

   public void OnPlayerCountChanged(int idx)
   {
      GillerNetworkMgr.I.NumLocalPlayers = int.Parse(PlayerCountDropdown.options[idx].text);
   }

   void OnConnectionStateChanged()
   {
      gameObject.SetActive(GillerNetworkMgr.I.State == GillerNetworkMgr.ConnectionState.Disconnected);
   }
}
