using Game.Shared;

namespace Game.Combat
{
    public readonly struct BufferedInput
    {
        public readonly ActionType ActionType;
        public readonly float Timestamp;

        public BufferedInput(ActionType actionType, float timestamp)
        {
            ActionType = actionType;
            Timestamp = timestamp;
        }
    }
}