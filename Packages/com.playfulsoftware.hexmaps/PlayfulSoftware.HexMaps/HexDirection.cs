namespace PlayfulSoftware.HexMaps
{
    public enum HexDirection
    {
        NE,
        E,
        SE,
        SW,
        W,
        NW
    }

    public static class HexDirectionExtensions
    {
        public static HexDirection Opposite(this HexDirection direction)
        {
            return (int)direction < 3 ? (direction + 3) : (direction - 3);
        }

        public static HexDirection Previous(this HexDirection direction)
            => direction.PreviousBySteps(1);

        public static HexDirection Next(this HexDirection direction)
            => direction.NextBySteps(1);

        public static HexDirection Previous2(this HexDirection direction)
            => direction.PreviousBySteps(2);

        public static HexDirection Next2(this HexDirection direction)
            => direction.NextBySteps(2);

        public static int ToRiverMask(this HexDirection dir)
            => 1 << (int) dir;

        public static bool IsMaskSet(this HexDirection dir, byte mask)
            => (mask & dir.ToRiverMask()) != 0;

        static HexDirection PreviousBySteps(this HexDirection direction, int steps)
        {
            direction -= steps;
            return direction >= HexDirection.NE ? direction : (direction + 6);
        }

        static HexDirection NextBySteps(this HexDirection direction, int steps)
        {
            direction += steps;
            return direction <= HexDirection.NW ? direction : (direction - 6);
        }
    }
}