using Game.Shared;
using UnityEngine;

namespace Game.Combat
{
    [CreateAssetMenu(fileName = "CombatData", menuName = "Scriptable Objects/CombatData")]
    public class ComboData : ScriptableObject
    {
        public ActionType[] inputActionTypes;
        public ActionData resultActionData;
        public int priority;
        public float maxGapBetweenInputs;
    }
}
