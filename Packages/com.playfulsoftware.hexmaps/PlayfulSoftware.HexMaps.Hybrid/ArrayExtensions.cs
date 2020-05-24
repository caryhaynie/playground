namespace PlayfulSoftware.HexMaps.Hybrid
{
    static class ArrayExtensions
    {
        public static bool IsNullOrEmpty<T>(this T[] self)
            => self == null || self.Length == 0;
    }
}