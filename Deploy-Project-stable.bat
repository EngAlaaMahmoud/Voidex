@echo off
title Smart Inventory Deployment
echo ==========================================
echo    SMART INVENTORY DEPLOYMENT
echo ==========================================

:: Check admin rights
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Run as Administrator!
    pause
    exit /b 1
)

set "DEPLOY_PATH=C:\InventoryApp"
set "PROJECT_ROOT=%~dp0"
set "HAS_DOTNET=0"
set "HAS_SQL=0"
set "HAS_EF_TOOLS=0"

echo ==========================================
echo    DOWNLOAD LINKS (If Needed)
echo ==========================================
echo.
echo .NET 9.0 SDK:
echo https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.301-windows-x64-installer
echo.
echo SQL Server Express:
echo https://go.microsoft.com/fwlink/p/?linkid=2216019^&culture=en-us
echo.
echo ==========================================

echo Checking existing installations...

:: Check .NET
dotnet --version 2>nul | findstr "9.0" >nul
if %errorlevel% equ 0 (
    echo ✓ .NET 9.0 already installed
    set "HAS_DOTNET=1"
) else (
    echo ❌ .NET 9.0 not found
    echo    Please download and install from the link above
    set "HAS_DOTNET=0"
)

:: Check EF Tools
if %HAS_DOTNET% equ 1 (
    echo Checking EF Core Tools...
    dotnet ef --version >nul 2>&1
    if %errorlevel% equ 0 (
        echo ✓ EF Core Tools already installed
        set "HAS_EF_TOOLS=1"
    ) else (
        echo → EF Core Tools not found - will install
        set "HAS_EF_TOOLS=0"
    )
)

:: Check SQL Server Express
sc query "MSSQL$SQLEXPRESS" >nul 2>&1
if %errorlevel% equ 0 (
    echo ✓ SQL Server Express already installed
    set "HAS_SQL=1"
    net start "MSSQL$SQLEXPRESS" >nul 2>&1
) else (
    echo ❌ SQL Server Express not found
    echo    Please download and install from the link above
    set "HAS_SQL=0"
)

echo.

:: Check if we can proceed
if %HAS_DOTNET% equ 0 (
    echo ⚠️ WARNING: .NET 9.0 is required for deployment
    echo    The build process may fail without it
    echo.
    set /p "continue_dotnet=Continue without .NET 9.0? (Y/N): "
    if /i "!continue_dotnet!" neq "Y" (
        echo Please install .NET 9.0 first, then run this script again.
        pause
        exit /b 1
    )
)

if %HAS_SQL% equ 0 (
    echo ⚠️ WARNING: SQL Server Express is required for the application
    echo    The application will not work without it
    echo.
    set /p "continue_sql=Continue without SQL Server Express? (Y/N): "
    if /i "!continue_sql!" neq "Y" (
        echo Please install SQL Server Express first, then run this script again.
        pause
        exit /b 1
    )
)

echo ==========================================
echo    STARTING DEPLOYMENT
echo ==========================================

:: Install EF Tools if needed
if %HAS_DOTNET% equ 1 if %HAS_EF_TOOLS% equ 0 (
    echo [1/7] Installing EF Core Tools...
    call :install_ef_tools
    if %errorlevel% neq 0 (
        echo ⚠️ EF Tools installation failed - continuing anyway
    ) else (
        echo ✓ EF Core Tools installed successfully
    )
) else (
    echo [SKIP] EF Core Tools already available
)

echo [2/7] Publishing application...
dotnet publish "%PROJECT_ROOT%Presentation\ASPNET\ASPNET.csproj" -c Release -o "%DEPLOY_PATH%" --self-contained false
if %errorlevel% neq 0 (
    echo ❌ ERROR: Application publishing failed!
    echo Make sure .NET 9.0 SDK is installed and try again.
    pause
    exit /b 1
)
echo ✓ Application published successfully

echo [3/7] Setting up directories and permissions...
mkdir "%DEPLOY_PATH%\wwwroot\app_data\images" 2>nul
mkdir "%DEPLOY_PATH%\wwwroot\app_data\docs" 2>nul
mkdir "%DEPLOY_PATH%\logs" 2>nul
mkdir "%DEPLOY_PATH%\Backups" 2>nul
icacls "%DEPLOY_PATH%" /grant Everyone:(OI)(CI)F /T /Q >nul
echo ✓ Directory structure created

echo [4/7] Creating configuration file...
call :create_config
echo ✓ Configuration file created

echo [5/7] Setting up firewall...
netsh advfirewall firewall delete rule name="Inventory App" >nul 2>&1
netsh advfirewall firewall add rule name="Inventory App" dir=in action=allow protocol=TCP localport=5000 >nul 2>&1
echo ✓ Firewall configured for port 5000

