Change Log
==========

0.1 - 2013.03.05
        Initial Release
        
0.2 - 2013.03.06
        Do not log task started/task finished messages for some task types.
        At this time, ignored task types are "echo", "property", "include".
        
0.3 - 2013.03.08
        Fixed bugs. Fixed a few potential null reference exceptions.
        
0.4 - 2013.04.22
        Added TeamCity tasks. 
        "teamcity-publishartifacts" for publishing artifacts.
        "teamcity-setbuildnumber" for setting the build number.
        "teamcity-progressmessage" for logging a teamcity progress message.
        
0.5 - 2013.05.01
        Added "mutex" task.