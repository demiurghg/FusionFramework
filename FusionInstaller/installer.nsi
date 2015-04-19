; example2.nsi
;
; This script is based on example1.nsi, but it remember the directory, 
; has uninstall support and (optionally) installs start menu shortcuts.
;
; It will install example2.nsi into a directory that the user selects,
;--------------------------------

!include "MUI.nsh"
  !define MUI_ABORTWARNING


; The name of the installer
Name "Fusion Game Library (v1.0)"
XPStyle on

; The file to write
OutFile "FusionInstaller-1.0.exe"

; The default installation directory
InstallDir $PROGRAMFILES\FusionGameLib

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\FusionGameLib" "Install_Dir"

; Request application privileges for Windows Vista
;RequestExecutionLevel admin

; Splash screen
Function .onInit
	# the plugins dir is automatically deleted when the installer exits
	InitPluginsDir
	File /oname=$PLUGINSDIR\splash.bmp "splash.bmp"
	#optional
	#File /oname=$PLUGINSDIR\splash.wav "C:\myprog\sound.wav"

	splash::show 3000 $PLUGINSDIR\splash

	Pop $0 ; $0 has '1' if the user closed the splash screen early,
			; '0' if everything closed normally, and '-1' if some error occurred.
FunctionEnd


  !define MUI_HEADERIMAGE
  !define MUI_HEADERIMAGE_RIGHT
  !define MUI_HEADERIMAGE_BITMAP "header-r.bmp" ; optional
  !define MUI_LICENSEPAGE_CHECKBOX
  !define MUI_COMPONENTSPAGE_NODESC
  
  !insertmacro MUI_PAGE_LICENSE "..\LICENSE"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_LANGUAGE "English"  
  
; Page components
; Page directory
; Page instfiles

; UninstPage uninstConfirm
; UninstPage instfiles


	
Section "Fusion Game Library Core" Section1

  SectionIn RO
  
  
  ; Set output path to the installation directory.
  SetOutPath "$INSTDIR\Bin"
  
  ; Binary stuff :
  File "..\Fusion\bin\x64\Release\*.dll"
  File "..\FbxTool\x64\Release\*.exe"
  File "..\Libs\FbxSdk\lib\vs2012\x64\release\*.dll"
  File "..\Tools\*.dll"
  File "..\Tools\*.exe"
  File "..\Tools\*.com"

  ; Content stuff :
  SetOutPath "$INSTDIR\Content"
  File "..\FusionContent\*.*"

  ; Build stuff :
  SetOutPath "$INSTDIR\Build"
  File "..\FusionProject.targets"
  File "..\FusionFramework.targets"
  
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\FusionGameLib "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\FusionGameLib" "DisplayName" "FusionGameLib"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\FusionGameLib" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\FusionGameLib" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\FusionGameLib" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
SectionEnd



Section "Fusion Game Library Samples"
	
	SetOutPath "$INSTDIR\Samples"
	File /r /x bin /x obj /x Temp "..\FusionSamples\FusionSamples.sln"

	SetOutPath "$INSTDIR\Samples\AnimationDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\AnimationDemo\*.*"

	SetOutPath "$INSTDIR\Samples\ComputeDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\ComputeDemo\*.*"

	SetOutPath "$INSTDIR\Samples\DeferredDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\DeferredDemo\*.*"

	SetOutPath "$INSTDIR\Samples\DescriptorDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\DescriptorDemo\*.*"

	SetOutPath "$INSTDIR\Samples\InputDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\InputDemo\*.*"

	SetOutPath "$INSTDIR\Samples\SpriteDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\SpriteDemo\*.*"

	SetOutPath "$INSTDIR\Samples\QuadDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\QuadDemo\*.*"

	SetOutPath "$INSTDIR\Samples\InstancingDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\InstancingDemo\*.*"

	SetOutPath "$INSTDIR\Samples\InstancingDemo2"
	File /r /x bin /x obj /x Temp "..\FusionSamples\InstancingDemo2\*.*"

	SetOutPath "$INSTDIR\Samples\SoundDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\SoundDemo\*.*"

	SetOutPath "$INSTDIR\Samples\SkinningDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\SkinningDemo\*.*"

	SetOutPath "$INSTDIR\Samples\SceneDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\SceneDemo\*.*"

	SetOutPath "$INSTDIR\Samples\ParticleDemo"
	File /r /x bin /x obj /x Temp "..\FusionSamples\ParticleDemo\*.*"

	SetOutPath "$INSTDIR\Samples\ParticleDemo2"
	File /r /x bin /x obj /x Temp "..\FusionSamples\ParticleDemo2\*.*"

SectionEnd


  
Section "Install DirectX End-User Runtimes (June 2010)" Section2
  SetOutPath "$INSTDIR\DirectX"
  NSISdl::download /TIMEOUT=30000 "http://download.microsoft.com/download/8/4/A/84A35BF1-DAFE-4AE8-82AF-AD2AE20B6B14/directx_Jun2010_redist.exe" "$INSTDIR\DirectX\dxsetup.exe"
  Pop $R0
  StrCmp $R0 success success
    SetDetailsView show
    DetailPrint "Download failed : $R0"
    Abort
  success:
    ExecWait '"$INSTDIR\DirectX\dxsetup.exe" /q /t:"$INSTDIR\DirectX\Temp"'
	ExecWait '"$INSTDIR\DirectX\Temp\DXSETUP.exe" /silent'
	Delete   '$INSTDIR\DirectX\Temp\*.*'
	RMDir    '$INSTDIR\DirectX\Temp'
SectionEnd



Section "Install Visual Studio Project Template"
	File /oname=$PLUGINSDIR\FusionTemplate.vsix "FusionTemplate.vsix"
	ExecShell "open" '$PLUGINSDIR\FusionTemplate.vsix'
SectionEnd

Section "Add Environment Variables"
	ExecWait 'setx FUSION_BIN "$INSTDIR\Bin" /M'
	ExecWait 'setx FUSION_BUILD "$INSTDIR\Build" /M'
	ExecWait 'setx FUSION_CONTENT "$INSTDIR\Content" /M'
SectionEnd


;--------------------------------

; Uninstaller

Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\FusionGameLib"
  DeleteRegKey HKLM SOFTWARE\FusionGameLib

  ; Remove files and uninstaller
  Delete $INSTDIR\Bin\*.*
  Delete $INSTDIR\Content\*.*
  Delete $INSTDIR\*.*
  RMDir /r "$INSTDIR"

  ; Remove shortcuts, if any
  ;-------!! Delete "$SMPROGRAMS\FusionGameLib\*.*"
  ; Remove directories used
  ;-------!! RMDir "$SMPROGRAMS\FusionGameLib"

SectionEnd
