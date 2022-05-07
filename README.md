# P3D-Legacy-Server
<p align="center">
  <a href="https://github.com/P3D-Legacy/P3D-Legacy-Server" alt="Lines Of Code">
    <img src="https://tokei.rs/b1/github/P3D-Legacy/P3D-Legacy-Server?category=code" />
  </a>
  <a href="https://www.codefactor.io/repository/github/p3d-legacy/p3d-legacy-server">
    <img src="https://www.codefactor.io/repository/github/p3d-legacy/p3d-legacy-server/badge" alt="CodeFactor" />
  </a>
  <a href="https://codeclimate.com/github/P3D-Legacy/P3D-Legacy-Server/maintainability">
    <img alt="Code Climate maintainability" src="https://img.shields.io/codeclimate/maintainability-percentage/P3D-Legacy/P3D-Legacy-Server">
  </a>
  <!--
  <a href="https://p3d-legacy.github.io/P3D-Legacy-Server/index.html" alt="Documentation">
    <img src="https://img.shields.io/badge/Documentation-%F0%9F%94%8D-blue?style=flat" />
  </a>
  -->
  <a href="https://p3d-legacy.github.io/P3D-Legacy-Server/index.html" alt="Swagger">
    <img alt="Swagger Validator" src="https://img.shields.io/swagger/valid/3.0?specUrl=https%3A%2F%2Fp3d-legacy.github.io%2FP3D-Legacy-Server%2Fopenapi.json">
  </a>
  </br>
  <a href="https://github.com/P3D-Legacy/P3D-Legacy-Server/actions/workflows/docker-publish.yml">
    <img alt="Latest Build" src="https://img.shields.io/github/workflow/status/P3D-Legacy/P3D-Legacy-Server/Publish%20Docker%20Image%20on%20Release">
  </a>
  <a href="https://github.com/P3D-Legacy/P3D-Legacy-Server/actions/workflows/docker-publish-qa.yml">
    <img alt="Latest QA Build" src="https://img.shields.io/github/workflow/status/P3D-Legacy/P3D-Legacy-Server/Publish%20Docker%20Image%20on%20Commit">
  </a>
  <a href="https://codecov.io/gh/P3D-Legacy/P3D-Legacy-Server">
    <img src="https://codecov.io/gh/P3D-Legacy/P3D-Legacy-Server/branch/master/graph/badge.svg" />
  </a>
</p>

The new 'enterprise grade' © server for P3D-Legacy based on ASP.NET Core and 
[Project Bedrock](https://github.com/davidfowl/BedrockFramework).

## How To Get It
### Executable
We provide statically compiled binaries for every supported by .NET platform.  
Check the [Releases](https://github.com/P3D-Legacy/P3D-Legacy-Server/releases/latest)
tab for the latest release available.

### Docker Image
We provide the image [ghcr.io/p3d-legacy/p3d-legacy-server](https://github.com/P3D-Legacy/P3D-Legacy-Server/pkgs/container/p3d-legacy-server) 
with two main tags:  
* `latest`/`latest-qa` for Windows(x64)/Debian(x64, ARM64, ARM32) based images.
They contain the full ASP.NET Core runtime.  
* `latest-alpine`/`latest-alpine-qa` for slimmed down Alpine(x64, ARM64, ARM32) images.
They contain the statically compiled application and also uses MUSL for the C runtime.  

The `-qa` stands for pre-release builds.  
We recommend to use a `docker-compose.yml` file for deployment.

## Configuration
You can directly modify the
[appsettings.json](https://github.com/P3D-Legacy/P3D-Legacy-Server/blob/master/src/P3D.Legacy.Server/appsettings.json)
or use
[environment variables](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#naming-of-environment-variables-1).
So `Server:Name` will be `Server__Name` as an env variable.

## UI
The server has a basic UI. Type `/uimode` when the server is running.
![image](https://media.discordapp.net/attachments/422092475163869201/972552577189294141/unknown.png?width=800&height=428)
