SubWCRev.exe "..\." installer.nsi installertmp.nsi
"%programfiles(x86)%\NSIS\makensis.exe" installertmp.nsi
del installertmp.nsi