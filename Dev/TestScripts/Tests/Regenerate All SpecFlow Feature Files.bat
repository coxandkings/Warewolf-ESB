cd /d "%~dp0..\.."
.nuget\nuget.exe restore AcceptanceTesting.sln
if not %errorlevel%==0 pause & exit 1
for /d %%d in (*.Specs) do (
	packages\SpecFlow.1.9.0\tools\specflow.exe generateAll "%%d\%%d.csproj" /force /verbose
)
for /d %%d in (Warewolf.AcceptanceTesting.*) do (
	packages\SpecFlow.1.9.0\tools\specflow.exe generateAll "%%d\%%d.csproj" /force /verbose
)
packages\SpecFlow.1.9.0\tools\specflow.exe generateAll Dev2.Studio.UISpecs\Dev2.Studio.UI.Specs.csproj /force /verbose
packages\SpecFlow.1.9.0\tools\specflow.exe generateAll Dev2.Installer.Specs\Warewolf.AcceptanceTesting.Installer.csproj /force /verbose