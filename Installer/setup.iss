; Inno Setup 6/7 betiği (opsiyonel). ISCC.exe ile derleyin.
; Ornek: "C:\Program Files\Inno Setup 7\ISCC.exe" setup.iss

#define MyAppName "A4Tech Control"
#define MyAppVersion "1.0.1"
#define MyAppPublisher "A4Tech Control"
#define MyAppExeName "A4TechControl.exe"
#define PublishDir "..\bin\Release\net8.0-windows\win-x64\publish"

[Setup]
AppId={{A4C0FFEE-1234-4B2D-9A11-5E6F7A8B9C0D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\SetupOutput
OutputBaseFilename=A4TechControl-Setup
SetupIconFile=..\Resources\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
CloseApplications=yes

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Masaustune kisayol ekle"; GroupDescription: "Ek kisayollar:"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Uygulamayi simdi calistir"; Flags: nowait postinstall skipifsilent
