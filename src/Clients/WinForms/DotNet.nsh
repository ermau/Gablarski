# DotNET version checking macro.
# Written by AnarkiNet(AnarkiNet@gmail.com) originally, modified by eyal0 (for use in http://www.sourceforge.net/projects/itwister)
# Downloads and runs the Microsoft .NET Framework version 2.0 Redistributable and runs it if the user does not have the correct version.
# To use, call the macro with a string:
# !insertmacro CheckDotNET "2"
# !insertmacro CheckDotNET "2.0.9"
# (Version 2.0.9 is less than version 2.0.10.)
# All register variables are saved and restored by CheckDotNet
# No output
  
!macro CheckDotNET DotNetReqVer
  DetailPrint "Checking your .NET Framework version..."
  # callee register save
  Push $0
  Push $1
  Push $2
  Push $3
  Push $4
  Push $5
  Push $6 # backup of intsalled ver
  Push $7 # backup of DoNetReqVer
 
  StrCpy $7 ${DotNetReqVer}
 
  System::Call "mscoree::GetCORVersion(w .r0, i ${NSIS_MAX_STRLEN}, *i r2r2) i .r1 ?u"
  
  ${If} $0 == 0
#  	DetailPrint ".NET Framework not found, download is required for program to run."
    Goto NoDotNET
  ${EndIf}
 
  StrCpy $0 $0 $2 1 ;remove the starting "v", $0 has the installed version num as a string
  StrCpy $6 $0
  StrCpy $1 $7 ;$1 has the requested verison num as a string
 
  ${Do}
    StrCpy $2 "" ;clear out the installed part
    StrCpy $3 "" ;clear out the required part
 
    ${Do}
      ${If} $0 == "" ;if there are no more characters in the version
        StrCpy $4 "." ;fake the end of the version string
      ${Else}
        StrCpy $4 $0 1 0 ;$4 = character from the installed ver
        ${If} $4 != "."
          StrCpy $0 $0 ${NSIS_MAX_STRLEN} 1 ;remove that first character from the remaining
        ${EndIf}
      ${EndIf}
      
      ${If} $1 == ""  ;if there are no more characters in the version
        StrCpy $5 "." ;fake the end of the version string
      ${Else}
        StrCpy $5 $1 1 0 ;$5 = character from the required ver
        ${If} $5 != "."
          StrCpy $1 $1 ${NSIS_MAX_STRLEN} 1 ;remove that first character from the remaining
        ${EndIf}
      ${EndIf}
            
      ${If} $4 == "."
      ${AndIf} $5 == "."
        ${ExitDo} ;we're at the end of the part
      ${EndIf}
 
      ${If} $4 == "." ;if we're at the end of the current installed part
        StrCpy $2 "0$2" ;put a zero on the front
      ${Else} ;we have another character
        StrCpy $2 "$2$4" ;put the next character on the back
      ${EndIf}
      ${If} $5 == "." ;if we're at the end of the current required part
        StrCpy $3 "0$3" ;put a zero on the front
      ${Else} ;we have another character
        StrCpy $3 "$3$5" ;put the next character on the back
      ${EndIf}
    ${Loop}
    
    ${If} $0 != "" ;let's remove the leading period on installed part if it exists
      StrCpy $0 $0 ${NSIS_MAX_STRLEN} 1
    ${EndIf}
    ${If} $1 != "" ;let's remove the leading period on required part if it exists
      StrCpy $1 $1 ${NSIS_MAX_STRLEN} 1
    ${EndIf}
    
    ${If} $2 S< $3
      IntOp $0 0 - 1 ;$0 = -1, installed less than required
      ${ExitDo}
    ${ElseIf} $2 S> $3
      IntOp $0 0 + 1 ;$0 = 1, installed greater than required
      ${ExitDo}
    ${ElseIf} $2 == ""
    ${AndIf} $3 == ""
      IntOp $0 0 + 0 ;$0 = 0, the versions are identical
      ${ExitDo}
    ${EndIf} ;otherwise we just keep looping through the parts
  ${Loop}
 
  ${If} $0 < 0
    DetailPrint ".NET Framework Version found: $6, but is older than the required version: $7"
    Goto NoDotNet
  ${Else}
    DetailPrint ".NET Framework Version found: $6, equal or newer to required version: $7."
    Goto DoneInstallingDotNet
  ${EndIf}
 
NoDotNet:

  DetailPrint ".NET Framework Version found: $2, required version: 2.0"

    MessageBox MB_YESNO|MB_ICONEXCLAMATION \
    ".NET Framework not installed.$\nRequired Version: $7 or greater.$\nInstall .NET Framework 3.5?" \
    /SD IDYES IDYES InstallDotNET IDNO GiveUpDotNET
  
InstallDotNET:
 
  DetailPrint "Pausing installation while .NET Framework installer runs."

  #ExecWait '"$LOCALAPPDATA\Temp\WindowsInstaller3.1.exe" /passive /norestart'
  #Delete "$LOCALAPPDATA\Temp\WindowsInstaller3.1.exe"

  ExecWait '"$TEMP\dotNetFx35setup.exe" /q /c:"Install "' $0
  ${If} $0 == 0 
    Goto DotNetInstallSuccess
  ${Else}
    ${If} $0 == 4097
        MessageBox MB_OK "Installation cannot continue. Please reinstall$\nusing an account with Administrative rights."
        DetailPrint "Installation aborted due to user rights."
        Goto GiveUpDotNET
    ${ElseIf} $0 == -2147023829
        DetailPrint "Error copying files for .NET 3.5 installation."
    ${ElseIf} $0 == 1602
        DetailPrint "User aborted .NET 3.5 installation." 
    ${EndIf}
    MessageBox MB_OK ".NET 3.5 failed to install.$\nInstallation aborted."
    Goto GiveUpDotNET 
  ${EndIf}
 
DotNetInstallSuccess:
  DetailPrint "Completed .NET Framework install/update. Removing .NET Framework installer."
  Delete "$TEMP\dotNetFx35setup.exe"    
  Goto DoneInstallingDotNet
 
GiveUpDotNET:
  Abort "Installation cancelled by user."
 
DoneInstallingDotNet:
  DetailPrint "Proceeding with remainder of installation."
  Pop $0
  Pop $1
  Pop $2
  Pop $3
  Pop $4
  Pop $5
  Pop $6 # backup of intsalled ver
  Pop $7 # backup of DoNetReqVer
!macroend