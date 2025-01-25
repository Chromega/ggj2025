using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GillerInputMgr : Singleton<GillerInputMgr>
{
   public InputActionProperty GamepadJoinAction;
   public InputActionProperty WasdJoinAction;
   public InputActionProperty ArrowsJoinAction;

   public PlayerInputManager InputManager;

   bool _hasWasdPlayer;
   bool _hasArrowsPlayer;

   List<GillerPlayerInput> _playersInputs = new List<GillerPlayerInput>();

   private void Start()
   {
      PreserveJoinAction(ref GamepadJoinAction);
      PreserveJoinAction(ref WasdJoinAction);
      PreserveJoinAction(ref ArrowsJoinAction);

      GamepadJoinAction.action.performed += JoinGamepad;
      WasdJoinAction.action.performed += JoinWASD;
      ArrowsJoinAction.action.performed += JoinArrows;
   }

   void PreserveJoinAction(ref InputActionProperty joinAction)
   {
      if (joinAction.reference != null && joinAction.action?.actionMap?.asset != null)
      {
         var inputActionAsset = Instantiate(joinAction.action.actionMap.asset);
         var inputActionReference = InputActionReference.Create(inputActionAsset.FindAction(joinAction.action.name));
         joinAction = new InputActionProperty(inputActionReference);
         joinAction.action.Enable();
      }
   }

   public void JoinGamepad(InputAction.CallbackContext context)
   {
      InputManager.JoinPlayerFromActionIfNotAlreadyJoined(context);
   }
   public void JoinWASD(InputAction.CallbackContext context)
   {
      if (_hasWasdPlayer)
         return;
      var device = context.control.device;
      var p = InputManager.JoinPlayer(pairWithDevice: device);
      p.SwitchCurrentControlScheme("Keyboard WASD", Keyboard.current);
      _hasWasdPlayer = true;
      WasdJoinAction.action.Disable();
   }

   public void JoinArrows(InputAction.CallbackContext context)
   {
      if (_hasArrowsPlayer)
         return;
      var device = context.control.device;
      var p = InputManager.JoinPlayer(pairWithDevice: device);
      p.SwitchCurrentControlScheme("Keyboard Arrows", Keyboard.current);
      _hasArrowsPlayer = true;
      ArrowsJoinAction.action.Disable();
   }

   public void RegisterPlayerInput(GillerPlayerInput playerInput)
   {
      _playersInputs.Add(playerInput);



      if (GillerPlayerMgr.I)
      {
         GillerPlayerMgr.I.OnPlayerInputAdded(playerInput);
      }
   }

   public List<GillerPlayerInput> GetPlayerInputs() { return _playersInputs; }
}
