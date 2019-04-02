#tool "nuget:?package=GitVersion.CommandLine&version=3.6.5"
#addin "nuget:?package=Newtonsoft.Json&version=9.0.1"

using Newtonsoft.Json;
using static System.IO.File;

GitVersion versionCache = null;
string fileCachePath = "./build/gitversion.json";

GitVersion GetSourceVersion()
{
	if(versionCache != null)
	{
		return versionCache;
	}
	try
	{
		if(!IsLocal())
		{
			var rawJson = ReadAllText(fileCachePath);
			versionCache = JsonConvert.DeserializeObject<GitVersion>(rawJson);
		}
		else
		{
			versionCache = GitVersion(new GitVersionSettings
			{
				UpdateAssemblyInfo = !IsLocal()
			});
		}
	}
	catch (Exception ex)
	{
		Information(ex.ToString());
		versionCache = new GitVersion
		{
			BranchName = "Unknown",
			Sha = System.Guid.NewGuid().ToString(),
			NuGetVersionV2 = "1.0.0-unknown"
		};
	}
	return versionCache;
}

bool IsLocal() => BuildSystem.IsLocalBuild;
