using UnityEngine;

namespace Game.Combat
{
    public class AutoDestroyVfx : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 0.5f;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }
    }
}
