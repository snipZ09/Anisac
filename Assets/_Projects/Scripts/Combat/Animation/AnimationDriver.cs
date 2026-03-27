using System;
using UnityEngine;

namespace Game.Combat
{
    public class AnimationDriver : MonoBehaviour
    {
        private ActionData _curAnimData;
        private int _curFrame;
        private float _countDown;
        private SpriteRenderer _renderer;
        private bool _isPlaying;
        public event Action OnAnimationDone;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (_curAnimData == null || !_isPlaying) return;

            _countDown += Time.deltaTime;

            if (_countDown >= (1f / _curAnimData.animationFrameRate))
            {
                _countDown = 0;
                
                if (++_curFrame >= _curAnimData.spriteAnimation.Length)
                {
                    _isPlaying = false;
                    OnAnimationDone?.Invoke();
                    return;
                }
                
                _renderer.sprite = _curAnimData.spriteAnimation[_curFrame];
            }
        }

        public void PlayAnimation(ActionData data)
        {
            _curAnimData = data;
            _curFrame = 0;
            _countDown = 0;
            _renderer.sprite = _curAnimData.spriteAnimation[_curFrame];
            _isPlaying =  true;
        }
    }
}