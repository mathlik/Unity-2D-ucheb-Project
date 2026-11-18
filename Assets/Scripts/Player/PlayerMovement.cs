using UnityEngine;
using System.Collections.Generic;
// ReSharper disable All

namespace MetroPlatform.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement settings")] 
        [SerializeField] private float _walkSpeed = 6f;
        
        [Header("Jump settings")] 
        [SerializeField] private float _jumpForce = 9f;
        [SerializeField] private LayerMask _jumpLayerMask;
        [SerializeField] private float _jumpTimeTotal = 0.15f;
        [SerializeField] private float _inputBufferJumpTotal = 0.15f;
        [SerializeField] private float _inputBufferJumpCounter;
        [SerializeField] private float _jumpFallAcceleration = 0.5f;
        
        [SerializeField] private float _coyotaTimeTotal = 0.15f;
        [SerializeField] private float _coyotaTimeCounter;
        
        
        // Serializing for debug purposes
        [Header("Inner Workings")]
        [SerializeField] private float _moveHorizontal;
        
        //jump
        [SerializeField] private float _jumpFallForceActing;
        [SerializeField] private bool _canJump;
        [SerializeField] private bool _grounded;
        [SerializeField] private Collider2D _groundHit;



        [SerializeField] private float _moveVertical;
        
        
        private float _inputAxisX => _playerInput.InputAxisX;
        private bool _inputJump => _playerInput.InputJump;
        
        
        //Timers
        [SerializeField] private TimerCountdown _timerJump;
        [SerializeField] private TimerCountdown _timerFall;
        [SerializeField] private TimerCountdown _timerCoyoteTime;
        [SerializeField] private List<TimerCountdown> _timers;
        
        [Header("References")]
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private Transform _visuals;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D  _groundCheckColl;
        
        private static readonly int AnimVelocityX = Animator.StringToHash("VelocityX");
        private static readonly int AnimJumping = Animator.StringToHash("Jumping");
        private static readonly int AnimFalling = Animator.StringToHash("Falling");


        private void Awake()
        {
            if(_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody2D>();
            if(_animator == null)
                _animator = _visuals.GetComponent<Animator>();
            if (_playerInput == null)
                _playerInput = GetComponent<PlayerInput>();
            InitializeTimers();
        }
        
        private void InitializeTimers()
        {
            _timerJump = new TimerCountdown(_jumpTimeTotal, "_timerJump");
            _timerFall = new TimerCountdown(999f, "_timerFall");
            _timerCoyoteTime = new TimerCountdown(_coyotaTimeTotal, "_timerCoyoteTime");
            
            //Events
            _timerJump.TimerStart += () =>
            {
                _animator.SetBool(AnimJumping, _timerJump.IsRunning);
            };
            _timerJump.TimerStop += () =>
            {
                _timerFall.Start();
                _animator.SetBool(AnimJumping, _timerJump.IsRunning);
                _animator.SetBool(AnimFalling, _timerFall.IsRunning);
            };
            _timerFall.TimerStart += () => _jumpFallForceActing = 0f;
            
            _timerFall.TimerStop += () =>
            {
                _animator.SetBool(AnimFalling, _timerFall.IsRunning);
            };

            _timers = new List<TimerCountdown>(3)
            {
                _timerJump,
                _timerFall,
                _timerCoyoteTime
            };
        }

        private void Update()
        {
            _inputBufferJump(_inputJump);
            _canJump = CanJump();
            OnJump(_inputBufferJumpCounter > 0f);

            UpdateTimers();

            Animate();
            Flip(_inputAxisX);
        }

        private void UpdateTimers()
        {
            foreach (var timer in _timers)
            {
                if (timer.IsRunning)
                    timer.Tick(Time.deltaTime);
            }
        }
        
        private void FixedUpdate()
        {
            //Walk and run
            _moveHorizontal = FixedMove(_inputAxisX); 
            
            //Jump
            if (_timerJump.IsRunning)
                _moveVertical = FixedJump();
            //Fall
            else if (_timerFall.IsRunning)
                _moveVertical = FixedFall();
            
            //Check if we should change Character state
            ChangeTimersState();
            
            //Finally using force
            _rigidbody.linearVelocity = new Vector2(_moveHorizontal, _moveVertical); 
        }
        
        private void ChangeTimersState()
        {
            //Stop
            if (_timerFall.IsRunning && _grounded)
                _timerFall.Stop();

            //Stop jumping if grounded
            if (_timerJump.IsRunning && _grounded && _timerJump.Progress > 0.3f)
                _timerJump.Stop();
        }

        //Visuals
        private void Animate()
        {
            _animator.SetFloat(AnimVelocityX, Mathf.Abs(_inputAxisX));
        }

        private void Flip(float _inputAxisX)
        {
            if (_inputAxisX >= 0)
            {
                transform.localEulerAngles = Vector3.zero;
            }
            else 
            {
                transform.localEulerAngles = new Vector3(0, -180, 0);
            }
        }

        private void _inputBufferJump(bool inputJump)
        {
            if (inputJump)
            {
                _inputBufferJumpCounter = _inputBufferJumpTotal;
            }
            else if (_inputBufferJumpCounter > 0)
            {
                _inputBufferJumpCounter -= Time.deltaTime;
            }
        }
        
        private bool CanJump()
        {
            _grounded = Grounded();
            if (_grounded)
            {
                _timerCoyoteTime.Reset();
                _timerCoyoteTime.Start();
            }

            return _timerCoyoteTime.IsRunning && !_timerJump.IsRunning;
        }


        
        
        /// <summary>
        /// //Walk and run
        /// </summary>
        /// <param name="inputAxis"></param>
        /// <returns></returns>
        private float FixedMove(float inputAxis)
        {
            return inputAxis * _walkSpeed;
        }
        
        private float FixedJump()
        {
            return _jumpForce * _timerJump.ProgressReversed;
        }
        
        private float FixedFall()
        {
            _jumpFallForceActing -= _jumpFallAcceleration;
            return _jumpFallForceActing;
        }
        
        
        // Events
        private void OnJump(bool inputJump)
        {
            if (inputJump && _canJump && !_timerJump.IsRunning)
                _timerJump.Start();
        }
        
        // Checks
        private bool Grounded()

        {
            //RaycastHit2D raycastHit = Physics2D.Raycast(_groundCheckT.position,Vector2.down, 0.05f, _jumpLayerMask);
            RaycastHit2D raycastHit = Physics2D.BoxCast(_groundCheckColl.bounds.center, _groundCheckColl.bounds.size, 0f, Vector2.down, 0.1f,
                _jumpLayerMask);
            _groundHit = raycastHit.collider;
            
            Debug.DrawRay(_groundCheckColl.bounds.center + new Vector3(-_groundCheckColl.bounds.extents.x, 0), Vector2.down * (_groundCheckColl.bounds.extents.y + 0.1f), Color.red, 1f);
            return raycastHit.collider != null;
        }


    }
}

