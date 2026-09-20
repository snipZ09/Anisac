using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSequenceVfx : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float animationFrameRate = 24f;

        private SpriteRenderer _spriteRenderer;
        private int _currentFrame;
        private float _frameTimer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (frames == null || frames.Length == 0)
            {
                Destroy(gameObject);
                return;
            }

            _spriteRenderer.sprite = frames[0];
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;

            _frameTimer += Time.deltaTime;
            if (_frameTimer < 1f / animationFrameRate) return;

            _frameTimer = 0f;
            _currentFrame++;

            if (_currentFrame >= frames.Length)
            {
                Destroy(gameObject);
                return;
            }

            _spriteRenderer.sprite = frames[_currentFrame];
        }
    }
}
