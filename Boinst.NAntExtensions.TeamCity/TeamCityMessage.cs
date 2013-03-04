namespace Boinst.NAntExtensions.TeamCity
{
    using NAnt.Core;

    public struct TeamCityMessage
    {
        public string Text { get; set; }
        public string ErrorDetails { get; set; }
        public Level Level { get; set; }
    }
}