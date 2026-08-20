using System;

namespace Game.Combat
{
    public interface IActionRunner
    {
        ActionRuntime CurrentRuntime { get; }
        event Action<ActionRuntime> OnActionStarted;
    }
}
