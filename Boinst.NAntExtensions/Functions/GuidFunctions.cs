namespace Boinst.NAntExtensions
{
    using System;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    [FunctionSet("guid", "Guid")]
    public class GuidFunctions : FunctionSetBase
    {
        public GuidFunctions(Project project, PropertyDictionary properties)
            : base(project, properties)
        {
        }

        [Function("new-guid")]
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString("D").ToUpperInvariant();
        }
    }
}