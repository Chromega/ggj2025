using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class GillerPlayer : NetworkBehaviour
{

   public enum State
   {
      Deflated,
      Inflated,
      Limp
   }

   [SerializeField]
   float MovementSpeed = 5.0f;

   [SerializeField]
   [Range(0.0f, 1.0f)]
   float Responsiveness = .5f;

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

   [Header("Audio")]
   [SerializeField]
   AudioClip InflateSfx;

   [SerializeField]
   AudioClip DeflateSfx;

   [SerializeField]
   Transform InstantFacingRoot;

   [SerializeField]
   Collider PushCollider;

    [Header("Damage")]
    [SerializeField]
    Material TemporaryMaterial;

    [SerializeField]
    float duration = 0.5f;

    Material OriginalMaterial;

    [SerializeField]
    Renderer FishRenderer;

    bool IsHurt = false;

    #region Synchronized State
    private NetworkVariable<State> _state = new NetworkVariable<State>(State.Deflated);
   #endregion
   //Local state

   #region Local State
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

   #endregion

   private void Start()
   {
      _rigidbody = GetComponent<Rigidbody>();
      _state.OnValueChanged += OnChangeState;
      collider.transform.localScale = 2 * Vector3.one;
      _audioSource = GetComponent<AudioSource>();
    }

   void OnChangeState(State oldState, State newState)
   {
      if (newState == State.Inflated)
         collider.transform.localScale = 4 * Vector3.one;
      else
         collider.transform.localScale = 2 * Vector3.one;

      if (newState == State.Inflated)
      {
         _audioSource.clip = InflateSfx;
         _audioSource.Play();
      }
      else if (oldState == State.Inflated)
      {
         _audioSource.clip = DeflateSfx;
         _audioSource.Play();
      }
   }



   // Update is called once per frame
   void FixedUpdate()
   {
      // IsOwner will also work in a distributed-authoritative scenario as the owner 
      // has the Authority to update the object.
      if (!IsOwner || !IsSpawned) return;

      if (!_isBeingPushed)
      {
         _rigidbody.linearVelocity = Vector3.Lerp(_rigidbody.linearVelocity, _moveInput * MovementSpeed, Utl.TimeInvariantExponentialLerpFactor(Responsiveness));
         //_rigidbody.linearVelocity = _moveInput * 5;
      }
   }

   private void Update()
   {
      if (IsOwner)
      {
         _currentYaw = Mathf.Lerp(_currentYaw, _targetYaw, Utl.TimeInvariantExponentialLerpFactor(.97f));
         FishRoot.transform.rotation = Quaternion.Euler(90, 0, _currentYaw);
      }

      float targetSpikeScale = _state.Value == State.Inflated ? 100 : 0;
      float currentSpikeScale = SpikeRoot.transform.localScale.x;
      SpikeRoot.transform.localScale = Mathf.MoveTowards(currentSpikeScale, targetSpikeScale, 500.0f * Time.deltaTime) * Vector3.one;
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
         FishAnimator.SetBool("Inflated", true);
      }
      else
      {
         _state.Value = State.Deflated;
         FishAnimator.SetBool("Inflated", false);
      }
   }

   public void OnShootSpikes()
   {
      Debug.Log("Spikes");
   }

   public override void OnNetworkSpawn()
   {
      if (IsOwner)
      {
         GillerInputMgr.I.RegisterLocalPlayer(this);
      }
      //Temporary
      DontDestroyOnLoad(this);
   }

   private void OnCollisionEnter(Collision collision)
   {
      if (!IsOwner)
         return;

      if (_state.Value == State.Inflated)
      {
         GillerPlayer otherPlayer = collision.gameObject.GetComponentInParent<GillerPlayer>();
         if (otherPlayer)
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
         Debug.Log("Damaged!");
            ChangeColorTemporarily();
      }
   }

    public void ChangeColorTemporarily()
    {

        if (TemporaryMaterial != null)
        {
            StartCoroutine(ChangeMaterialCoroutine());
        }
        else
        {
            Debug.LogWarning("Missing Renderer or Temporary Material reference.");
        }
    }

    private IEnumerator ChangeMaterialCoroutine()
    {
        Material[] materials = FishRenderer.materials;

        if (materials != null)
        {
            IsHurt = true;
            OriginalMaterial = materials[1];

            materials[1] = TemporaryMaterial;
            FishRenderer.materials = materials;

            yield return new WaitForSeconds(duration);

            materials[1] = OriginalMaterial;
            FishRenderer.materials = materials;
            IsHurt = false;
        }
        else
        {
            Debug.LogWarning("Invalid material or renderer.");
        }
    }
}
