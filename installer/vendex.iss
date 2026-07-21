; Instalador do Vendex PDV — gerado com Inno Setup 6.
;
; Instala por usuário (sem exigir admin/UAC) em {localappdata}\Programs\Vendex PDV,
; porque o app grava o banco SQLite numa pasta "dados" ao lado do .exe
; (ver Vendex.App/AppPaths.cs) — instalar em Program Files exigiria elevar
; toda vez só para o app conseguir escrever seus próprios dados.
;
; Pré-requisito: rodar `dotnet publish` (self-contained, single-file, win-x64)
; com saída em ..\publish\VendexPDV antes de compilar este script.

#define MyAppName "Vendex PDV"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Vendex"
#define MyAppExeName "Vendex.App.exe"
#define PublishDir "..\publish\VendexPDV"

[Setup]
AppId={{8C6F2E1A-9B4D-4E7A-9C3F-2D8A1B5E6F0C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=output
OutputBaseFilename=VendexPDV-Setup-{#MyAppVersion}
SetupIconFile=..\src\Vendex.App\Assets\vendex-icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
