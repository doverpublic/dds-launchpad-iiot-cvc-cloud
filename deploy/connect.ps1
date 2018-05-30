<#
.SYNOPSIS
Builds and deploys the LaunchpadIoT solution to a cluster.

.DESCRIPTION
A deployment of the LaunchpadIoT solution does the following:
 - Register and create an instance of the Launchpad.IoT.Admin.Application
 - Register the Launchpad.IoT.EventsProcessor.Application type but don't create an instance.
 - Register the Launchpad.IoT.Insight.Application type but don't create an instance.

.PARAMETER Configuration
Build configuration used to build the solution. Example: Release, Debug. Default is Debug.

.PARAMETER PublishProfileName
Name of the publish profile XML file to use for publishing. Example: Cloud, Local.5Node. Default is Local.5Node.

.PARAMETER ApplicationParameter
Hashtable of the Service Fabric application parameters to be used for the application.

.PARAMETER OverwriteBehavior
Overwrite Behavior if an application exists in the cluster with the same name. Available Options are Never, Always, SameAppTypeAndVersion.
This setting is not applicable when upgrading an application.
'Never' will not remove the existing application. This is the default behavior.
'Always' will remove the existing application even if its Application type and Version is different from the application being created.
'SameAppTypeAndVersion' will remove the existing application only if its Application type and Version is same as the application being created.

.PARAMETER CopyPackageTimeoutSec
Timeout in seconds for copying application package to image store.

.PARAMETER SkipPackageValidation
Switch signaling whether the package should be validated or not before deployment.

.PARAMETER UseExistingClusterConnection
Indicates that the script should make use of an existing cluster connection that has already been established in the PowerShell session.
The cluster connection parameters configured in the publish profile are ignored.

.EXAMPLE
.\deploy.ps1

Deploy a Debug build of the IoT project to a local 5-node cluster.

.EXAMPLE
.\deploy.ps1 -Configuration [Release,Debug] -PublishProfileName Cloud[File type e.g. .POC01]

Deploy a Release build of the IoT project to a cluster defined in a publish profile file called Cloud.xml

#>

Param
(

    [String]
    $Configuration = "Debug",

    [String]
    $PublishProfileName = "Local.5Node",

    [Hashtable]
    $ApplicationParameters = @{},

    [String]
    [ValidateSet('Never','Always','SameAppTypeAndVersion')]
    $OverwriteBehavior = 'SameAppTypeAndVersion',

    [int]
    $CopyPackageTimeoutSec = 600,

    [Switch]
    $SkipPackageValidation,

    [Switch]
    $UsePublishProfileClusterConnection = $true,

    [Switch]
    $SkipBuild = $true
)

echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
echo ">>>>>>> About To Start Deployment <<<<<<<<<<<<<<<"

echo ">>>>>>> Starting Deployment Preparation <<<<<<<<<"

# Get references to the solution directory and the directory of this script.
$LocalDir = (Split-Path $MyInvocation.MyCommand.Path)
$SolutionDir = [System.IO.Path]::Combine((get-item $LocalDir).Parent.FullName, "src")

# Locations of the three applications
$AdminApplicationDir = "$SolutionDir\Launchpad.Iot.Admin.Application"
$EventsProcessorApplicationDir = "$SolutionDir\Launchpad.Iot.EventsProcessor.Application"
$InsightApplicationDir = "$SolutionDir\Launchpad.Iot.Insight.Application"


# Import the Service Fabric SDK PowerShell module and a functions module included with the solution.
# This is installed with the Service Fabric SDK.
$RegKey = "HKLM:\SOFTWARE\Microsoft\Service Fabric SDK"
$ModuleFolderPath = (Get-ItemProperty -Path $RegKey -Name FabricSDKPSModulePath).FabricSDKPSModulePath

# This import will make the service fabric functions run in their own scope
# Import-Module "$ModuleFolderPath\ServiceFabricSDK.psm1"

# This import will make the service fabric functions run in the global scope (thus it can see the connection created in this script)
Import-Module "$ModuleFolderPath\ServiceFabricSDK.ps1"


# This is included with the solution
Import-Module "$LocalDir\Scripts\functions.psm1"

# Get the parameters file first so that one can reuse the profile name profile filename
$ParametersfileName = $PublishProfileName + ".Parameters.xml"

# Get a publish profile from the profile XML files in the Deploy directory
$PublishProfileName = $PublishProfileName + ".Profile.xml"

$PublishProfileFile = [System.IO.Path]::Combine($LocalDir, "Profiles\$PublishProfileName")
$PublishProfile = Read-PublishProfile $PublishProfileFile

$ParametersfileName = [System.IO.Path]::Combine($LocalDir, "Profiles\$ParametersfileName")


# Using the publish profile, connect to the SF cluster
if ($UsePublishProfileClusterConnection)
{
    $ClusterConnectionParameters = $publishProfile.ClusterConnectionParameters
    if ($SecurityToken)
    {
        $ClusterConnectionParameters["SecurityToken"] = $SecurityToken
    }

    try
    {
        Connect-ServiceFabricCluster @ClusterConnectionParameters
    }
    catch [System.Fabric.FabricObjectClosedException]
    {
        Write-Warning "Service Fabric cluster may not be connected."
        throw
    }
}
else
{
	echo "Open cluster connection to local cluster"
    # This is to force the connection to the local cluster if still not connected
    Connect-ServiceFabricCluster 
}

try
{
	echo "Test the cluster connection"
    Test-ServiceFabricClusterConnection
}
catch
{
	Write-Host "Please connect to a cluster."
	Exit
}

Repair-ServiceFabricPartition -All
