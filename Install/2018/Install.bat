@echo off
set RevitVersion=2018
echo f | xcopy "%cd%\EiBreRebarUtils.dll" "%appdata%\Autodesk\Revit\Addins\%RevitVersion%\EiBreRebarUtils.dll" /y
echo f | xcopy "%cd%\EiBreRebarUtils.addin" "%appdata%\Autodesk\Revit\Addins\%RevitVersion%\EiBreRebarUtils.addin" /y
timeout 15
exit