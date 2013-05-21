namespace Boinst.NUnitAddins.TeamCity
{
    using System.Text;

    public static class TeamCityMessageFormatter
    {
        public static string FormatTestFailedMessage(string testname, string message, string detail)
        {
            testname = EscapeInvalidCharacters(testname);
            message = EscapeInvalidCharacters(message);
            detail = EscapeInvalidCharacters(detail);
            return string.Format("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", testname, message, detail);
        }

        public static string FormatTestFinishedMessage(string testname)
        {
            testname = EscapeInvalidCharacters(testname);
            return string.Format("##teamcity[testFinished name='{0}']", testname);
        }

        public static string FormatTestErrorMessage(string testname, string text)
        {
            testname = EscapeInvalidCharacters(testname);
            text = EscapeInvalidCharacters(text);
            return string.Format("##teamcity[testStdErr name='{0}' out='{1}']", testname, text);
        }

        public static string EscapeInvalidCharacters(string text)
        {
            var builder = new StringBuilder(text);
            builder.Replace("|", "||");
            builder.Replace("'", "|'");
            builder.Replace("\n", "|n");
            builder.Replace("\r", "|r");
            builder.Replace("\t", "    ");
            builder.Replace("]", "|]");
            return builder.ToString();
        }

        public static string FormatTestOutputMessage(string testname, string text)
        {
            testname = EscapeInvalidCharacters(testname);
            text = EscapeInvalidCharacters(text);
            return string.Format("##teamcity[testStdOut name='{0}' out='{1}']", testname, text);
        }
    }
}