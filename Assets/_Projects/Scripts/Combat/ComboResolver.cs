using System.Collections.Generic;

namespace Game.Combat
{
    public class ComboResolver
    {
        private ComboDatabase _comboDatabase;

        public ComboResolver(ComboDatabase comboDatabase)
        {
            _comboDatabase = comboDatabase;
        }

        public ActionData Resolve(IReadOnlyList<BufferedInput> inputs, ActionData previousAction)
        {
            ComboData bestCombo = null;
            foreach (ComboData combo in _comboDatabase.combos)
            {
                if (IsComboMatch(combo, inputs))
                {
                    if (IsInputMatchTiming(inputs, combo.maxGapBetweenInputs, combo.inputActionTypes.Length))
                    {
                        if (combo.requiredPreviousAction != null && combo.requiredPreviousAction != previousAction)
                        {
                            continue;
                        }   
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
            bool isMatch = true;
            if (combo.inputActionTypes.Length > inputs.Count)
            {
                isMatch = false;
            }
            else
            {
                for (int i = 0; i < combo.inputActionTypes.Length; i++)
                {
                    var comboValue = combo.inputActionTypes[combo.inputActionTypes.Length - 1 - i];
                    var inputValue = inputs[inputs.Count - 1 - i].ActionType;
                    if (comboValue != inputValue)
                    {
                        isMatch = false;
                        break;
                    }
                }
            }

            return isMatch;
        }

        private bool IsInputMatchTiming(IReadOnlyList<BufferedInput> inputs, float maxGap, int comboLength)
        {
            for (int i = inputs.Count - comboLength; i < inputs.Count - 1; i++)
            {
                if (inputs[i + 1].Timestamp - inputs[i].Timestamp > maxGap)
                    return false;
            }

            return true;
        }
    }
}