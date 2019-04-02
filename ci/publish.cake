using System.Linq;

void BuildPackages(GitVersion version)
{
	var packSettings = new DotNetCorePackSettings {
		OutputDirectory = "build/",
		Configuration = "Release",
		NoBuild = false,
		MSBuildSettings = new DotNetCoreMSBuildSettings()
	};

	packSettings.MSBuildSettings.Properties["PackageVersion"] =
			new [] { version.NuGetVersionV2 };

	DotNetCorePack(
		project: "src/OctoConfig.Core/OctoConfig.Core.csproj",
		settings: packSettings);

	DotNetCorePack(
		project: "src/OctoConfigTool/OctoConfigTool.csproj",
		settings: packSettings);
}

void PublishPackages(string source, string apiKey)
{
	var nupkgs = GetFiles($"build/*.nupkg");

	if(!nupkgs.Any())
	{
		throw new Exception($"No nuget packages found in directory 'build");
	}

	if (source == null) {
		throw new Exception("missing required parameter \"nugetPublishUrl\"");
	}

	if (apiKey == null) {
		throw new Exception("missing required parameter \"nugetApiKey\"");
	}

	foreach(var package in nupkgs)
	{
		DotNetCoreNuGetPush(
			package.FullPath,
			new DotNetCoreNuGetPushSettings {
				Source = source,
				ApiKey = apiKey
			});
	}
}

const string imageName = "hcr.io/cloud-platform/octoconfigtool";

void PrepareForDockerBuild(GitVersion version)
{
	var publishSettings = new DotNetCorePublishSettings {
		Framework = "netcoreapp2.1",
		OutputDirectory = "build/docker",
		Configuration = "Release",
		MSBuildSettings = new DotNetCoreMSBuildSettings()
	};

	publishSettings.MSBuildSettings.Properties["PackageVersion"] =
			new [] { version.NuGetVersionV2 };

	DotNetCorePublish(
		project: "src/OctoConfigTool/OctoConfigTool.csproj",
		settings: publishSettings);
}
