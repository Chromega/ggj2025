using UnityEngine;
using UnityEngine.InputSystem;

public class GillerPlayerInput : MonoBehaviour
{
   PlayerInput _input;
   GillerPlayer _player;
   public GillerPlayer Player
   { 
      get { return _player; }
      set 
      {
         if (_player != null)
            _player.Input = null;
         _player = value;
         if (_player != null)
            _player.Input = this;
      }
   }

   void Start()
   {
      _input = GetComponent<PlayerInput>();

      GillerInputMgr.I.RegisterPlayerInput(this);

      DontDestroyOnLoad(gameObject);
   }

   bool CanReceiveInput()
   {
      if (!GillerGameMgr.I)
         return false;
      if (GillerGameMgr.I.GetGameState() != GillerGameMgr.GameState.Playing)
         return false;
      return true;
   }
   public void OnMove(InputValue value)
   {
      if (!CanReceiveInput()) return;

      Vector2 v = value.Get<Vector2>();
      if (_player)
         _player.OnMoveInput(v);
   }

   public void OnBlowWater()
   {
      if (!CanReceiveInput()) return;

      if (_player)
         _player.OnBlowWater();
   }
   public void OnInflate()
   {
      if (!CanReceiveInput()) return;

      if (_player)
         _player.OnInflate();
   }
   public void OnShootSpikes()
   {
      if (!CanReceiveInput()) return;

      if (_player)
         _player.OnShootSpikes();
   }
}
