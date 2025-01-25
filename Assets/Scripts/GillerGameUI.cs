using System;
using System.Collections;
using UnityEngine;

public class GillerGameUI : MonoBehaviour
{
   public TMPro.TextMeshProUGUI pressSpaceLabel;
   public TMPro.TextMeshProUGUI numberLabel;
   public Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      if (GillerGameMgr.I)
      {
         GillerGameMgr.I.OnGameStateChanged.AddListener(OnGameStateChanged);
         RefreshUI();
      }
    }

   private void OnGameStateChanged(GillerGameMgr.GameState state)
   {
      if (state == GillerGameMgr.GameState.Countdown)
      {
         StartCoroutine(DoStartCountdown());
      }
      RefreshUI();
   }

   void RefreshUI()
   {
      pressSpaceLabel.gameObject.SetActive(GillerGameMgr.I.GetGameState() == GillerGameMgr.GameState.Lobby);
   }

   IEnumerator DoStartCountdown()
   {
      int number = 3;

      while (number > 0)
      {
         numberLabel.text = number.ToString();
         animator.SetTrigger("Count");
         yield return new WaitForSeconds(1f);
         --number;
      }
      numberLabel.text = "FIGHT!";
      animator.SetTrigger("Count");
   }
}