echo [6/7] Creating management scripts...
call :create_start_script
call :create_management_scripts
call :create_database_scripts
call :create_desktop_shortcut
echo ✓ Management scripts created

echo [7/7] Final setup...
call :create_migration_script
echo ✓ Migration tools configured

echo.
echo ==========================================
echo    DEPLOYMENT COMPLETED!
echo ==========================================
echo.

if %HAS_DOTNET% equ 1 if %HAS_SQL% equ 1 (
    echo 🚀 Perfect! All prerequisites installed - ready to run!
    echo 💾 Database: Will be created/updated automatically via migrations
    echo 🛠️ EF Tools: Available for development
) else (
    echo ⚠️ IMPORTANT REMINDERS:
    if %HAS_DOTNET% equ 0 (
        echo    • Install .NET 9.0 SDK before running
        echo      https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.301-windows-x64-installer
    )
    if %HAS_SQL% equ 0 (
        echo    • Install SQL Server Express before running  
        echo      https://go.microsoft.com/fwlink/p/?linkid=2216019^&culture=en-us
    )
)

echo.
echo 🚀 TO START APPLICATION:
echo    • Run: %DEPLOY_PATH%\start-inventory.bat
echo    • Access: http://localhost:5000
echo    • Login: admin@yourcompany.com / AdminPass123!
echo    • Desktop: Smart Inventory shortcut created on desktop
echo.
echo 📋 FIRST RUN NOTES:
echo    • Application will run migrations automatically
echo    • Database tables will be created/updated
echo    • Default admin user will be created
echo    • This may take 30-60 seconds on first startup
echo.
echo 🛠️ MANAGEMENT COMMANDS:
echo    • Start:       %DEPLOY_PATH%\start-inventory.bat
echo    • Stop:        %DEPLOY_PATH%\stop-inventory.bat
echo    • Status:      %DEPLOY_PATH%\check-status.bat
echo    • DB Reset:    %DEPLOY_PATH%\reset-database.bat
echo    • DB Backup:   %DEPLOY_PATH%\backup-database.bat
echo    • Migrations:  %DEPLOY_PATH%\manage-migrations.bat
echo.
pause
exit /b 0

:install_ef_tools
echo   - Installing Entity Framework Core Tools...
dotnet tool install --global dotnet-ef >nul 2>&1
if %errorlevel% neq 0 (
    echo   - EF Tools may already be installed or installation failed
    echo   - Checking if tools are working...
    dotnet ef --version >nul 2>&1
    if %errorlevel% equ 0 (
        echo   - EF Tools are working correctly
        exit /b 0
    ) else (
        echo   - EF Tools installation failed
        exit /b 1
    )
)
echo   - EF Tools installed successfully
exit /b 0

:create_database
if %HAS_SQL% equ 0 (
    echo   - SQL Server not available, migrations will run when app starts
    exit /b 0
)

echo   - Database will be created/updated automatically by migrations
echo   - No manual database creation needed
exit /b 0

:create_config
(
echo {
echo   "ConnectionStrings": {
echo     "DefaultConnection": "Server=.\\SQLEXPRESS;Database=StockManagement;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
echo   },
echo   "DatabaseProvider": "SqlServer",
echo   "FileImageManager": {
echo     "PathFolder": "wwwroot/app_data/images",
echo     "MaxFileSizeInMB": 5
echo   },
echo   "FileDocumentManager": {
echo     "PathFolder": "wwwroot/app_data/docs",
echo     "MaxFileSizeInMB": 25
echo   },
echo   "AspNetIdentity": {
echo     "Password": {
echo       "RequireDigit": true,
echo       "RequireLowercase": true,
echo       "RequireUppercase": true,
echo       "RequireNonAlphanumeric": true,
echo       "RequiredLength": 8
echo     },
echo     "Lockout": {
echo       "DefaultLockoutTimeSpanInMinutes": 30,
echo       "MaxFailedAccessAttempts": 5,
echo       "AllowedForNewUsers": true
echo     },
echo     "User": {
echo       "RequireUniqueEmail": true
echo     },
echo     "SignIn": {
echo       "RequireConfirmedEmail": false
echo     },
echo     "DefaultAdmin": {
echo       "Email": "admin@yourcompany.com",
echo       "Password": "AdminPass123!"
echo     }
echo   },
echo   "Jwt": {
echo     "Key": "YourVeryStrongAndSecureSecretKeyForProduction32Chars!",
echo     "Issuer": "InventoryManagement",
echo     "Audience": "InventoryUsers",
echo     "ExpireInMinute": 30
echo   },
echo   "IsDemoVersion": false,
echo   "Kestrel": {
echo     "Endpoints": {
echo       "Http": {
echo         "Url": "http://0.0.0.0:5000"
echo       }
echo     }
echo   },
echo   "AllowedHosts": "*",
echo   "Logging": {
echo     "LogLevel": {
echo       "Default": "Information",
echo       "Microsoft.AspNetCore": "Warning"
echo     }
echo   }
echo }
) > "%DEPLOY_PATH%\appsettings.json"
exit /b 0

