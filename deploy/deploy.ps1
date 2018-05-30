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

.PARAMETER UnregisterUnusedApplicationVersionsAfterUpgrade
Indicates whether to unregister any unused application versions that exist after an upgrade is finished.

.PARAMETER OverrideUpgradeBehavior
Indicates the behavior used to override the upgrade settings specified by the publish profile.
'None' indicates that the upgrade settings will not be overridden.
'ForceUpgrade' indicates that an upgrade will occur with default settings, regardless of what is specified in the publish profile.
'VetoUpgrade' indicates that an upgrade will not occur, regardless of what is specified in the publish profile.

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
.\deploy.ps1 -Configuration [Release,Debug] -PublishProfileName Cloud[File type e.g. .POC01] -ApplicationParameters @{CustomParameter1='MyValue'; CustomParameter2='MyValue'}

Deploy a Release build of the IoT project to a cluster defined in a publish profile file called Cloud.xml

#>

Param
(

    [String]
    $Configuration = "Debug",

    [String]
    $PublishProfileName = "Local.5Node",

    [Hashtable]
    $ApplicationCreateParameters = @{},

    [Hashtable]
    $UpgradeParameters = @{},

    [Boolean]
    $UnregisterUnusedApplicationVersionsAfterUpgrade,

    [String]
    [ValidateSet('None', 'ForceUpgrade', 'VetoUpgrade')]
    $OverrideUpgradeBehavior = 'None',

    [String]
    [ValidateSet('Never','Always','SameAppTypeAndVersion')]
    $OverwriteBehavior = 'SameAppTypeAndVersion',

    [int]
    $CopyPackageTimeoutSec = 1200,

    [Switch]
    $SkipPackageValidation,

    [Switch]
    $UsePublishProfileClusterConnection = $true,

    [Switch]
    $SkipBuild = $true,

    [String]
    $ApplicationToDeploy = "All",

    [String]
    [ValidateSet('New', 'Upgrade')]
    $DeploymentType = "Upgrade",

    [String]
    $ApplicationInstanceToUpgrade

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
$ParametersfileName = "Default.Parameters.xml"

# Get a publish profile from the profile XML files in the Deploy directory
$PublishProfileName = $PublishProfileName + ".Profile.xml"

$PublishProfileFile = [System.IO.Path]::Combine($LocalDir, "Profiles\$PublishProfileName")
$PublishProfile = Read-PublishProfile $PublishProfileFile

echo  ">>>>>> PublishProfilePath=$PublishProfileFile  <<<<<<"
Write-Host ($PublishProfile | Out-String) -ForegroundColor Green 
echo  ">>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<"


$IsUpgrade = $DeploymentType -eq 'Upgrade' -and  ($publishProfile.UpgradeDeployment -and $publishProfile.UpgradeDeployment.Enabled -and $OverrideUpgradeBehavior -ne 'VetoUpgrade') -or $OverrideUpgradeBehavior -eq 'ForceUpgrade'

$UpgradeParameters = $publishProfile.UpgradeDeployment.Parameters

if ($OverrideUpgradeBehavior -eq 'ForceUpgrade')
{
    # Warning: Do not alter these upgrade parameters. It will create an inconsistency with Visual Studio's behavior.
    $UpgradeParameters = @{ UnmonitoredAuto = $true; Force = $true }
}

$UpgradeParameters['UnregisterUnusedVersions'] = $UnregisterUnusedApplicationVersionsAfterUpgrade

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

echo ">>>>>>> Finished Deployment Preparation <<<<<<<<<"
echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"



# Build and package the applications
if( !$SkipBuild )
{
    echo ">>>>>>> About To Start Package Builds <<<<<<<<<"

    & "$LocalDir\buildPackages.cmd"

    echo ">>>>>>> Finished Builds <<<<<<<<<"
    echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
    echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
    echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
}



# Publish the packages

echo  ">>>>>> Application Create Parameters Type=$ApplicationCreateParameters <<<<<<"
Write-Host ($ApplicationCreateParameters | Out-String) -ForegroundColor Green
echo  ">>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<"
echo  ">>>>>> Application Upgrade Parameters Type=$UpgradeParameters <<<<<<"
Write-Host ($UpgradeParameters | Out-String) -ForegroundColor Green
echo  ">>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<"


if ($ApplicationToDeploy -eq 'All'  -or $ApplicationToDeploy -eq 'EventsProcessor' )
{
    if ($IsUpgrade)
    {
       echo ">>>>>>> About To Start Publishing for $EventsProcessorApplicationDir [UPGRADE]<<<<<<<<<"
       echo ">>>>>>> Parameters <<<<<<<<<<<<"
       echo ApplicationName=["$ApplicationInstanceToUpgrade"]
       echo ApplicationPackagePath=["$EventsProcessorApplicationDir\pkg\$Configuration"]
       echo ApplicationParameterFilePath=["$EventsProcessorApplicationDir\pkg\$Configuration\$ParametersfileName"]
       echo SkipPackageValidation=[$SkipPackageValidation]
       echo CopyPackageTimeoutSec=[$CopyPackageTimeoutSec]
       echo ">>>>>>>>>>>>>><<<<<<<<<<<<<<<<<"

           if( $ApplicationInstanceToUpgrade )
           {
               Publish-UpgradedServiceFabricApplication -ApplicationName "$ApplicationInstanceToUpgrade" `
                -ApplicationPackagePath "$EventsProcessorApplicationDir\pkg\$Configuration" `
                -Action "RegisterAndUpgrade" `
                -SkipPackageValidation:$SkipPackageValidation `
                -CopyPackageTimeoutSec $CopyPackageTimeoutSec `
                -ErrorAction Continue `
                -ErrorVariable ProcessError
           }
           else
           {
               Publish-UpgradedServiceFabricApplication `
                -ApplicationPackagePath "$EventsProcessorApplicationDir\pkg\$Configuration" `
                -ApplicationParameterFilePath "$AdminApplicationDir\pkg\$Configuration\$ParametersfileName" `
                -Action "Register" `
                -SkipPackageValidation:$SkipPackageValidation `
                -CopyPackageTimeoutSec $CopyPackageTimeoutSec `
                -ErrorAction Continue `
                -ErrorVariable ProcessError
           }
    }
    else
    {
        echo ">>>>>>> About To Start Publishing for $EventsProcessorApplicationDir [NEW]<<<<<<<<<"
        Publish-NewServiceFabricApplication `
            -ApplicationPackagePath "$EventsProcessorApplicationDir\pkg\$Configuration" `
            -ApplicationParameterFilePath "$LocalDir\Profiles\DummyApplicationParameters.xml" `
            -Action "Register" `
            -ApplicationParameter @{} `
            -OverwriteBehavior $OverwriteBehavior `
            -SkipPackageValidation:$SkipPackageValidation `
            -CopyPackageTimeoutSec $CopyPackageTimeoutSec `
            -ErrorAction Continue `
            -ErrorVariable ProcessError
    }

    if($ProcessError)
    {
        echo ">>>>>>> Finished Publish for $EventsProcessorApplicationDir [WITH ERROR]<<<<<<<<<"
        echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
        echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
    }
    else
    {
        echo ">>>>>>> Finished Publish for $EventsProcessorApplicationDir [SUCCESS] <<<<<<<<<"
        echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
        echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
    }
}

if(!$ProcessError)
{
    if ($ApplicationToDeploy -eq 'All'  -or $ApplicationToDeploy -eq 'Insight' )
    {
        if ($IsUpgrade)
        {
           echo ">>>>>>> About To Start Publishing for $InsightApplicationDir [UPGRADE]<<<<<<<<<"
           echo ">>>>>>> Parameters <<<<<<<<<<<<"
           echo ApplicationName=["$ApplicationInstanceToUpgrade"]
           echo ApplicationPackagePath=["$InsightApplicationDir\pkg\$Configuration"]
           echo ApplicationParameterFilePath=["$InsightApplicationDir\pkg\$Configuration\$ParametersfileName"]
           echo SkipPackageValidation=[$SkipPackageValidation]
           echo CopyPackageTimeoutSec=[$CopyPackageTimeoutSec]
           echo ">>>>>>>>>>>>>><<<<<<<<<<<<<<<<<"

           if( $ApplicationInstanceToUpgrade )
           {
               Publish-UpgradedServiceFabricApplication -ApplicationName "$ApplicationInstanceToUpgrade" `
                    -ApplicationPackagePath "$InsightApplicationDir\pkg\$Configuration" `
                    -Action "RegisterAndUpgrade" `
                    -SkipPackageValidation:$SkipPackageValidation `
                    -CopyPackageTimeoutSec $CopyPackageTimeoutSec `
                    -ErrorAction Continue `
                    -ErrorVariable ProcessError
           }
           else
           {
               Publish-UpgradedServiceFabricApplication `
                    -ApplicationPackagePath "$InsightApplicationDir\pkg\$Configuration" `
                    -ApplicationParameterFilePath "$AdminApplicationDir\pkg\$Configuration\$ParametersfileName" `
                    -Action "Register" `
                    -SkipPackageValidation:$SkipPackageValidation `
                    -CopyPackageTimeoutSec $CopyPackageTimeoutSec `
                    -ErrorAction Continue `
                    -ErrorVariable ProcessError
           }
        }
        else
        {
            echo ">>>>>>> About To Start Publishing for $InsightApplicationDir [NEW]<<<<<<<<<"
            Publish-NewServiceFabricApplication `
                -ApplicationPackagePath "$InsightApplicationDir\pkg\$Configuration" `
                -ApplicationParameterFilePath "$LocalDir\Profiles\DummyApplicationParameters.xml" `
                -Action "Register" `
                -ApplicationParameter @{} `
                -OverwriteBehavior $OverwriteBehavior `
                -SkipPackageValidation:$SkipPackageValidation `
                -CopyPackageTimeoutSec $CopyPackageTimeoutSec `
                -ErrorAction Continue `
                -ErrorVariable ProcessError
        }
    
        if($ProcessError)
        {
            echo ">>>>>>> Finished Publish for $InsightApplicationDir [WITH ERROR]<<<<<<<<<"
            echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
            echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
        }
        else
        {
            echo ">>>>>>> Finished Publish for $InsightApplicationDir [SUCCESS] <<<<<<<<<"
            echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
            echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
        }        
    }
    
    if(!$ProcessError)
    {
        if ($ApplicationToDeploy -eq 'All'  -or $ApplicationToDeploy -eq 'Admin' )
        {
            if ($IsUpgrade)
            {
               echo ">>>>>>> About To Start Publishing for $AdminApplicationDir [UPGRADE]<<<<<<<<<"
               Publish-UpgradedServiceFabricApplication `
                    -ApplicationPackagePath "$AdminApplicationDir\pkg\$Configuration" `
                    -ApplicationParameterFilePath "$AdminApplicationDir\pkg\$Configuration\$ParametersfileName" `
                    -Action "RegisterAndUpgrade" `
                    -SkipPackageValidation:$SkipPackageValidation `
                    -CopyPackageTimeoutSec $CopyPackageTimeoutSec `
                    -ErrorAction Continue `
                    -ErrorVariable ProcessError 
            }
            else
            {
                echo ">>>>>>> About To Start Publishing for $AdminApplicationDir [NEW]<<<<<<<<<"
                Publish-NewServiceFabricApplication `
                    -ApplicationPackagePath "$AdminApplicationDir\pkg\$Configuration" `
                    -ApplicationParameterFilePath "$AdminApplicationDir\pkg\$Configuration\$ParametersfileName" `
                    -Action "RegisterAndCreate" `
                    -ApplicationParameter $ApplicationCreateParameters `
                    -OverwriteBehavior $OverwriteBehavior `
                    -SkipPackageValidation:$SkipPackageValidation `
                    -CopyPackageTimeoutSec $CopyPackageTimeoutSec `
                    -ErrorAction Continue `
                    -ErrorVariable ProcessError
            }

            if($ProcessError)
            {
                echo ">>>>>>> Finished Publish for $AdminApplicationDir [WITH ERROR]<<<<<<<<<"
                echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
                echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
            }
            else
            {
                echo ">>>>>>> Finished Publish for $AdminApplicationDir [SUCCESS] <<<<<<<<<"
                echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
                echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
            }        
        }
    }
}

# Remove all imported modules
Get-Module | Remove-Module -Verbose

echo ">>>>>>>>>>>>>Deployment Complete<<<<<<<<<<<<<<<<<"
echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
echo ">>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<"
