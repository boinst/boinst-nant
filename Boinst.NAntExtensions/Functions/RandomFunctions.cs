namespace Boinst.NAntExtensions
{
    using System;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    [FunctionSet("random", "Random")]
    public class RandomFunctions : FunctionSetBase
    {
        private static readonly Random random = new Random();

        public RandomFunctions(Project project, PropertyDictionary properties)
            : base(project, properties)
        {
        }

        [Function("next")]
        public static int Next(int i)
        {
            return random.Next(i);
        }

        [Function("next-range")]
        public static int Next(int i, int j)
        {
            return random.Next(i, j);
        }
    }
}