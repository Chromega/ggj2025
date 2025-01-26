using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.InputSystem.LowLevel;

public class GillerPlayer : NetworkBehaviour
{
   [System.Serializable]
   public struct FishSkin
   {
      public Material[] fishMats;
      public Material[] spikeMats;
   }


   public enum State
   {
      Deflated,
      Inflated,
      Limp
   }

   [SerializeField]
   float NormalMovementSpeed = 10f;

   [SerializeField]
   [Range(0.0f, 1.0f)]
   float NormalResponsiveness = .9f;

   [SerializeField]
   float InflatedMovementSpeed = 2f;

   [SerializeField]
   [Range(0.0f, 1.0f)]
   float InflatedResponsiveness = .5f;

   [SerializeField]
   float LimpMovementSpeed = 5f;

   [SerializeField]
   [Range(0.0f, 1.0f)]
   float LimpResponsiveness = .75f;

   [SerializeField]
   float BreathRegenDelay = .3f;

   [SerializeField]
   float BreathRegenTimePerSegment = .5f;
   
   [SerializeField]
   float InflationDecayTimePerSegment = 1.2f;


   [SerializeField]
   Animator FishAnimator;

   [SerializeField]
   new Collider collider;

   [SerializeField]
   Transform FishRoot;

   [SerializeField]
   Transform SpikeRoot;

   [SerializeField]
   ParticleSystem BlowWaterParticles;

   [SerializeField]
   Transform InstantFacingRoot;

   [SerializeField]
   Collider PushCollider;

   [Header("Damage")]
   [SerializeField]
   Material TemporaryMaterial;

   [SerializeField]
   float duration = 0.05f;

   Material OriginalMaterial;

   [SerializeField]
   Renderer FishRenderer;

   [SerializeField]
   Renderer SpikeRenderer;

   [SerializeField]
   FishSkin[] FishSkins;

   [Header("SFX")]
   public FMODUnity.StudioEventEmitter InflateSFX;
   public FMODUnity.StudioEventEmitter DeflateSFX;
   public FMODUnity.StudioEventEmitter BlowSFX;
   public FMODUnity.StudioEventEmitter MissileSFX;
   public FMODUnity.StudioEventEmitter DamagedSFX;

   [Header("UI")]
   [SerializeField]
   UnityEngine.UI.Image AirMeter;
   [SerializeField]
   UnityEngine.UI.Image InflationMeter;
   [SerializeField]
   GameObject InflationRoot;

   #region Synchronized State
   NetworkVariable<State> _state = new NetworkVariable<State>(State.Deflated);
   public NetworkVariable<int> _playerIdx = new NetworkVariable<int>(0);
   public NetworkVariable<int> _localInputIdx = new NetworkVariable<int>(0);
   const float kMaxBreath = 4;
   NetworkVariable<float> _breath = new NetworkVariable<float>(kMaxBreath);
   const float kMaxInflation = 2;
   NetworkVariable<float> _inflation = new NetworkVariable<float>(kMaxInflation);

   #endregion
   //Local state

   #region Local State
   bool IsBubbled = false;
   bool IsHurt = false;
   bool _isBeingPushed = false;
   GillerPlayerInput _input;
   public GillerPlayerInput Input
   {
      get { return _input; }
      set { _input = value; }
   }

   Coroutine _getPushedCoroutine;

   Vector2 _moveInput;

   Rigidbody _rigidbody;

   float _targetYaw = -90;
   float _currentYaw = -90;

   AudioSource _audioSource;

   float _breathRegenTimer = 999f;
   float _limpTime = 0.0f;

   #endregion

   private void Start()
   {
      _rigidbody = GetComponent<Rigidbody>();
      collider.transform.localScale = 2 * Vector3.one;
      _audioSource = GetComponent<AudioSource>();


      NetworkManager.SceneManager.OnUnload += SceneManager_OnUnload;
   }

