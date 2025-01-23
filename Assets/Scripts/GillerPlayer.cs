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

   #endregion

   private void Start()
   {
      _rigidbody = GetComponent<Rigidbody>();
      _state.OnValueChanged += OnChangeState;
      collider.transform.localScale = 2 * Vector3.one;
   }

   void OnChangeState(State oldState, State newState)
   {
      if (newState == State.Inflated)
         collider.transform.localScale = 4 * Vector3.one;
      else
         collider.transform.localScale = 2 * Vector3.one;
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
      _currentYaw = Mathf.Lerp(_currentYaw, _targetYaw, Utl.TimeInvariantExponentialLerpFactor(.97f));
      FishRoot.transform.rotation = Quaternion.Euler(90, 0, _currentYaw);
   }

   public void OnMoveInput(Vector2 v)
   {
      _moveInput = v;

      if (_moveInput.x < 0)
         _targetYaw = 90;
      else if (_moveInput.x > 0)
         _targetYaw = 270;
   }

   public void OnBlowWater()
   {
      Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
      for (int i = 0; i < colliders.Length; i++)
      {
         Collider c = colliders[i];
         GillerPlayer gp = c.GetComponentInParent<GillerPlayer>();
         if (gp && gp != this)
         {
            gp.ReceivePushRpc(transform.position);
         }
      }
   }

   [Rpc(SendTo.Owner)]
   void ReceivePushRpc(Vector3 source)
   {
      _rigidbody.linearVelocity = (transform.position - source).normalized * 4f;
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
   /*
   private void OnCollisionEnter(Collision collision)
   {
      if (!IsOwner)
         return;

      if (_state.Value == State.Inflated)
      {
         GillerPlayer otherPlayer = collision.gameObject.GetComponentInParent<GillerPlayer>();
         if (otherPlayer)
         {
            otherPlayer.ReceiveSpikedHitRpc(this);
         }
      }
   }


   [Rpc(SendTo.Owner)]
   void ReceiveSpikedHitRpc(GillerPlayer source)
   {
      if (_state.Value != State.Inflated)
      {
         Debug.Log("Damaged!");
      }
   }*/
}
