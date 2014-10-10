namespace Boinst.NAntExtensions.TeamCity
{
    using System;
    using System.Text;

    using NAnt.Core;

    public static class TeamCityMessageFormatter
    {
        public static string FormatMessage(TeamCityMessage message)
        {
            // If the message is already a TeamCity control message,
            // no further formatting is required.
            if (message.Text.Contains("##teamcity")) return message.Text;

            string errorDetails = message.Level != Level.Error
                                      ? string.Empty
                                      : string.Format(" errorDetails='{0}'", TeamCityMessageFormatter.EscapeInvalidCharacters(message.ErrorDetails));

            return string.Format(
                "##teamcity[message text='{0}' {1} status='{2}']",
                TeamCityMessageFormatter.EscapeInvalidCharacters(message.Text),
                errorDetails,
                TeamCityMessageFormatter.FormatLevel(message.Level));
        }

        public static string FormatProgressStartMessage(string blockName)
        {
            blockName = TeamCityMessageFormatter.EscapeInvalidCharacters(blockName);
            return string.Format("##teamcity[progressStart name='{0}']", blockName);
        }
        public static string FormatProgressFinishMessage(string blockName)
        {
            blockName = TeamCityMessageFormatter.EscapeInvalidCharacters(blockName);
            return string.Format("##teamcity[progressFinish name='{0}']", blockName);
        }

        public static string FormatBlockOpenedMessage(string blockName)
        {
            blockName = TeamCityMessageFormatter.EscapeInvalidCharacters(blockName);
            return string.Format("##teamcity[blockOpened name='{0}']", blockName);
        }

        public static string FormatBlockClosedMessage(string blockName)
        {
            blockName = TeamCityMessageFormatter.EscapeInvalidCharacters(blockName);
            return string.Format("##teamcity[blockClosed name='{0}']", blockName);
        }

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
            var builder = new StringBuilder(text);
            builder.Replace("|", "||");
            builder.Replace("'", "|'");
            builder.Replace("\n", "|n");
            builder.Replace("\r", "|r");
            builder.Replace("\t", "    ");
            builder.Replace("]", "|]");
            return builder.ToString();
        }

        public static string FormatLevel(Level level)
        {
            switch (level)
            {
                case Level.Error:
                    return "ERROR";
                case Level.Warning:
                    return "WARNING";
                default:
                    return "NORMAL";
            }
        }

        /// <summary>
        /// We are *in* TeamCity if the environment variable TEAMCITY_VERSION is set
        /// to a non-empty value.
        /// </summary>
        public static bool InTeamcity()
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_VERSION"));
        }
    }
}