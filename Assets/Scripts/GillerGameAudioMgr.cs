using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GillerGameAudioMgr : MonoBehaviour
{
   public FMODUnity.StudioEventEmitter BattleBgmEmitter;
   public FMODUnity.StudioGlobalParameterTrigger GlobalParameter;
   public FMODUnity.StudioEventEmitter VictoryBgmEmitter;

   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
    {
      GillerGameMgr.I.OnGameStateChanged.AddListener(OnGameStateChanged);
    }

   private void OnGameStateChanged(GillerGameMgr.GameState state)
   {
      if (state == GillerGameMgr.GameState.Playing)
      {
         SafePlay(BattleBgmEmitter);
      }
      else if (state == GillerGameMgr.GameState.GameOver)
      {
         try { BattleBgmEmitter.Stop(); } catch { }
         SafePlay(VictoryBgmEmitter);
      }
   }

   public static void SafePlay(FMODUnity.StudioEventEmitter emitter)
   {
      try { emitter.Play(); } catch { }
   }

   void Update()
   {
      if (!GillerPlayerMgr.I)
         return;
      int playerCount = GillerPlayerMgr.I.GetPlayers().Count;
      int targetIntensity = 5 - playerCount;
      targetIntensity = Mathf.Clamp(targetIntensity, 1, 3);
      //GlobalParameter.Value = targetIntensity;

      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MX_Layering_Intensity", targetIntensity);

   }
}
