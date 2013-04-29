using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Heurocosm.Rnd;
using Xunit;

namespace Heurocosm.Tests
{
    public class ShakespeareanMonkeysTests : ISpawner<string>, IFitnessCalculator<string>, ICrossoverOperator<string>, IMutatorOperator<string>
    {
        private readonly static char[] validChars;
        private readonly static string target;

        static ShakespeareanMonkeysTests()
        {
            // Initialize the valid characters to newlines plus all the alphanumerics and symbols
            validChars = new char[2 + (127 - 32)];
            validChars[0] = (char)10;
            validChars[1] = (char)13;
            for (int i = 2, pos = 32; i < validChars.Length; i++, pos++)
            {
                validChars[i] = (char)pos;
            }

            target = "To be or not to be, that is the question;" + Environment.NewLine +
            "Whether 'tis nobler in the mind to suffer" + Environment.NewLine +
            "The slings and arrows of outrageous fortune," + Environment.NewLine +
            "Or to take arms against a sea of troubles," + Environment.NewLine +
            "And by opposing, end them.";

            // Seed random number generator to provide consistent results.
            ThreadSafeRandom.Seed(42);
        }

        public string CreateItem(Random rnd)
        {
            var sb = new StringBuilder(target.Length);
            for (int i = 0; i < target.Length; i++)
            {
                sb.Append(validChars[rnd.Next(0, validChars.Length)]);
            }
            return sb.ToString();
        }

        public double CalculateFitness(string item)
        {
            if (item != null && target != null)
            {
                int diffs = 0;
                for (int i = 0; i < target.Length; i++)
                {
                    if (target[i] != item[i]) diffs++;
                }
                return diffs;
            }
            else
                return Int32.MaxValue;
        }

        public string Crossover(Random rnd, string item1, string item2)
        {
            int crossoverPoint = rnd.Next(1, item1.Length);
            return item1.Substring(0, crossoverPoint) + item2.Substring(crossoverPoint);
        }

        public string Mutate(Random rnd, string item)
        {
            var sb = new StringBuilder(item);

            for (int i = 0; i < item.Length; i++)
            {
                if (item[i] != target[i])
                {
                    sb[i] = validChars[rnd.Next(0, validChars.Length)];
                    break;
                }
            }
            return sb.ToString();
        }

        [Fact]
        public async Task CanMonkeysCreateShakespeare()
        {
            var engine = new Engine<string>(this, this, this, this);

            var result = await engine.Run(CancellationToken.None);

            Assert.Equal(target, result);
        }
    }
}