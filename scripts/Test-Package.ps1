$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

# Deliberately outside the checkout: no inherited build props or project references.
$packageTestRoot = Join-Path ([IO.Path]::GetTempPath()) ('portic-package-' + [Guid]::NewGuid().ToString('N'))
$packageFeed = Join-Path $packageTestRoot 'feed'
$consumerRoot = Join-Path $packageTestRoot 'consumer'
New-Item -ItemType Directory -Path $packageFeed, $consumerRoot | Out-Null
dotnet pack (Join-Path $PSScriptRoot '../src/Portic.Core/Portic.Core.csproj') -c Release -o $packageFeed
$package = @(Get-ChildItem $packageFeed -Filter '*.nupkg')
if ($package.Count -ne 1) { throw 'Expected one runtime package.' }
$archive = [IO.Compression.ZipFile]::OpenRead($package[0].FullName)
try {
    $spec = @($archive.Entries | Where-Object FullName -Like '*.nuspec')
    if ($spec.Count -ne 1) { throw 'Expected one nuspec.' }
    $reader = [IO.StreamReader]::new($spec[0].Open())
    try { [xml]$metadata = $reader.ReadToEnd() } finally { $reader.Dispose() }
    if ($metadata.package.metadata.id -ne 'Portic.Core' -or
        $metadata.package.metadata.license.InnerText -ne 'AGPL-3.0-only') { throw 'Invalid package metadata.' }
    $version = $metadata.package.metadata.version
    foreach ($entry in @('lib/net10.0/Portic.Core.dll', 'README.md')) {
        if (-not $archive.GetEntry($entry)) { throw "Missing $entry" }
    }
    $dependencies = @($metadata.package.metadata.dependencies.group.dependency.id)
    if ($dependencies -notcontains 'Portic.Sdk' -or $dependencies -notcontains 'Vev.Fabric.Contracts' -or
        $dependencies -contains 'MinVer') { throw 'Incorrect runtime dependencies.' }
} finally { $archive.Dispose() }

@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
    <PackageReference Include="Portic.Core" Version="$version" />
  </ItemGroup>
</Project>
"@ | Set-Content (Join-Path $consumerRoot 'Consumer.csproj')
$escapedFeed = [Security.SecurityElement]::Escape($packageFeed)
@"
<configuration>
  <packageSources><clear /><add key="built" value="$escapedFeed" /><add key="public" value="https://api.nuget.org/v3/index.json" /></packageSources>
  <packageSourceMapping>
    <packageSource key="built"><package pattern="Portic.Core" /></packageSource>
    <packageSource key="public"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
"@ | Set-Content (Join-Path $consumerRoot 'nuget.config')
Copy-Item (Join-Path $PSScriptRoot 'package-consumer.cs.txt') (Join-Path $consumerRoot 'Program.cs')
dotnet restore (Join-Path $consumerRoot 'Consumer.csproj') --configfile (Join-Path $consumerRoot 'nuget.config') --packages (Join-Path $packageTestRoot 'packages')
dotnet run --project (Join-Path $consumerRoot 'Consumer.csproj') -c Release --no-restore
Write-Output "Standalone package consumer passed ($version)."
