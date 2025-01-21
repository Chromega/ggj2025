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
