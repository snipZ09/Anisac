using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class InputBuffer : MonoBehaviour
    {
        private Queue<BufferedInput> bufferedInputs = new();

        [SerializeField] private float windowTime = 0.5f;
        [SerializeField] private int maxEntries = 50;


        public void Push(BufferedInput input)
        {
            if (bufferedInputs.Count >= maxEntries)
            {
                bufferedInputs.Dequeue();
            }
            bufferedInputs.Enqueue(input);
        }

        public IReadOnlyList<BufferedInput> GetValid()
        {
            List<BufferedInput> result = new();
            foreach (BufferedInput input in bufferedInputs)
            {
                if (Time.time - input.Timestamp <= windowTime)
                {
                    result.Add(input);
                }
            }

            return result;
        }

        public void ConsumeAll()
        {
            bufferedInputs.Clear();
        }
    }
}