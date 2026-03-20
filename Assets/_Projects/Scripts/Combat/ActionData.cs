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
        [Header("Phases")] public float timeStartUp;
        public float timeActive;
        public float timeRecovery;
        [Header("Cancel")] public bool canCancel;
        public float cancelWindowStart;
        public float cancelWindowEnd;
        
        public float TotalDuration => timeActive + timeStartUp + timeRecovery;
    }
}