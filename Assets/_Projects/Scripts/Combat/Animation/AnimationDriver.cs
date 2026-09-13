using System;
using UnityEngine;

namespace Game.Combat
{
    public class AnimationDriver : MonoBehaviour
    {
        private ActionData _curAnimData;
        private ActionSystem _actionSystem;
        private int _curFrame;
        private float _countDown;
        private ActionData _lastLocomotionData;
        private float _priorityTimer;
        private SpriteRenderer _renderer;
        private bool _isPlaying;
        public event Action OnAnimationDone;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _actionSystem = GetComponent<ActionSystem>();
        }

        // Update is called once per frame
        private void Update()
        {
            bool priorityJustEnded = false;
            if (_priorityTimer > 0f && !float.IsPositiveInfinity(_priorityTimer))
            {
                _priorityTimer -= Time.deltaTime;
                if (_priorityTimer <= 0f)
                {
                    _priorityTimer = 0f;
                    priorityJustEnded = true;
                }
            }

            if (priorityJustEnded && _lastLocomotionData != null && (_actionSystem == null || _actionSystem.CurrentRuntime == null))
            {
                PlayAnimation(_lastLocomotionData);
            }

            if (_curAnimData == null || !_isPlaying) return;

            _countDown += Time.deltaTime;

            if (_countDown >= (1f / _curAnimData.animationFrameRate))
            {
                _countDown = 0;

                if (++_curFrame >= _curAnimData.spriteAnimation.Length)
                {
                    if (_curAnimData.loopAnimation)
                    {
                        _curFrame = 0;
                        _renderer.sprite = _curAnimData.spriteAnimation[_curFrame];
                        return;
                    }

                    _isPlaying = false;
                    OnAnimationDone?.Invoke();
                    return;
                }

                _renderer.sprite = _curAnimData.spriteAnimation[_curFrame];
            }
        }

        private void PlayAnimation(ActionData data)
        {
            _curAnimData = data;
            _curFrame = 0;
            _countDown = 0;
            _renderer.sprite = _curAnimData.spriteAnimation[_curFrame];
            _isPlaying =  true;
        }
        
        public void PlayLocomotionAnimation(ActionData data)
        {
            _lastLocomotionData = data;

            if (_priorityTimer > 0f || (_actionSystem != null && _actionSystem.CurrentRuntime != null))
            {
                return;
            }
            PlayAnimation(data);
        }

        public void PlayActionAnimation(ActionData data)
        {
            PlayAnimation(data);
        }

        public void PlayPriorityAnimation(ActionData data, float duration)
        {
            if (data == null) return;

            _priorityTimer = duration < 0f ? float.PositiveInfinity : duration;
            PlayAnimation(data);
        }
    }
}
