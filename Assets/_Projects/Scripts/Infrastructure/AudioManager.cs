using UnityEngine;

namespace Game.Infrastructure
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private int sfxPoolSize = 8;

        private AudioSource _musicSource;
        private AudioSource[] _sfxPool;
        private int _nextPoolIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;

            _sfxPool = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxPoolSize; i++)
            {
                AudioSource src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _sfxPool[i] = src;
            }
        }

        private void Start()
        {
            if (backgroundMusic != null)
            {
                PlayMusic(backgroundMusic);
            }
        }

        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            GetAvailableSource().PlayOneShot(clip, volume);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        private AudioSource GetAvailableSource()
        {
            foreach (AudioSource src in _sfxPool)
            {
                if (!src.isPlaying) return src;
            }

            AudioSource fallback = _sfxPool[_nextPoolIndex];
            _nextPoolIndex = (_nextPoolIndex + 1) % _sfxPool.Length;
            return fallback;
        }
    }
}
