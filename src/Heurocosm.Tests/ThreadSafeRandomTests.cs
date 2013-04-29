using System.Linq;
using Heurocosm.Rnd;
using Xunit;

namespace Heurocosm.Tests
{
    public class ThreadSafeRandomTests
    {
        [Fact]
        public void SeedingWorks()
        {
            ThreadSafeRandom.Seed(42);

            var rng = new ThreadSafeRandom();

            var results = Enumerable.Range(0, 10).Select(_ => rng.Next()).ToList();

            var expected = new int[] { 804318780, 2041643425, 1571945019, 1285609304, 335047478, 334995681, 124733607, 1860099108, 1290884657, 1520574281 };

            Assert.Equal(expected, results);
        }
    }
}