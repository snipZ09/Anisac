using UnityEngine;

namespace Game.Combat
{
    [CreateAssetMenu(fileName = "ComboDatabase", menuName = "Scriptable Objects/ComboDatabase")]
    public class ComboDatabase : ScriptableObject
    {
        public ComboData[] combos;
    }
}
