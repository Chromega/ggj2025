using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
using System.Collections;
using Unity.Networking.Transport;

public class GillerNetworkMgr : MonoBehaviour
{
   public static GillerNetworkMgr I { get; private set; }

   public NetworkManager NetworkManager;
   public string RoomCode;
   public int NumLocalPlayers;
   ISession m_Session;

   public NetworkTransport LocalEditorTransport;
   public NetworkTransport OnlineTransport;

   public UnityEvent OnConnectionStateChanged;

   public enum ConnectionState
   {
      Disconnected,
      Connecting,
      Connected,
   }

   private ConnectionState _state = ConnectionState.Disconnected;
   public ConnectionState State
   {
      get
      {
         return _state;
      }
      private set
      {
         _state = value;
         OnConnectionStateChanged?.Invoke();
      }
   }

   async void Awake()
   {
      I = this;

      NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
      NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;

      RoomCode = UnityEngine.Random.Range(0, 9999).ToString("0000");

      await UnityServices.InitializeAsync();
   }


   //Debugging!
   private void Start()
   {
      //StartLocalMultiplayerSession();
      MultiplayerService.Instance.SessionAdded += Instance_SessionAdded;
   }

   private void Instance_SessionAdded(ISession obj)
   {
      State = ConnectionState.Connected;
   }

   private void OnDestroy()
   {
      if (I == this)
         I = null;
   }

   public void StartLocalMultiplayerSession()
   {
#if UNITY_WEBGL && !UNITY_EDITOR
      StartNetworkSession();
#else
      LocalEditorTransport.enabled = true;
      OnlineTransport.enabled = false;
      NetworkManager.NetworkConfig.NetworkTransport = LocalEditorTransport;
      StartLocalSessionAsync(RoomCode, UnityEngine.Random.Range(0, 1000000000).ToString());
#endif
   }

   public async void StartNetworkSession()
   {
#if UNITY_WEBGL && !UNITY_EDITOR
      LocalEditorTransport.enabled = false;
      OnlineTransport.enabled = true;
      NetworkManager.NetworkConfig.NetworkTransport = OnlineTransport;
#else
      LocalEditorTransport.enabled = true;
      OnlineTransport.enabled = false;
      NetworkManager.NetworkConfig.NetworkTransport = LocalEditorTransport;
#endif
      await CreateOrJoinSessionAsync(RoomCode, UnityEngine.Random.Range(0, 1000000000).ToString());
   }

   public void StartLocalSessionAsync(string sessionName, string profileName)
   {
      State = ConnectionState.Connecting;
      if (NetworkManager.StartHost())
         State = ConnectionState.Connected;
      else
         State = ConnectionState.Disconnected;

      return;
      /*if (string.IsNullOrEmpty(profileName) || string.IsNullOrEmpty(sessionName))
      {
         Debug.LogError("Please provide a player and session name, to login.");
         return;
      }

      State = ConnectionState.Connecting;
      try
      {
         // Only sign in if not already signed in.
         if (!AuthenticationService.Instance.IsSignedIn)
         {
            AuthenticationService.Instance.SwitchProfile(profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
         }

         Debug.Log("AAA");

         // Set the session options.
         var options = new SessionOptions()
         {
            Name = sessionName,
            MaxPlayers = 4
         }.WithDirectNetwork();

         // Join a session if it already exists, or create a new one.
         m_Session = await MultiplayerService.Instance.CreateSessionAsync(options);
         Debug.Log("BBBB");
         State = ConnectionState.Connected;
      }
      catch (Exception e)
      {
         State = ConnectionState.Disconnected;
         Debug.LogException(e);
      }*/
   }
   public async Task CreateOrJoinSessionAsync(string sessionName, string profileName)
   {
      if (string.IsNullOrEmpty(profileName) || string.IsNullOrEmpty(sessionName))
      {
         Debug.LogError("Please provide a player and session name, to login.");
         return;
      }

      State = ConnectionState.Connecting;
      try
      {
         // Only sign in if not already signed in.
         if (!AuthenticationService.Instance.IsSignedIn)
         {
            AuthenticationService.Instance.SwitchProfile(profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
         }

         // Set the session options.
         var options = new SessionOptions()
         {
            Name = sessionName,
            MaxPlayers = 4
         }.WithDistributedAuthorityNetwork();

         // Join a session if it already exists, or create a new one.
         m_Session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);
         State = ConnectionState.Connected;
      }
      catch (Exception e)
      {
         State = ConnectionState.Disconnected;
         Debug.LogException(e);
      }
   }

   public async void Disconnect()
   {
      if (m_Session != null)
      {
         await m_Session.LeaveAsync();
      }

      State = ConnectionState.Disconnected;
   }


   // Just for logging.
   void OnClientConnectedCallback(ulong clientId)
   {
      if (NetworkManager.LocalClientId == clientId)
      {
         Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
         GillerSceneMgr.I.OnConnectionStateChanged();
      }
   }

   // Just for logging.
   void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
   {
      if (NetworkManager.LocalClient.IsSessionOwner)
      {
         Debug.Log($"Client-{NetworkManager.LocalClientId} is the session owner!");

      }
   }
}
