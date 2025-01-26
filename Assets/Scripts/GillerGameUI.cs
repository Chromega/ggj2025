using System;
using System.Collections;
using UnityEngine;

public class GillerGameUI : MonoBehaviour
{
   public TMPro.TextMeshProUGUI pressSpaceLabel;
   public TMPro.TextMeshProUGUI numberLabel;
   public TMPro.TextMeshProUGUI gameOverLabel;
   public TMPro.TextMeshProUGUI roomCodeLabel;
   public FMODUnity.StudioEventEmitter countSfx3;
   public FMODUnity.StudioEventEmitter countSfx2;
   public FMODUnity.StudioEventEmitter countSfx1;
   public FMODUnity.StudioEventEmitter countSfx0;
   public Animator animator;

   public Color[] winnerColors;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      if (GillerGameMgr.I)
      {
         GillerGameMgr.I.OnGameStateChanged.AddListener(OnGameStateChanged);
         RefreshUI();
         roomCodeLabel.text = "ROOM CODE: " + GillerNetworkMgr.I.RoomCode;
      }
    }

   private void OnGameStateChanged(GillerGameMgr.GameState state)
   {
      if (state == GillerGameMgr.GameState.Countdown)
      {
         StartCoroutine(DoStartCountdown());
      }
      else if (state == GillerGameMgr.GameState.GameOver)
      {
         int winner = GillerGameMgr.I.winner.Value;
         if (winner < 0)
         {
            gameOverLabel.text = "NO WINNER";
            gameOverLabel.color = Color.black;
         }
         else if (winner == 0)
         {
            gameOverLabel.text = "GREEN WINS!";
            gameOverLabel.color = winnerColors[0];
         }
         else if (winner == 1)
         {
            gameOverLabel.text = "YELLOW WINS!";
            gameOverLabel.color = winnerColors[1];
         }
         else if (winner == 2)
         {
            gameOverLabel.text = "ORANGE WINS!";
            gameOverLabel.color = winnerColors[2];
         }
         else if (winner == 3)
         {
            gameOverLabel.text = "PINK WINS!";
            gameOverLabel.color = winnerColors[3];
         }
      }
      RefreshUI();
   }

   void RefreshUI()
   {
      pressSpaceLabel.gameObject.SetActive(GillerGameMgr.I.GetGameState() == GillerGameMgr.GameState.Lobby);
      gameOverLabel.gameObject.SetActive(GillerGameMgr.I.GetGameState() == GillerGameMgr.GameState.GameOver);
   }

   IEnumerator DoStartCountdown()
   {
      int number = 3;

      while (number > 0)
      {
         if (number == 3)
            GillerGameAudioMgr.SafePlay(countSfx3);
         if (number == 2)
            GillerGameAudioMgr.SafePlay(countSfx2);
         if (number == 1)
            GillerGameAudioMgr.SafePlay(countSfx1);
         numberLabel.text = number.ToString();
         animator.SetTrigger("Count");
         yield return new WaitForSeconds(1f);
         --number;
      }
      GillerGameAudioMgr.SafePlay(countSfx0);
      numberLabel.text = "FIGHT!";
      animator.SetTrigger("Count");
   }
}
