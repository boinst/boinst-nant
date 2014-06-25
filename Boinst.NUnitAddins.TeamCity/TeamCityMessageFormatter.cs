namespace Boinst.NUnitAddins.TeamCity
{
    using System.Text;

    /// <summary>
    /// This class contains the methods for formatting 
    /// </summary>
    public static class TeamCityMessageFormatter
    {
        public static string FormatTestFailedMessage(string testname, string message, string detail)
        {
            testname = EscapeInvalidCharacters(testname);
            message = EscapeInvalidCharacters(message);
            detail = EscapeInvalidCharacters(detail);

            return string.Format("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", testname, message, detail);
        }

        public static string FormatTestStartedMessage(string testname)
        {
            testname = EscapeInvalidCharacters(testname);
            return string.Format("##teamcity[testStarted name='{0}']", testname);
        }

        public static string FormatSuiteStartedMessage(string suiteName)
        {
            suiteName = EscapeInvalidCharacters(suiteName);
            return string.Format("##teamcity[testSuiteStarted name='{0}']", suiteName);
        }

        public static string FormatSuiteFinishedMessage(string suiteName)
        {
            suiteName = EscapeInvalidCharacters(suiteName);
            return string.Format("##teamcity[testSuiteFinished name='{0}']", suiteName);
        }

        public static string FormatTestFinishedMessage(string testname)
        {
            testname = EscapeInvalidCharacters(testname);
            return string.Format("##teamcity[testFinished name='{0}']", testname);
        }

        public static string FormatTestErrorMessage(string testname, string text)
        {
            if (text.ToLowerInvariant().Contains("##teamcity")) return text.Trim();

            testname = EscapeInvalidCharacters(testname);
            text = EscapeInvalidCharacters(text);
            return string.Format("##teamcity[testStdErr name='{0}' out='{1}']", testname, text);
        }

        public static string FormatTestOutputMessage(string testname, string text)
        {
            if (text.ToLowerInvariant().Contains("##teamcity")) return text.Trim();

            testname = EscapeInvalidCharacters(testname);
            text = EscapeInvalidCharacters(text);
            return string.Format("##teamcity[testStdOut name='{0}' out='{1}']", testname, text);
        }

        public static string EscapeInvalidCharacters(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) 
                return string.Empty;

            var builder = new StringBuilder(text.Trim());
            builder.Replace("|", "||");
            builder.Replace("'", "|'");
            builder.Replace("\n", "|n");
            builder.Replace("\r", "|r");
            builder.Replace("\t", "    ");
            builder.Replace("]", "|]");
            return builder.ToString();
        }
    }
}