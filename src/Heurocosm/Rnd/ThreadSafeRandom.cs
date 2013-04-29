using System;
using System.Linq;
using System.Threading;

namespace Heurocosm.Rnd
{
    public class ThreadSafeRandom : Random
    {
        private static ISeedProvider global = new RNGCryptoSeedProvider();

        private ThreadLocal<Random> local = new ThreadLocal<Random>(() =>
        {
            return new MersenneTwister(global.GetSeed());
        });

        public static void Seed(int seed)
        {
            global = new ConstantSeedProvider(seed);
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>A 32-bit signed integer greater than or equal to zero and less than System.Int32.MaxValue.</returns>
        public override int Next()
        {
            return local.Value.Next();
        }

        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to zero.</param>
        /// <returns>A 32-bit signed integer greater than or equal to zero, and less than maxValue; that is, the range of return values ordinarily includes zero but not maxValue. However, if maxValue equals zero, maxValue is returned.</returns>
        public override int Next(int maxValue)
        {
            return local.Value.Next(maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number greater than or equal to specified minimum and less than specified maximum
        /// </summary>
        /// <param name="minValue"> The inclusive lower bound or random number to be generated.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A 32-bit signed integer greater than or equal to minValue, and less than maxValue; that is, the range of return values ordinarily includes zero but not maxValue. However, if maxValue equals zero, maxValue is returned. </returns>
        public override int Next(int minValue, int maxValue)
        {
            return local.Value.Next(minValue, maxValue);
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
        public override double NextDouble()
        {
            return local.Value.NextDouble();
        }

        public double NextDouble(double minValue, double maxValue)
        {
            return (maxValue - minValue) * local.Value.NextDouble() + minValue;
        }

        public int[] ShuffleNumbers(int start, int count)
        {
            // <pex>
            if (count < 0)
                throw new ArgumentException("count < 0", "count");

            // </pex>
            int[] shuffle = Enumerable.Range(start, count).ToArray();

            for (int i = 0; i < count; i++)
            {
                int n = local.Value.Next(i + 1);

                //Swap
                int temp = shuffle[i];
                shuffle[i] = shuffle[n];
                shuffle[n] = temp;
            }

            return shuffle;
        }
    }
}