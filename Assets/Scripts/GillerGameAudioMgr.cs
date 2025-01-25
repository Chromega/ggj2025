using System;
using Unity.Netcode;
using UnityEngine;

public class GillerGameAudioMgr : MonoBehaviour
{
   public FMODUnity.StudioEventEmitter BattleBgmEmitter;

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
}
