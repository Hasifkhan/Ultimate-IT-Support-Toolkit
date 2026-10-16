@echo off
setlocal
title Building IT Support Toolkit
cd /d "%~dp0"

set "CSC=%windir%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%windir%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo .NET Framework 4 compiler was not found on this PC.
    if not defined CI pause
    exit /b 1
)

echo Building IT-Support-Toolkit.exe ...
"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu ^
  /out:"IT-Support-Toolkit.exe" ^
  /win32icon:"app.ico" /win32manifest:"app.manifest" ^
  /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll ^
  ToolkitApp.cs

if errorlevel 1 (
    echo.
    echo BUILD FAILED - please send the error text above.
    if not defined CI pause
    exit /b 1
)

echo.
echo SUCCESS: IT-Support-Toolkit.exe created in this folder.
if not defined CI pause
