using System;
using System.Security.Cryptography;

namespace Heurocosm.Rnd
{
    public interface ISeedProvider
    {
        int GetSeed();
    }

    public class ConstantSeedProvider : ISeedProvider
    {
        private readonly int seed;

        public ConstantSeedProvider(int seed)
        {
            this.seed = seed;
        }

        public int GetSeed()
        {
            return seed;
        }
    }

    public class RNGCryptoSeedProvider : ISeedProvider, IDisposable
    {
        private readonly RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();

        public int GetSeed()
        {
            byte[] buffer = new byte[4];
            provider.GetBytes(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (provider != null)
                    provider.Dispose();
        }

        ~RNGCryptoSeedProvider()
        {
            Dispose(false);
        }
    }
}