using System;

using URandom = UnityEngine.Random;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    /// <summary>
    /// Utility class to allow using a known random seed in a `using` block.
    /// </summary>
    internal sealed class RandomSeedScope : IDisposable
    {
        private readonly URandom.State m_OldState;

        public RandomSeedScope(int seed)
        {
            m_OldState = URandom.state;
            URandom.InitState(seed);
        }

        void IDisposable.Dispose()
        {
            URandom.state = m_OldState;
        }
    }
}