using System.Collections.Generic;
using System.Linq;
using Game.Shared;
using UnityEngine;

namespace Game.Combat
{
    public class ComboResolver
    {
        private ComboDatabase _comboDatabase;

        public ComboResolver(ComboDatabase comboDatabase)
        {
            _comboDatabase = comboDatabase;
        }

        public ActionData Resolve(IReadOnlyList<BufferedInput> inputs)
        {
            ComboData bestCombo = null;
            foreach (ComboData combo in _comboDatabase.combos)
            {
                if (IsComboMatch(combo, inputs))
                {
                    if (IsInputMatchTiming(inputs, combo.maxGapBetweenInputs))
                    {
                        if (bestCombo != null)
                        {
                            if (bestCombo.priority > combo.priority)
                            {
                                bestCombo = combo;
                            }
                            else if (bestCombo.priority == combo.priority)
                            {
                                bestCombo = bestCombo.inputActionTypes.Length > combo.inputActionTypes.Length
                                    ? bestCombo
                                    : combo;
                            }

                            continue;
                        }

                        bestCombo = combo;
                    }
                }
            }

            return bestCombo?.resultActionData;
        }

        private bool IsComboMatch(ComboData combo, IReadOnlyList<BufferedInput> inputs)
        {
            return combo.inputActionTypes.Length <= inputs.Count &&
                   combo.inputActionTypes.Select((x, i) => x == inputs[i].ActionType)
                       .All(result => result);
        }

        private bool IsInputMatchTiming(IReadOnlyList<BufferedInput> inputs, float maxGap)
        {
            for (int i = 0; i < inputs.Count - 1; i++)
            {
                if (inputs[i + 1].Timestamp - inputs[i].Timestamp > maxGap)
                    return false;
            }

            return true;
        }
    }
}