   void OnChangeState(State oldState, State newState)
   {
      if (newState == State.Inflated)
         collider.transform.localScale = 4 * Vector3.one;
      else
         collider.transform.localScale = 2 * Vector3.one;

      if (newState == State.Inflated)
      {
         GillerGameAudioMgr.SafePlay(InflateSFX);
      }
      else if (oldState == State.Inflated)
      {
         GillerGameAudioMgr.SafePlay(DeflateSFX);
      }

      if (newState == State.Limp)
      {
         _limpTime = 0.0f;
      }
      else if (oldState == State.Limp)
      {
         Color c = AirMeter.color;
         c.a = 1;
         AirMeter.color = c;
      }

      if (IsOwner)
      {
         FishAnimator.SetInteger("State", (int)newState);
      }
   }

   void OnChangePlayerIdx(int oldState, int newState)
   {
      RefreshPlayerSkin();
   }

   void RefreshPlayerSkin()
   {
      FishSkin skin = FishSkins[_playerIdx.Value % FishSkins.Length];
      FishRenderer.materials = skin.fishMats;
      SpikeRenderer.materials = skin.spikeMats;
   }


   [Rpc(SendTo.Owner)]
   public void SetPlayerIdxRpc(int idx)
   {
      _playerIdx.Value = idx;
      transform.position = GillerGameMgr.I.SpawnPositions[idx % GillerGameMgr.I.SpawnPositions.Length].position;
   }

   float GetMovementSpeed()
   {
      if (_state.Value == State.Inflated)
         return InflatedMovementSpeed;
      else if (_state.Value == State.Deflated)
         return NormalMovementSpeed;
      else
         return LimpMovementSpeed;
   }

   float GetResponsiveness()
   {
      if (_state.Value == State.Inflated)
         return InflatedResponsiveness;
      else if (_state.Value == State.Deflated)
         return NormalResponsiveness;
      else
         return LimpResponsiveness;
   }

   // Update is called once per frame
   void FixedUpdate()
   {
      // IsOwner will also work in a distributed-authoritative scenario as the owner 
      // has the Authority to update the object.
      if (!IsOwner || !IsSpawned) return;

      if (!_isBeingPushed)
      {
         _rigidbody.linearVelocity = Vector3.Lerp(_rigidbody.linearVelocity, _moveInput * GetMovementSpeed(), Utl.TimeInvariantExponentialLerpFactor(GetResponsiveness()));
         //_rigidbody.linearVelocity = _moveInput * 5;
      }
   }

   private void Update()
   {
      if (IsOwner)
      {
         _currentYaw = Mathf.Lerp(_currentYaw, _targetYaw, Utl.TimeInvariantExponentialLerpFactor(.97f));
         FishRoot.transform.rotation = Quaternion.Euler(90, 0, _currentYaw);

         _breathRegenTimer += Time.deltaTime;
         if (_state.Value == State.Limp)
         {
            float newBreath = _breath.Value + Time.deltaTime / BreathRegenTimePerSegment;
            if (newBreath >= kMaxBreath)
            {
               newBreath = kMaxBreath;
               _state.Value = State.Deflated;
            }
            _breath.Value = newBreath;
         }
         else if (_state.Value == State.Inflated)
         {
            float newInflation = _inflation.Value - Time.deltaTime / InflationDecayTimePerSegment;
            if (newInflation <= 0)
            {
               newInflation = 0;
               _state.Value = State.Deflated;
            }
            _inflation.Value = newInflation;
         }
         else if (_state.Value == State.Deflated)
         {
            if (_breathRegenTimer > BreathRegenDelay)
            {
               if (_breath.Value < kMaxBreath)
               {
                  float newBreath = _breath.Value + Time.deltaTime / BreathRegenTimePerSegment;
                  if (newBreath > kMaxBreath)
                     newBreath = kMaxBreath;
                  _breath.Value = newBreath;
               }
            }
         }
      }

      float targetSpikeScale = _state.Value == State.Inflated ? 100 : 0;
      float currentSpikeScale = SpikeRoot.transform.localScale.x;
      SpikeRoot.transform.localScale = Mathf.MoveTowards(currentSpikeScale, targetSpikeScale, 500.0f * Time.deltaTime) * Vector3.one;

      AirMeter.fillAmount = _breath.Value / kMaxBreath;
      InflationMeter.fillAmount = _inflation.Value / kMaxInflation;
      InflationRoot.gameObject.SetActive(_state.Value == State.Inflated);
      if (_state.Value == State.Limp)
      {
         _limpTime += Time.deltaTime;

         Color c = AirMeter.color;
         c.a = Mathf.Sin(_limpTime * 50.0f) / 2 + .5f;
         AirMeter.color = c;
      }
   }

