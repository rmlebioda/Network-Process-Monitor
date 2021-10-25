# Readonly disclaimer
This project will not be maintained by me anymore.

# Network Process Monitor
Program tracks all running processes in system and reports their network usage in a table along with execution path.
It also does not delete/forget finished processes and has ability to filter alive/dead processes out from the table.

# Technology
Program is written in C# .NetFramework 4.7.2 using WinForms.

Administrator rights are required to run Microsoft's EventTracking system, more info:

https://www.nuget.org/packages/Microsoft.Diagnostics.Tracing.TraceEvent/

# Options
Program lets set two things in App.config appSettings:
- key "ListRefreshRate" - table refresh rate in miliseconds (default 1s = 1000ms),
- key "ErrorLoggerPath" - path to log all catched exception in program, set to empty string to disable logger

# Bugs
Probably a lot (despite small app), but most of them are fixed and it should be quite stable.

Known bugs:
- Desynchronization in filtering/sorting columns during first few seconds - probably caused by bad-communicating threads, but 
it's hard to find issue and it's not critical bug, so I'm not fixing it for now.
- Windows freezes for a short period of time during refreshing a process list and usually breaks scrolling
