Name Gablarski

RequestExecutionLevel admin

# General Symbol Definitions
!define REGKEY "SOFTWARE\$(^Name)"
!define VERSION 0.0.0.0
!define COMPANY "Eric Maupin"
!define URL http://gablarski.org

# MUI Symbol Definitions
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\orange-install.ico"
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_STARTMENUPAGE_REGISTRY_ROOT HKLM
!define MUI_STARTMENUPAGE_REGISTRY_KEY ${REGKEY}
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME StartMenuGroup
!define MUI_STARTMENUPAGE_DEFAULTFOLDER Gablarski
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\orange-uninstall.ico"
!define MUI_UNFINISHPAGE_NOAUTOCLOSE

# Included files
!include Sections.nsh
!include MUI2.nsh
!include DotNet.nsh

# Variables
Var StartMenuGroup

# Installer pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE ..\..\..\Gablarski.License.txt
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

# Installer languages
!insertmacro MUI_LANGUAGE English

# Installer attributes
OutFile GablarskiSetup.exe
InstallDir $PROGRAMFILES\Gablarski
CRCCheck on
XPStyle on
ShowInstDetails show
VIProductVersion 0.0.0.0
VIAddVersionKey ProductName Gablarski
VIAddVersionKey ProductVersion "${VERSION}"
VIAddVersionKey CompanyName "${COMPANY}"
VIAddVersionKey CompanyWebsite "${URL}"
VIAddVersionKey FileVersion "${VERSION}"
VIAddVersionKey FileDescription ""
VIAddVersionKey LegalCopyright ""
InstallDirRegKey HKLM "${REGKEY}" Path
ShowUninstDetails show

# Installer sections
Section -Main SEC0000
    SetOutPath $INSTDIR
    SetOverwrite on
    File ..\..\..\lib\Cadenza.dll
    File ..\..\..\lib\Cadenza.pdb
    File ..\..\..\lib\Growl.Connector.dll
    File ..\..\..\lib\Growl.CoreLibrary.dll
    File ..\..\..\lib\Growl.license.txt
    File ..\..\..\lib\libcelt.dll
    File ..\..\..\lib\libcelt.license.txt
    File ..\..\..\lib\log4net.dll
    File ..\..\..\lib\log4net.License.txt
    File ..\..\..\lib\OpenAL32.dll
    File ..\..\..\lib\OpenALSoft.License.txt
    File ..\..\..\lib\System.Data.SQLite.DLL
    File ..\..\Gablarski\bin\{config}\Gablarski.dll
    File ..\..\..\Gablarski.License.txt
    File ..\..\Gablarski\bin\{config}\Gablarski.pdb
    File ..\..\Gablarski\bin\{config}\Gablarski.XML
    File ..\..\Gablarski.Clients\bin\{config}\Gablarski.Clients.dll
    File ..\..\Gablarski.Clients\bin\{config}\Gablarski.Clients.pdb
    File ..\..\Gablarski.Clients\bin\{config}\Gablarski.Clients.XML
    File ..\..\Gablarski.Growl\bin\{config}\Gablarski.Growl.dll
    File ..\..\Gablarski.Growl\bin\{config}\Gablarski.Growl.pdb
    File ..\..\Gablarski.Input.DirectInput\bin\{config}\Gablarski.Input.DirectInput.dll
    File ..\..\Gablarski.Input.DirectInput\bin\{config}\Gablarski.Input.DirectInput.pdb
    File ..\..\Gablarski.iTunes\bin\{config}\Gablarski.iTunes.dll
    File ..\..\Gablarski.iTunes\bin\{config}\Gablarski.iTunes.pdb
    File ..\..\..\lib\Interop.iTunesLib.dll
    File ..\..\Gablarski.OpenAL\bin\{config}\Gablarski.OpenAL.dll
    File ..\..\Gablarski.OpenAL\bin\{config}\Gablarski.OpenAL.pdb
    File ..\..\Gablarski.SpeechNotifier\bin\{config}\Gablarski.SpeechNotifier.dll
    File ..\..\Gablarski.SpeechNotifier\bin\{config}\Gablarski.SpeechNotifier.pdb
    File ..\..\Gablarski.Winamp\bin\{config}\Gablarski.Winamp.dll
    File ..\..\Gablarski.Winamp\bin\{config}\Gablarski.Winamp.pdb
    File bin\x86\{config}\GablarskiClient.exe
    File bin\x86\{config}\GablarskiClient.exe.config
    File bin\x86\{config}\GablarskiClient.pdb
    File bin\x86\{config}\Headphones.ico
    File bin\x86\{config}\Microsoft.WindowsAPICodePack.dll
    File bin\x86\{config}\Microsoft.WindowsAPICodePack.pdb
    File bin\x86\{config}\Microsoft.WindowsAPICodePack.Shell.dll
    File bin\x86\{config}\Microsoft.WindowsAPICodePack.Shell.pdb
    WriteRegStr HKLM "${REGKEY}\Components" Main 1
    
    SetOutPath $TEMP
    SetOverwrite on
    File ..\..\..\tools\dxwebsetup.exe
    ExecWait "dxwebsetup.exe /Q"
    File ..\..\..\tools\dotNetFx35setup.exe
    !insertmacro CheckDotNET "3.5.30729.4926"
SectionEnd