   public void OnMoveInput(Vector2 v)
   {
      _moveInput = v;

      if (_moveInput.x < 0)
      {
         _targetYaw = 90;
         InstantFacingRoot.transform.localRotation = Quaternion.Euler(0, 180, 0);
      }
      else if (_moveInput.x > 0)
      {

         _targetYaw = 270;
         InstantFacingRoot.transform.localRotation = Quaternion.Euler(0, 0, 0);
      }
   }

   public void OnBlowWater()
   {
      if (_state.Value == State.Inflated)
      {
         float newInflation = _inflation.Value - 1f;
         if (newInflation < 0f)
         {
            newInflation = 0f;
            _state.Value = State.Deflated;
         }
         _inflation.Value = newInflation;
         _breathRegenTimer = 0;
      }
      else if (_state.Value == State.Deflated)
      {
         float newBreath = _breath.Value - 1f;
         if (newBreath <= 0)
         {
            newBreath = 0;
            _state.Value = State.Limp;
         }
         _breath.Value = newBreath;
         _breathRegenTimer = 0;
      }
      else
      {
         return;
      }

      DoBlowFxRpc(_targetYaw > 180);
      Collider[] outColliders;
      float[] outDistances;
      Vector3[] outDirections;


      int count = Utl.OverlapCollider(PushCollider, out outColliders, out outDistances, out outDirections);
      for (int i = 0; i < count; i++)
      {
         Collider c = outColliders[i];
         if (c.isTrigger)
         {
            continue;
         }
         GillerPlayer gp = c.GetComponentInParent<GillerPlayer>();
         if (gp && gp != this)
         {
            gp.ReceivePushRpc(transform.position);
         }
      }
   }

   [Rpc(SendTo.Everyone)]
   void DoBlowFxRpc(bool right)
   {
      GillerGameAudioMgr.SafePlay(BlowSFX);
      InstantFacingRoot.transform.localRotation = Quaternion.Euler(0, right ? 0 : 180, 0);
      BlowWaterParticles.Play();
   }

   [Rpc(SendTo.Owner)]
   void ReceivePushRpc(Vector3 source)
   {
      Debug.Log("Get pushed");
      _rigidbody.linearVelocity = (transform.position - source).normalized * 20f;
      if (_getPushedCoroutine != null)
         StopCoroutine(_getPushedCoroutine);
      _getPushedCoroutine = StartCoroutine(DoReceivePush());
   }

   IEnumerator DoReceivePush()
   {
      _isBeingPushed = true;
      yield return new WaitForSeconds(.5f);
      _isBeingPushed = false;
   }

   public void OnInflate()
   {
      if (_state.Value == State.Deflated)
      {
         _state.Value = State.Inflated;
         _inflation.Value = kMaxInflation;
      }
      /*else if (_state.Value == State.Deflated)
      {
         _state.Value = State.Deflated;
         FishAnimator.SetBool("Inflated", false);
      }*/
   }

   public void OnShootSpikes()
   {
      Debug.Log("Spikes");
   }