:create_start_script
(
echo @echo off
echo title Inventory Management System
echo color 0A
echo cd /d "%DEPLOY_PATH%"
echo echo ==========================================
echo echo    INVENTORY MANAGEMENT SYSTEM
echo echo ==========================================
echo echo.
echo echo Starting application...
echo echo.
echo echo 🌐 Access: http://localhost:5000
echo echo 🔐 Login: admin@yourcompany.com / AdminPass123!
echo echo 💾 Database: StockManagement on .\SQLEXPRESS
echo echo.
echo echo ⏳ First run may take 30-60 seconds for migrations
echo echo ==========================================
echo echo.
echo :: Start application in background
echo start /b dotnet ASPNET.dll
echo :: Wait and open browser
echo echo Waiting for application to start...
echo timeout /t 10 /nobreak ^>nul
echo echo Opening browser...
echo start http://localhost:5000
echo echo.
echo echo ✓ Application started in background
echo echo ✓ Browser opened automatically
echo echo.
echo echo Press any key to stop the application...
echo pause ^>nul
echo :: Stop application
echo echo.
echo echo Stopping application...
echo taskkill /f /im dotnet.exe ^>nul 2^>^&1
echo echo ✓ Application stopped.
echo timeout /t 2 ^>nul
) > "%DEPLOY_PATH%\start-inventory.bat"
exit /b 0

:create_management_scripts
:: Create stop script
(
echo @echo off
echo echo Stopping Inventory Management System...
echo taskkill /f /im dotnet.exe 2^>nul
echo echo ✓ Application stopped.
echo timeout /t 2
) > "%DEPLOY_PATH%\stop-inventory.bat"

:: Create status check script
(
echo @echo off
echo title System Status
echo echo ==========================================
echo echo    SYSTEM STATUS CHECK
echo echo ==========================================
echo echo.
echo echo [1/4] Application Status...
echo tasklist /fi "imagename eq dotnet.exe" ^| find /i "dotnet.exe" ^> nul
echo if %%errorlevel%% == 0 ^(echo ✓ RUNNING^) else ^(echo ✗ STOPPED^)
echo echo.
echo echo [2/4] SQL Server Status...
echo sc query "MSSQL$SQLEXPRESS" ^| find "RUNNING" ^> nul
echo if %%errorlevel%% == 0 ^(echo ✓ RUNNING^) else ^(echo ✗ STOPPED^)
echo echo.
echo echo [3/4] Database Connection...
echo sqlcmd -S .\SQLEXPRESS -E -Q "SELECT 1" ^> nul 2^>^&1
echo if %%errorlevel%% == 0 ^(echo ✓ CONNECTED^) else ^(echo ✗ FAILED^)
echo echo.
echo echo [4/4] Web Service Status...
echo powershell -Command "try { Invoke-WebRequest -Uri 'http://localhost:5000' -TimeoutSec 5 ^| Out-Null; Write-Host '✓ RESPONDING' } catch { Write-Host '✗ NOT RESPONDING' }"
echo echo.
echo pause
) > "%DEPLOY_PATH%\check-status.bat"
exit /b 0

:create_database_scripts
:: Create database backup script
(
echo @echo off
echo title Database Backup
echo echo Creating database backup...
echo set backup_date=%%date:~-4,4%%%%date:~-10,2%%%%date:~-7,2%%
echo set backup_time=%%time:~0,2%%%%time:~3,2%%%%time:~6,2%%
echo set backup_time=%%backup_time: =0%%
echo set backup_name=StockManagement_%%backup_date%%_%%backup_time%%.bak
echo echo.
echo echo Backing up database to: %%backup_name%%
echo sqlcmd -S .\SQLEXPRESS -E -Q "BACKUP DATABASE [StockManagement] TO DISK = '%DEPLOY_PATH%\Backups\%%backup_name%%'"
echo if %%errorlevel%% == 0 ^(
echo     echo ✓ Backup completed successfully
echo     echo Location: %DEPLOY_PATH%\Backups\%%backup_name%%
echo ^) else ^(
echo     echo ❌ Backup failed
echo ^)
echo echo.
echo pause
) > "%DEPLOY_PATH%\backup-database.bat"

