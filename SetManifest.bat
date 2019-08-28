@echo off
pushd %~dp0

Manifest\bin\Debug\Manifest.exe "PSFile\bin\Debug\PSFile.dll" "PSFile\Cmdlet" "PSFile\bin\Debug\PSFile.psd1" "PSFile\bin\Debug\PSFile.psm1"
Manifest\bin\Release\Manifest.exe "PSFile\bin\Release\PSFile.dll" "PSFile\Cmdlet" "PSFile\bin\Release\PSFile.psd1" "PSFile\bin\Release\PSFile.psm1"

pause
