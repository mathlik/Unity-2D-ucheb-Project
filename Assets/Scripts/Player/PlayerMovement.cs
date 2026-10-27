using UnityEngine;
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
        
        [SerializeField] private float _coyotaTimeTotal = 0.15f;
        [SerializeField] private float _coyotaTimeCounter;
        
        
        // Serializing for debug purposes
        [Header("Inner Workings")]
        [SerializeField] private float _moveHorizontal;
        //jump
        private bool _canJump;
        [SerializeField] private bool _grounded;
        [SerializeField] private Collider2D _groundHit;



        [SerializeField] private float _moveVertical;
        [SerializeField] private bool _jumping;
        [SerializeField] private float _jumpCounter;
        
        private float _inputAxisX => _playerInput.InputAxisX;
        private bool _inputJump => _playerInput.InputJump;
        
        [Header("References")]
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private Transform _visuals;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D  _groundCheckColl;
        
        private static readonly int AnimVelocityX = Animator.StringToHash("VelocityX");
        private static readonly int AnimJumping = Animator.StringToHash("Jumping");


        private void Start()
        {
            if(_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody2D>();
            if(_animator == null)
                _animator = _visuals.GetComponent<Animator>();
            if (_playerInput == null)
                _playerInput = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            // Input
            _inputBufferJump(_inputJump);
            OnJump(_inputBufferJumpCounter > 0);
            
            _canJump = CanJump();
            
            //Jump timer
            if (_jumping)
            {
                _jumpCounter -= Time.deltaTime;
                if (_jumpCounter <= 0f)
                {
                    _jumping = false;
                }
            }
            
            //Visuals

            Animate();
            Flip(_inputAxisX);

        }
        
        private void FixedUpdate()
        {
            //Walk and run
            _moveHorizontal = FixedMove(_inputAxisX); 
            
            //Jump
            _moveVertical = FixedJump(_jumping);
            
            //Finally using force
            _rigidbody.linearVelocity = new Vector2(_moveHorizontal, _moveVertical); 
        }

        //Visuals
        private void Animate()
        {
            _animator.SetFloat(AnimVelocityX, Mathf.Abs(_inputAxisX));
            _animator.SetBool(AnimJumping, !_grounded);
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
                _coyotaTimeCounter = _coyotaTimeTotal;
            }
            else if (_coyotaTimeCounter > 0)
            {
                _coyotaTimeCounter -= Time.deltaTime;
            }

            return _coyotaTimeCounter > 0;
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
        
        private float FixedJump(bool jumping)
        {
            if (jumping)
            {
                var progress = _jumpCounter / _jumpTimeTotal;
                Debug.Log($"progress : {progress}");
                return _jumpForce;
            }

            return 0f;
        }
        
        
        // Events
        private void OnJump(bool inputJump)
        {
            if (inputJump && _canJump)
            {
                _jumping = true;
                _jumpCounter = _jumpTimeTotal;
            }
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

