using Game.Shared;
using UnityEngine;

namespace Game.Combat
{
    [CreateAssetMenu(fileName = "ActionData", menuName = "Scriptable Objects/ActionData")]
    public class ActionData : ScriptableObject
    {
        public ActionType actionType;
        [Header("Animation")] public Sprite[] spriteAnimation;
        public float animationFrameRate;
        public bool loopAnimation;
        [Header("Phases")] public float timeStartUp;
        public float timeActive;
        public float timeRecovery;
        [Header("Cancel")] public bool canCancel;
        public float cancelWindowStart;
        public float cancelWindowEnd;

        [Header("Combat")] public int hitboxIndex;
        public float damageAmount;
        public DamageType damageType;

        public float TotalDuration => timeActive + timeStartUp + timeRecovery;
    }
}