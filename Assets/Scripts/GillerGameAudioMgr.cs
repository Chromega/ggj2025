using System;
using Unity.Netcode;
using UnityEngine;

public class GillerGameAudioMgr : MonoBehaviour
{
   public FMODUnity.StudioEventEmitter BattleBgmEmitter;
   public FMODUnity.StudioGlobalParameterTrigger GlobalParameter;

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
   }

   public static void SafePlay(FMODUnity.StudioEventEmitter emitter)
   {
      try { emitter.Play(); } catch { }
   }

   void Update()
   {
      int playerCount = GillerPlayerMgr.I.GetPlayers().Count;
      int targetIntensity = 5 - playerCount;
      targetIntensity = Mathf.Clamp(targetIntensity, 1, 3);

      GlobalParameter.Value = targetIntensity;
   }
}
