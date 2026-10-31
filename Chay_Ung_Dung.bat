@echo off
title He Thong Quan Ly Ky Tuc Xa Thong Minh
cd /d "%~dp0"
echo ========================================================
echo   HE THONG QUAN LY KY TUC XA THONG MINH (KTX SMART)
echo   Dang khoi dong ung dung WinForms .NET 10...
echo ========================================================

:: Giai phong file DLL neu co tien trinh KTX.WinForms dang chay ngam
taskkill /F /IM KTX.WinForms.exe >nul 2>&1

:: Khoi chay ung dung WinForms
dotnet run --project KTX.WinForms\KTX.WinForms.csproj
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ========================================================
    echo   UNG DUNG DANG DUNG HOAC CO SU CO.
    echo ========================================================
    pause
)
