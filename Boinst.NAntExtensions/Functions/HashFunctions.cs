namespace Boinst.NAntExtensions
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// The hash functions.
    /// </summary>
    [FunctionSet("hash", "Hash")]
    public class HashFunctions : FunctionSetBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HashFunctions"/> class.
        /// </summary>
        public HashFunctions(Project project, PropertyDictionary properties)
            : base(project, properties)
        {
        }

        /// <summary>
        /// Format a Win32 error code.
        /// </summary>
        /// <example>
        /// <code>
        /// ${hash::sha512("mystring")} 
        /// </code>
        /// </example>
        [Function("sha512")]
        public static string CalculateSha512Hash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hasher = new SHA512Managed();
            var hashBytes = hasher.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
        }

        /// <summary>
        /// Format a Win32 error code.
        /// </summary>
        /// <example>
        /// <code>
        /// ${hash::sha1("mystring")} 
        /// </code>
        /// </example>
        [Function("sha1")]
        public static string CalculateSha1Hash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hasher = new SHA1Managed();
            var hashBytes = hasher.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
