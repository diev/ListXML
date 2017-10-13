@echo off
rem Directory with the project to build
set proj=ListXML

rem Arguments to build
set args=/t:Rebuild /p:Configuration=Release

rem If you have Microsoft Visual Studio Community 2017 installed:
call :build Community

echo BuildTools
rem If you have no full Visual Studio installed, install just Build Tools 2017 only:
rem open https://www.visualstudio.com/ru/downloads/
rem expand "Other Tools and Frameworks"
rem seek "Build Tools for Visual Studio 2017"
rem download https://www.visualstudio.com/ru/thank-you-downloading-visual-studio/?sku=BuildTools&rel=15
rem run "vs_buildtools.exe --add Microsoft.VisualStudio.Workload.MSBuildTools --quiet"
call :build BuildTools
exit 1

:build
set exe="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\%1\MSBuild\15.0\Bin\MSBuild.exe"
if not exist %exe% goto :eof
%exe% %proj% %args%
exit 0
