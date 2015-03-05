; example2.nsi
;
; This script is based on example1.nsi, but it remember the directory, 
; has uninstall support and (optionally) installs start menu shortcuts.
;
; It will install example2.nsi into a directory that the user selects,

;--------------------------------

; The name of the installer
Name "Fusion Game Library (v0.9.$WCREV$)"
XPStyle on

; The file to write
OutFile "FusionInstaller-0.9.$WCREV$.exe"

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



Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles


Section "Fusion Game Library Core"

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
	File /r /x bin /x obj "..\FusionSamples\FusionSamples.sln"

	SetOutPath "$INSTDIR\Samples\AnimationDemo"
	File /r /x bin /x obj "..\FusionSamples\AnimationDemo\*.*"

	SetOutPath "$INSTDIR\Samples\ComputeDemo"
	File /r /x bin /x obj "..\FusionSamples\ComputeDemo\*.*"

	SetOutPath "$INSTDIR\Samples\DeferredDemo"
	File /r /x bin /x obj "..\FusionSamples\DeferredDemo\*.*"

	SetOutPath "$INSTDIR\Samples\DescriptorDemo"
	File /r /x bin /x obj "..\FusionSamples\DescriptorDemo\*.*"

	SetOutPath "$INSTDIR\Samples\InputDemo"
	File /r /x bin /x obj "..\FusionSamples\InputDemo\*.*"

	SetOutPath "$INSTDIR\Samples\SpriteDemo"
	File /r /x bin /x obj "..\FusionSamples\SpriteDemo\*.*"

	SetOutPath "$INSTDIR\Samples\QuadDemo"
	File /r /x bin /x obj "..\FusionSamples\QuadDemo\*.*"

	SetOutPath "$INSTDIR\Samples\InstancingDemo"
	File /r /x bin /x obj "..\FusionSamples\InstancingDemo\*.*"

	SetOutPath "$INSTDIR\Samples\InstancingDemo2"
	File /r /x bin /x obj "..\FusionSamples\InstancingDemo2\*.*"

	SetOutPath "$INSTDIR\Samples\SoundDemo"
	File /r /x bin /x obj "..\FusionSamples\SoundDemo\*.*"

	SetOutPath "$INSTDIR\Samples\SkinningDemo"
	File /r /x bin /x obj "..\FusionSamples\SkinningDemo\*.*"

	SetOutPath "$INSTDIR\Samples\SceneDemo"
	File /r /x bin /x obj "..\FusionSamples\SceneDemo\*.*"

	SetOutPath "$INSTDIR\Samples\ParticleDemo"
	File /r /x bin /x obj "..\FusionSamples\ParticleDemo\*.*"

	SetOutPath "$INSTDIR\Samples\ParticleDemo2"
	File /r /x bin /x obj "..\FusionSamples\ParticleDemo2\*.*"

SectionEnd

Section "Install DirectX End-User Runtime"
	File /oname=$PLUGINSDIR\dxwebsetup.exe "dxwebsetup.exe"
	ExecWait '$PLUGINSDIR\dxwebsetup.exe /Q'
SectionEnd


Section "Install Visual Studio Project Template"
	File /oname=$PLUGINSDIR\FusionTemplate.vsix "FusionTemplate.vsix"
	ExecShell "open" '$PLUGINSDIR\FusionTemplate.vsix'
SectionEnd

Section "Add Environment Variables"
	ExecWait 'setx FUSION_DIR "$INSTDIR" /M'
	ExecWait 'setx FUSION_BIN "$INSTDIR\Bin" /M'
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
