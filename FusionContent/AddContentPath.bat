@REM ==============================================================
@REM Run this batch file to add current directory to FUSION_CONTENT 
@REM Do not forget to restart Visual Studio.
@REM ==============================================================

setx FUSION_CONTENT "%cd%;%FUSION_CONTENT%" /M

@echo FUSION_BIN     %FUSION_BIN%
@echo FUSION_CONTENT %FUSION_CONTENT%

@echo Restart MS Visual Studio and command line tools.
@pause