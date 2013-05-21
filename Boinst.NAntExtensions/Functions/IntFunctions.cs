namespace Boinst.NAntExtensions
{
    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// Functions that operate on integers
    /// </summary>
    [FunctionSet("int", "Int")]
    public class IntFunctions : FunctionSetBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntFunctions"/> class.
        /// </summary>
        public IntFunctions(Project project, PropertyDictionary propDict)
            : base(project, propDict)
        {
        }
        
        /// <summary>
        /// Returns the given integer formated as hexadecimal
        /// </summary>
        /// <param name="val">
        /// The integer value to format as hexadecimal.
        /// </param>
        /// <example>
        /// <code>
        /// ${int::to-hexadecimal(-1073741515)} ==&gt; C0000135
        /// </code>
        /// </example>
        [Function("to-hexadecimal")]
        public static string ToHexadecimal(int val)
        {
            return val.ToString("X");
        }
    }
}
