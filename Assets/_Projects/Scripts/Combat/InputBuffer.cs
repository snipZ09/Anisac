using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class InputBuffer : MonoBehaviour
    {
        private List<BufferedInput> bufferedInputs;
        private float windowTimer;
        private float windowTime = 0.5f;

        private void Update()
        {
            if (bufferedInputs.Count > 0)
            {
                windowTimer += Time.deltaTime;
                if (windowTimer >= windowTime)
                {
                    ConsumeAll();
                }
            }
        }

        public void Push(BufferedInput input)
        {
            if (bufferedInputs.Count == 0)
            {
                windowTimer = 0;
            }
            bufferedInputs.Add(input);
        }
        
        public List<BufferedInput> GetValid()
        {
            return bufferedInputs;
        }

        public void ConsumeAll()
        {
            bufferedInputs.Clear();
        }
    }
}