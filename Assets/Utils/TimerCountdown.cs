using UnityEngine;
using System;

namespace MetroPlatform
{
    public class TimerCountdown : MonoBehaviour
    {
        public bool Debugging = false;
        public string Name = "DefaultName";
        [SerializeField] private float _timeTotal;
        [SerializeField] private float _timeCounter;
        
        [field: SerializeField] public bool IsRunning { get; private set; }

        public bool IsFinished => _timeCounter <= 0;
        public float Progress => 1 - (_timeCounter / _timeTotal);
        
        public event Action TimerStart = delegate { };
        public event Action TimerStop = delegate { };
        
        
        
        public TimerCountdown(float timeTotal, string name = "")
        {
            _timeTotal = timeTotal;
            Name = name;
        }
        
        void Start(float newTimeTotal)
        {
            _timeTotal = newTimeTotal;
            Start();
        
        }
        
        void Start()
        {
            if (Debugging)
            {
                Debug.Log($"Time {Name} Start. TimeTotal: {_timeTotal}");
            }
            
            
            if (!IsRunning)
            {
                IsRunning = true;
                TimerStart.Invoke();
            }
        }

        // Update is called once per frame
        void Stop()
        {
            if (Debugging)
            {
                Debug.Log($"Time {Name} Start. TimeTotal: {_timeTotal}");
            }
            
            if (IsRunning)
            {
                IsRunning = false;
                TimerStop.Invoke();
            }
        }

        public void Tick(float deltaTime)
        {
            if (IsRunning && _timeCounter > 0)
            {
                _timeCounter -= deltaTime;
            }

            if (IsRunning && _timeCounter < 0)
            {
                Stop();
            }
        }

        public void Reset()
        {
            _timeCounter = _timeTotal;
        }
        
        public void Pause()
        {
            IsRunning = false;
        }
        
        public void Resume()
        {
            IsRunning = true;
        }
        
        public void SwitchTimeOnOrOff()
        {
            IsRunning = !IsRunning;
        }

        
        
        
    }
}
