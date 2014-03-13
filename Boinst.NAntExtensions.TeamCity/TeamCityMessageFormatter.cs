namespace Boinst.NAntExtensions.TeamCity
{
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
    }
}