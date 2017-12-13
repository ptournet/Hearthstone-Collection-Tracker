@echo off
REM Remove the directories if they exist
if exist HearthMirror del HearthMirror
if exist HearthDb del HearthDb
git clone https://github.com/HearthSim/HearthMirror
git clone https://github.com/HearthSim/HearthDb

nuget help | head -1  >nul 2>&1 && (
    nuget help | head -1
    nuget restore
) || (
    ECHO nuget is not installed. When loading project, go to VIEW -> other windows -> package manager console and restore nuget packages
    pause
)
