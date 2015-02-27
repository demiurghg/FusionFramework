#----------------------------------------------------------
# This Powershell script will download and install 
# all required third-party binaries, including:
#   1. Autodesk FBX SDK 2014.1 for VS2010
#   2. SharpDX 3.0.0 binaries.
#   3. DirectX End-User Runtimes (June 2010)
#----------------------------------------------------------

$webclient = New-Object System.Net.WebClient

#
#----------------------------------------------------------
# Download FBX SDK:
#----------------------------------------------------------

Write-Host "Getting FBX SDK..."
$url = "http://images.autodesk.com/adsk/files/fbx20141_fbxsdk_vs2012_win.exe"
$file = "fbxsdk.exe"
$webclient.DownloadFile($url,$file)

Write-Host "Installing..."
$sdk  = ".\fbxsdk.exe"
$arg = "/S /D=$pwd\FbxSdk"
start-process $sdk $arg -wait

Write-Host "Done!"
#>

#
#----------------------------------------------------------
# Download SharpDX:
#----------------------------------------------------------
Write-Host "Getting SharpDX binaries..."
$url = "http://sharpdx.org/upload/SharpDX-SDK-LatestDev.exe"
$file = "sharpdxsdk.exe"
$arg = "-o$pwd\SharpDX -y"
$webclient.DownloadFile($url,$file)

Write-Host "Installing..."
start-process $file $arg -wait

Write-Host "Done!"
#>

<#
#----------------------------------------------------------
# Download  DirectX End-User Runtimes (June 2010) :
#----------------------------------------------------------

Write-Host "Getting DirectX End-User Runtime..."

$url = "http://download.microsoft.com/download/8/4/A/84A35BF1-DAFE-4AE8-82AF-AD2AE20B6B14/directx_Jun2010_redist.exe"
$file = "directx_jun2010_redist.exe"
$webclient.DownloadFile($url,$file)

Write-Host "Extracting..."
$arg = "/q" + " /t:$Env:Temp"
start-process $file $arg -wait

Write-Host "Installing..."
start-process "$Env:Temp\DXSETUP.exe" "/silent" -wait

Write-Host "Done!"
#>

#----------------------------------------------------------
# Wait for any key:
#----------------------------------------------------------

Write-Host "Press any key..."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
