namespace Pancake.Physics
{
    public struct RaycastTicket
    {
        public int value;
        public bool fixedUpdateLoop;

        public static readonly RaycastTicket Invalid = new() {value = -1};

        public RaycastTicket(int value, bool fixedUpdateLoop)
        {
            this.value = value;
            this.fixedUpdateLoop = fixedUpdateLoop;
        }

        public bool IsValid() { return value >= 0; }
    }
}