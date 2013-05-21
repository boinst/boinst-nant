namespace Boinst.NAntExtensions
{
    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// Functions that operate on integers
    /// </summary>
    [FunctionSet("win32", "Win32")]
    public class Win32Functions : FunctionSetBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Win32Functions"/> class.
        /// </summary>
        public Win32Functions(Project project, PropertyDictionary propDict)
            : base(project, propDict)
        {
        }

        /// <summary>
        /// Format a Win32 error code.
        /// </summary>
        /// <example>
        /// <code>
        /// ${win32::format-win32-error(0)} ==&gt; 0 (0): No Error
        /// </code>
        /// </example>
        [Function("format-win32-error")]
        public static string FormatWin32Error(int result)
        {
            string msg = string.Format("{0} ({1})", result, result.ToString("X"));
            string errorDetail = result == 0 ? "No Error" : GetLastWin32Error();
            if (!string.IsNullOrWhiteSpace(errorDetail)) msg += ": " + errorDetail;
            return msg;
        }

        [Function("get-last-win32-error")]
        public static string GetLastWin32Error()
        {
            string errmsg = new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error()).Message;
            return string.IsNullOrWhiteSpace(errmsg) ? string.Empty : errmsg;
        }
    }
}
