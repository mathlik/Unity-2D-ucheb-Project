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
        
        
        // Serializing for debug purposes
        [Header("Inner Workings")]
        [SerializeField] private float _moveHorizontal;
        //jump
        private bool _canJump => Grounded();



        [SerializeField] private float _moveVertical;
        [SerializeField]private bool _jumping;
        [SerializeField]private float _jumpCounter;
        
        private float _inputAxisX => _playerInput.InputAxisX;
        private bool _inputJump => _playerInput.InputJump;
        
        [Header("References")]
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Transform _groundCheckT;
        


        private void Start()
        {
            if(_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody2D>();
            if (_playerInput == null)
                _playerInput = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            OnJump(_inputJump);
            
            //Jump timer
            if (_jumping)
            {
                _jumpCounter -= Time.deltaTime;
                if (_jumpCounter <= 0f)
                {
                    _jumping = false;
                }
            }
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
            RaycastHit2D raycastHit = Physics2D.Raycast(_groundCheckT.position,
                Vector2.down, 0.05=0f, _jumpLayerMask);
            
            Debug.DrawRay(_groundCheckT.position, Vector2.down, Color.red, 2f);
            return raycastHit.collider != null;
        }
        

    }
}

