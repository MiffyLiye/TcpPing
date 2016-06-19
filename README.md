# TcpPing: A Portable TCP Ping Tool

[![Build Status](https://travis-ci.org/MiffyLiye/TcpPing.svg?branch=master)](https://travis-ci.org/MiffyLiye/TcpPing)
[![Build status](https://ci.appveyor.com/api/projects/status/suru6w0479ddcb32?svg=true)](https://ci.appveyor.com/project/MiffyLiye/tcpping)

## Tech Stack
* Language: C# 6.0
* Runtime: Mono, .NET Core
* Test Framework: Machine.Specifications

## How to use
See release notes in each release.

## Build from source
```shell
xbuild /p:Configuration=Release ./TcpPing.sln
```

## Run automated tests
* First install Machine.Specifications.Runner.Console with nuget
```shell
nuget install Machine.Specifications.Runner.Console
```
Assume that it is installed to ./Machine.Specifications.Runner.Console/

* Then build the test project in Release mode. 
```shell
xbuild /p:Configuration=Release ./TcpPingTest/TcpPingTest.csproj 
```
Assume that the result is ./TcpPingTest/bin/Release/TcpPingTest.dll

* Finally run tests in TcpPingTest.dll
```shell
mono ./Machine.Specifications.Runner.Console/tools/mspec.exe ./TcpPingTest/bin/Release/TcpPingTest.dll
```

## Tested Platforms
* OS X
* Windows

## Known Issues
See issues.
