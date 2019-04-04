#addin "nuget:?package=Cake.MiniCover&version=0.29.0-next20180721071547&prerelease"

#l publish.cake

var target = Argument("target", "build");
var configuration = Argument("configuration", "Release");
var version = Argument<string>("version", "0.0.1-ci");

System.IO.Directory.SetCurrentDirectory(MakeAbsolute(Directory("../")).FullPath);
SetMiniCoverToolsProject("./ci/minicover.csproj");

var sln = "OctoConfigTool.sln";
var nugetPublishUrl = Argument<string>("nugetPublishUrl", null);
var nugetApiKey = Argument<string>("nugetApiKey", null);

Setup(context =>
	{
		Information("Version: {0}", version);
		Verbose("Current working directory {0}", System.IO.Directory.GetCurrentDirectory());
	});

Task("clean")
	.Does(() =>
	{
		DotNetCoreClean(
			project: sln,
			settings: new DotNetCoreCleanSettings {
				Verbosity = DotNetCoreVerbosity.Quiet,
				Configuration = configuration
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
				Configuration = configuration
			});
	});

Task("test")
	.Does(() =>
	{
		MiniCover(tool =>
			{
				foreach(var proj in GetFiles("./test/**/*.csproj"))
				{
					Information("Testing Project: " + proj);
					DotNetCoreTest(proj.FullPath, new DotNetCoreTestSettings
					{
						NoBuild = true,
						Configuration = configuration,
						ArgumentCustomization = args => args.Append("--no-restore")
					});
				}
			},
			new MiniCoverSettings()
				.WithAssembliesMatching("./test/**/*.dll")
				.WithSourcesMatching("./src/**/*.cs")
				.WithNonFatalThreshold()
				.GenerateReport(ReportType.CONSOLE)
		);
	});

Task("Coveralls")
    .WithCriteria(TravisCI.IsRunningOnTravisCI)
    .Does(() =>
{
    MiniCoverReport(new MiniCoverSettings()
        .WithCoverallsSettings(c => c.UseTravisDefaults())
        .GenerateReport(ReportType.COVERALLS)
    );
});

Task("package")
	.Does(() =>
	{
		BuildPackages(version);
		BuildDocker(version);
	});

Task("publish")
	.Does(() =>
	{
		PublishPackages(nugetPublishUrl, nugetApiKey);
		PublishDocker(version);
	});

Task("ci-test")
	.IsDependentOn("clean")
	.IsDependentOn("build")
	.IsDependentOn("test");

Task("ci-publish")
	.IsDependentOn("clean")
	.IsDependentOn("build")
	.IsDependentOn("test")
	.IsDependentOn("package");
	//.IsDependentOn("publish");

Task("Default")
    .IsDependentOn("ci-test");


RunTarget(target);