   public override void OnNetworkSpawn()
   {
      _playerIdx.OnValueChanged += OnChangePlayerIdx;
      _state.OnValueChanged += OnChangeState;

      GillerPlayerMgr.I.RegisterPlayer(this);

      RefreshPlayerSkin();
   }

   private void SceneManager_OnUnload(ulong clientId, string sceneName, AsyncOperation asyncOperation)
   {
      if (IsOwner)
         StartCoroutine(DelayedDespawn(asyncOperation));
   }

   IEnumerator DelayedDespawn(AsyncOperation asyncOperation)
   {
      yield return new WaitUntil(() => { return asyncOperation.isDone; });
      yield return new WaitForSeconds(1.0f);
      NetworkObject.Despawn(true);
   }

   private void OnCollisionEnter(Collision collision)
   {
      if (!IsOwner)
         return;

      if (_state.Value == State.Inflated)
      {
         GillerPlayer otherPlayer = collision.gameObject.GetComponentInParent<GillerPlayer>();
         if (otherPlayer && otherPlayer.IsHurt == false)
         {
            otherPlayer.ReceiveSpikedHitRpc(NetworkObject);
         }
      }
   }


   [Rpc(SendTo.Owner)]
   void ReceiveSpikedHitRpc(NetworkObjectReference source)
   {
      if (_state.Value != State.Inflated && IsHurt == false)
      {
         TakeDamage(1f);
         ChangeColorTemporarilyRpc();
      }
   }

   [Rpc(SendTo.Owner)]
   public void ReceiveEelHitRpc(NetworkObjectReference source)
   {
      if (_state.Value != State.Inflated && IsHurt == false)
      {
         /*NetworkObject o;
         if (source.TryGet(out o))
            ReceivePushRpc(o.transform.position);
         TakeDamage(2f);
         ChangeColorTemporarilyRpc();*/

         NetworkObject.Despawn(true);
      }
   }

   [Rpc(SendTo.Everyone)]
   public void ChangeColorTemporarilyRpc()
   {
      GillerGameAudioMgr.SafePlay(DamagedSFX);
      if (TemporaryMaterial != null)
      {
         StartCoroutine(ChangeMaterialCoroutine());
      }
      else
      {
         Debug.LogWarning("Missing Renderer or Temporary Material reference.");
      }
   }

   void TakeDamage(float amount)
   {
      if (IsOwner)
      {
         float newBreath = _breath.Value - amount;
         if (newBreath <= 0)
         {
            newBreath = 0f;
            _state.Value = State.Limp;
         }
         _breath.Value = newBreath;
         _breathRegenTimer = 0;
      }
   }


    public void RaiseBreath(float amount)
    {
        if (IsOwner)
        {
            float newBreath = _breath.Value + amount;
            if (newBreath > kMaxBreath)
            {
                newBreath = kMaxBreath;
            }
            _breath.Value = newBreath;
        }
    }

   private IEnumerator ChangeMaterialCoroutine()
   {
      Material[] materials = FishRenderer.materials;

      if (materials != null)
      {
         IsHurt = true;
         OriginalMaterial = materials[1];

         for (int i = 0; i < 4; i++)
         {
            materials[1] = TemporaryMaterial;
            FishRenderer.materials = materials;

            yield return new WaitForSeconds(duration);

            materials[1] = OriginalMaterial;
            FishRenderer.materials = materials;

            yield return new WaitForSeconds(duration);

         }
         IsHurt = false;
      }
      else
      {
         Debug.LogWarning("Invalid material or renderer.");
      }
   }


   public override void OnDestroy()
   {
      base.OnDestroy();
      if (GillerPlayerMgr.I)
         GillerPlayerMgr.I.UnregisterPlayer(this);

      if (NetworkManager!=null && NetworkManager.SceneManager!=null)
         NetworkManager.SceneManager.OnUnload -= SceneManager_OnUnload;
   }
}
