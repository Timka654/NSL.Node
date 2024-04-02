###########################################################
#
# Script to upgrade all NuGet packages in solution to last version
#
# USAGE
# Place this file (Upgrade-Package.ps1) to your solution folder. 
# From Package Manager Console execute
#
# .\Upgrade-Package.ps1 -PackageID:Castle.Core
#
# Do not hestitate to contact me at any time
# mike@chaliy.name, http://twitter.com/chaliy
#
# Update to NuGet 1.1 is done by JasonGrundy, see comments bellow
#
##########################################################

param($PackageID)

$packageManager = $host.PrivateData.packageManagerFactory.CreatePackageManager()

foreach ($project in Get-Project) {
	$fileSystem = New-Object NuGet.PhysicalFileSystem($project.Properties.Item("FullPath").Value) 	
	$repo = New-Object NuGet.PackageReferenceRepository($fileSystem, $packageManager.LocalRepository)

	foreach ($package in $repo.GetPackages() | ? {$_.Id -like "*" + $PackageID +"*"}) {
		Update-Package $package.Id -Project:$project.Name 
	}
}