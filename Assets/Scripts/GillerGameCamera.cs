using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GillerGameCamera : MonoBehaviour
{
   new Camera camera;
   public Volume vol;
   public UnityEngine.UI.Image fadeOut;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      camera = GetComponent<Camera>();
      GillerGameMgr.I.OnGameStateChanged.AddListener(OnGameStateChanged);
    }

   private void OnDestroy()
   {
      if (GillerGameMgr.I)
      {
         GillerGameMgr.I.OnGameStateChanged.RemoveListener(OnGameStateChanged);
      }
   }

   void OnGameStateChanged(GillerGameMgr.GameState state)
   {
      if (state == GillerGameMgr.GameState.GameOver)
      {
         StartCoroutine(ZoomInCoroutine());
      }
   }

   IEnumerator ZoomInCoroutine()
   {
      Vector3 startPos = transform.position;
      float startFov = camera.fieldOfView;
      Vignette vignette;
      vol.profile.TryGet(out vignette);
      float startVignette = vignette.intensity.value;

      yield return Utl.LerpFunction(1.0f, (x) =>
      {
         Vector3 focusTarget = transform.position;
         focusTarget.z = 0;

         if (GillerPlayerMgr.I.GetPlayers().Count > 0)
         {
            focusTarget = GillerPlayerMgr.I.GetPlayers()[0].transform.position;
         }

         Vector3 endPos = focusTarget;
         endPos.z = startPos.z;

         transform.position = Vector3.Lerp(startPos, endPos, x);
         camera.fieldOfView = Mathf.Lerp(startFov, 10, x);
         vignette.intensity.value = Mathf.Lerp(startVignette, .6f, x);
      });

      while (true)
      {
         if (GillerPlayerMgr.I.GetPlayers().Count > 0)
         {
            Vector3 focusTarget = transform.position;
            focusTarget.z = 0;
            if (GillerPlayerMgr.I.GetPlayers().Count > 0)
            {
               focusTarget = GillerPlayerMgr.I.GetPlayers()[0].transform.position;
            }
            Vector3 endPos = focusTarget;
            endPos.z = startPos.z;
            transform.position = endPos;
         }
         else
         {
            break;
         }
         yield return null;
      }

      yield return Utl.LerpFunction(1.0f, (x) =>
      {
         fadeOut.gameObject.SetActive(true);
         fadeOut.color = new Color(0f, 0f, 0f, x);
      });
   }
}