Section -post SEC0001
    WriteRegStr HKLM "${REGKEY}" Path $INSTDIR
    SetOutPath $INSTDIR
    WriteUninstaller $INSTDIR\uninstall.exe
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    SetOutPath $SMPROGRAMS\$StartMenuGroup
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\Uninstall $(^Name).lnk" $INSTDIR\uninstall.exe
    !insertmacro MUI_STARTMENU_WRITE_END
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayName "$(^Name)"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayVersion "${VERSION}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" Publisher "${COMPANY}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" URLInfoAbout "${URL}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayIcon $INSTDIR\uninstall.exe
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" UninstallString $INSTDIR\uninstall.exe
    WriteRegDWORD HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" NoModify 1
    WriteRegDWORD HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" NoRepair 1
SectionEnd

# Macro for selecting uninstaller sections
!macro SELECT_UNSECTION SECTION_NAME UNSECTION_ID
    Push $R0
    ReadRegStr $R0 HKLM "${REGKEY}\Components" "${SECTION_NAME}"
    StrCmp $R0 1 0 next${UNSECTION_ID}
    !insertmacro SelectSection "${UNSECTION_ID}"
    GoTo done${UNSECTION_ID}
next${UNSECTION_ID}:
    !insertmacro UnselectSection "${UNSECTION_ID}"
done${UNSECTION_ID}:
    Pop $R0
!macroend

# Uninstaller sections
Section /o -un.Main UNSEC0000
    Delete /REBOOTOK $INSTDIR\Microsoft.WindowsAPICodePack.Shell.pdb
    Delete /REBOOTOK $INSTDIR\Microsoft.WindowsAPICodePack.Shell.dll
    Delete /REBOOTOK $INSTDIR\Microsoft.WindowsAPICodePack.pdb
    Delete /REBOOTOK $INSTDIR\Microsoft.WindowsAPICodePack.dll
    Delete /REBOOTOK $INSTDIR\Headphones.ico
    Delete /REBOOTOK $INSTDIR\GablarskiClient.pdb
    Delete /REBOOTOK $INSTDIR\GablarskiClient.exe.config
    Delete /REBOOTOK $INSTDIR\GablarskiClient.exe
    Delete /REBOOTOK $INSTDIR\Gablarski.Winamp.pdb
    Delete /REBOOTOK $INSTDIR\Gablarski.Winamp.dll
    Delete /REBOOTOK $INSTDIR\Gablarski.SpeechNotifier.pdb
    Delete /REBOOTOK $INSTDIR\Gablarski.SpeechNotifier.dll
    Delete /REBOOTOK $INSTDIR\Gablarski.OpenAL.pdb
    Delete /REBOOTOK $INSTDIR\Gablarski.OpenAL.dll
    Delete /REBOOTOK $INSTDIR\Interop.iTunesLib.dll
    Delete /REBOOTOK $INSTDIR\Gablarski.iTunes.pdb
    Delete /REBOOTOK $INSTDIR\Gablarski.iTunes.dll
    Delete /REBOOTOK $INSTDIR\Gablarski.Input.DirectInput.pdb
    Delete /REBOOTOK $INSTDIR\Gablarski.Input.DirectInput.dll
    Delete /REBOOTOK $INSTDIR\Gablarski.Growl.pdb
    Delete /REBOOTOK $INSTDIR\Gablarski.Growl.dll
    Delete /REBOOTOK $INSTDIR\Gablarski.Clients.XML
    Delete /REBOOTOK $INSTDIR\Gablarski.Clients.pdb
    Delete /REBOOTOK $INSTDIR\Gablarski.Clients.dll
    Delete /REBOOTOK $INSTDIR\Gablarski.XML
    Delete /REBOOTOK $INSTDIR\Gablarski.pdb
    Delete /REBOOTOK $INSTDIR\Gablarski.License.txt
    Delete /REBOOTOK $INSTDIR\Gablarski.dll
    Delete /REBOOTOK $INSTDIR\System.Data.SQLite.DLL
    Delete /REBOOTOK $INSTDIR\OpenALSoft.License.txt
    Delete /REBOOTOK $INSTDIR\OpenAL32.dll
    Delete /REBOOTOK $INSTDIR\log4net.License.txt
    Delete /REBOOTOK $INSTDIR\log4net.dll
    Delete /REBOOTOK $INSTDIR\libcelt.license.txt
    Delete /REBOOTOK $INSTDIR\libcelt.dll
    Delete /REBOOTOK $INSTDIR\Growl.license.txt
    Delete /REBOOTOK $INSTDIR\Growl.CoreLibrary.dll
    Delete /REBOOTOK $INSTDIR\Growl.Connector.dll
    Delete /REBOOTOK $INSTDIR\Cadenza.pdb
    Delete /REBOOTOK $INSTDIR\Cadenza.dll
    DeleteRegValue HKLM "${REGKEY}\Components" Main
SectionEnd

Section -un.post UNSEC0001
    DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)"
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\Uninstall $(^Name).lnk"
    Delete /REBOOTOK $INSTDIR\uninstall.exe
    DeleteRegValue HKLM "${REGKEY}" StartMenuGroup
    DeleteRegValue HKLM "${REGKEY}" Path
    DeleteRegKey /IfEmpty HKLM "${REGKEY}\Components"
    DeleteRegKey /IfEmpty HKLM "${REGKEY}"
    RmDir /REBOOTOK $SMPROGRAMS\$StartMenuGroup
    RmDir /REBOOTOK $INSTDIR
    Push $R0
    StrCpy $R0 $StartMenuGroup 1
    StrCmp $R0 ">" no_smgroup
no_smgroup:
    Pop $R0
SectionEnd

# Installer functions
Function .onInit
    InitPluginsDir
FunctionEnd

# Uninstaller functions
Function un.onInit
    ReadRegStr $INSTDIR HKLM "${REGKEY}" Path
    !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup
    !insertmacro SELECT_UNSECTION Main ${UNSEC0000}
FunctionEnd