:: Create database reset script
(
echo @echo off
echo title Database Reset
echo echo ==========================================
echo echo    DATABASE RESET WARNING
echo echo ==========================================
echo echo.
echo echo ⚠️ This will DELETE all data in the database!
echo echo.
echo echo Current database: StockManagement
echo echo Server: .\SQLEXPRESS
echo echo.
echo set /p "confirm=Are you sure you want to reset the database? (type YES): "
echo if /i "%%confirm%%" neq "YES" ^(
echo     echo Reset cancelled.
echo     pause
echo     exit /b 0
echo ^)
echo echo.
echo echo Creating backup before reset...
echo call "%DEPLOY_PATH%\backup-database.bat"
echo echo.
echo echo Dropping and recreating database...
echo sqlcmd -S .\SQLEXPRESS -E -Q "DROP DATABASE IF EXISTS [StockManagement]"
echo sqlcmd -S .\SQLEXPRESS -E -Q "CREATE DATABASE [StockManagement]"
echo echo.
echo echo ✓ Database reset completed
echo echo.
echo echo Next steps:
echo echo 1. Start the application: start-inventory.bat
echo echo 2. The application will recreate all tables automatically
echo echo 3. Default admin user will be created
echo echo.
echo pause
) > "%DEPLOY_PATH%\reset-database.bat"
exit /b 0

:create_migration_script
:: Create migration management script
(
echo @echo off
echo title Migration Management
echo :start
echo echo ==========================================
echo echo    MIGRATION MANAGEMENT TOOLS
echo echo ==========================================
echo echo.
echo echo Available commands:
echo echo.
echo echo [1] List all migrations
echo echo [2] Create new migration
echo echo [3] Update database manually
echo echo [4] Generate SQL script
echo echo [5] Exit
echo echo.
echo set /p "choice=Choose option (1-5): "
echo.
echo if "%%choice%%"=="1" goto :list_migrations
echo if "%%choice%%"=="2" goto :create_migration
echo if "%%choice%%"=="3" goto :update_database
echo if "%%choice%%"=="4" goto :generate_script
echo if "%%choice%%"=="5" exit /b 0
echo echo Invalid choice
echo pause
echo goto :start
echo.
echo :list_migrations
echo echo Listing all migrations...
echo dotnet ef migrations list --project "%PROJECT_ROOT%Infrastructure\Infrastructure" --startup-project "%PROJECT_ROOT%Presentation\ASPNET"
echo pause
echo goto :start
echo.
echo :create_migration
echo set /p "migration_name=Enter migration name: "
echo echo Creating migration: %%migration_name%%
echo dotnet ef migrations add %%migration_name%% --project "%PROJECT_ROOT%Infrastructure\Infrastructure" --startup-project "%PROJECT_ROOT%Presentation\ASPNET"
echo pause
echo goto :start
echo.
echo :update_database
echo echo Updating database manually...
echo dotnet ef database update --project "%PROJECT_ROOT%Infrastructure\Infrastructure" --startup-project "%PROJECT_ROOT%Presentation\ASPNET"
echo pause
echo goto :start
echo.
echo :generate_script
echo set /p "script_name=Enter script filename (without .sql): "
echo echo Generating SQL script: %%script_name%%.sql
echo dotnet ef migrations script --project "%PROJECT_ROOT%Infrastructure\Infrastructure" --startup-project "%PROJECT_ROOT%Presentation\ASPNET" --output "%DEPLOY_PATH%\Backups\%%script_name%%.sql"
echo echo Script saved to: %DEPLOY_PATH%\Backups\%%script_name%%.sql
echo pause
echo goto :start
) > "%DEPLOY_PATH%\manage-migrations.bat"
exit /b 0

:create_desktop_shortcut
echo   - Creating desktop shortcut...

:: Create a temporary VBScript to create the shortcut
(
echo Set oWS = WScript.CreateObject("WScript.Shell"^)
echo sLinkFile = oWS.SpecialFolders("Desktop"^) ^& "\Smart Inventory.lnk"
echo Set oLink = oWS.CreateShortcut(sLinkFile^)
echo oLink.TargetPath = "%DEPLOY_PATH%\start-inventory.bat"
echo oLink.WorkingDirectory = "%DEPLOY_PATH%"
echo oLink.Description = "Smart Inventory Management System"
echo oLink.IconLocation = "%SystemRoot%\System32\imageres.dll,137"
echo oLink.WindowStyle = 1
echo oLink.Save
) > "%TEMP%\create_shortcut.vbs"

:: Execute the VBScript
cscript "%TEMP%\create_shortcut.vbs" >nul 2>&1

:: Clean up temporary file
del "%TEMP%\create_shortcut.vbs" >nul 2>&1

if exist "%USERPROFILE%\Desktop\Smart Inventory.lnk" (
    echo   ✓ Desktop shortcut created successfully
) else (
    echo   ⚠️ Desktop shortcut creation failed
)
exit /b 0