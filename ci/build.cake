#l publish.cake
#l helpers.cake

var version = GetSourceVersion();

var target = Argument("target", "build");

System.IO.Directory.SetCurrentDirectory(MakeAbsolute(Directory("../")).FullPath);

var sln = "OctoConfigTool.sln";
var nugetPublishUrl = Argument<string>("nugetPublishUrl", null);
var nugetApiKey = Argument<string>("nugetApiKey", null);

Setup(context =>
	{
		Information("Version: {0}", version.NuGetVersionV2);
		Information("Branch:  {0}", version.BranchName);
	});

Task("clean")
	.Does(() =>
	{
		Verbose(System.IO.Directory.GetCurrentDirectory());
		DotNetCoreClean(
			project: sln,
			settings: new DotNetCoreCleanSettings {
				Verbosity = DotNetCoreVerbosity.Quiet,
				Configuration = "Release"
			});
		CleanDirectories(new []{ "build" });
	});

Task("build")
	.Does(() =>
	{
		DotNetCoreRestore(
			settings: new DotNetCoreRestoreSettings {
				Verbosity = DotNetCoreVerbosity.Quiet
			});

		DotNetCoreBuild(
			project: sln,
			settings: new DotNetCoreBuildSettings {
				Verbosity = DotNetCoreVerbosity.Quiet,
				Configuration = "Release"
			});
	});

Task("test")
	.Does(() =>
	{
		Information("Running unit tests");
		var resultDir = MakeAbsolute(Directory("build/test_results"));
		var testProjects = GetFiles("test/**/*Tests*.csproj");
		foreach(var testProject in testProjects)
		{
			DotNetCoreTest(
				project: testProject.FullPath,
				settings: new DotNetCoreTestSettings {
					Logger = "trx;LogFileName=UnitTests.trx",
					WorkingDirectory = "build",
					Configuration = "Release",
					NoBuild = true,
					ArgumentCustomization = a =>
						a.Append($"-r {resultDir.FullPath}")
				});
		}
	});

Task("package")
	.Does(() =>
	{
		BuildPackages(version);
	});

Task("publish")
	.Does(() =>
	{
		PublishPackages(nugetPublishUrl, nugetApiKey);
	});

Task("prepare-docker")
	.Does(() =>
	{
		PrepareForDockerBuild(version);
	});

Task("jenkins-build")
	.IsDependentOn("clean")
	.IsDependentOn("build");

Task("jenkins-test")
	.IsDependentOn("test");

Task("jenkins-publish-nuget")
	.IsDependentOn("package")
	.IsDependentOn("publish");

RunTarget(target);
