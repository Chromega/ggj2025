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
   public void OnMove(InputValue value)
   {
      Vector2 v = value.Get<Vector2>();
      if (_player)
         _player.OnMoveInput(v);
   }

   public void OnBlowWater()
   {
      if (_player)
         _player.OnBlowWater();
   }
   public void OnInflate()
   {
      if (_player)
         _player.OnInflate();
   }
   public void OnShootSpikes()
   {
      if (_player)
         _player.OnShootSpikes();
   }
}
