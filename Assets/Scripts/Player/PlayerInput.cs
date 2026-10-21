using UnityEngine;
// ReSharper disable All

namespace MetroPlatform.Player
{
    public class PlayerInput : MonoBehaviour
    {
        public float InputAxisX;
        public bool InputJump;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            InputAxisX = Input.GetAxisRaw("Horizontal");
            InputJump = Input.GetButtonDown("Jump");
        }
    }
}
