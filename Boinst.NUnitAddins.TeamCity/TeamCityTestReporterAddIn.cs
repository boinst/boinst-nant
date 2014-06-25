namespace Boinst.NUnitAddins.TeamCity
{
    using System;

    using NUnit.Core.Extensibility;

    /// <summary>
    /// Test reporter for TeamCity
    /// </summary>
    [NUnitAddin(Name = "TeamCityTestReporter", Description = "Reports running NUnit tests to TeamCity.")]
    public class TeamCityTestReporterAddIn : IAddin
    {
        /// <summary>
        /// Install the extension.
        /// </summary>
        public bool Install(IExtensionHost host)
        {
            // If we're not running under TeamCity, nothing to do here.
            if (!InTeamcity()) return true;

            var listeners = host.GetExtensionPoint("EventListeners");
            if (listeners == null) return false;
            
            listeners.Install(new TeamCityTestReporterListener());

            return true;
        }

        /// <summary>
        /// Are we currently in TeamCity?
        /// <br/>
        /// We don't log teamcity-specific messages if we're not running in TeamCity.
        /// </summary>
        private static bool InTeamcity()
        {
            return true;
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_VERSION"));
        }
    }
}
