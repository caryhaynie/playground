namespace PlayfulSoftware.HexMaps.Hybrid
{
    public static class HexMapConstants
    {
        // These ratios are intrinsic to hexmaps, so they can't be changed.
        public const float outerToInner = 0.866025404f;
        public const float innerToOuter = 1f / outerToInner;
    }
}