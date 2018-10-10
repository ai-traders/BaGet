[![Build Status](https://travis-ci.com/ai-traders/BaGet.svg?branch=master)](https://travis-ci.com/ai-traders/BaGet)

# BaGet :baguette_bread:

A lightweight [NuGet service](https://docs.microsoft.com/en-us/nuget/api/overview) implementation.

About this fork:
 - we have previously created and used [LiGet](https://github.com/ai-traders/liget/), now we are moving to BaGet.
 - [upstream BaGet](https://github.com/loic-sharma/BaGet) has several missing pieces which we added in the fork.
 - added unit, integration tests and e2e tests with paket and nuget cli.
 - added release cycle and testing of docker image using continuous delivery practices. [PR](https://github.com/loic-sharma/BaGet/pull/108)
 - implements read-through cache, which [does not work upstream](https://github.com/loic-sharma/BaGet/issues/93)
 - uses paket and FAKE for build system. [PR](https://github.com/loic-sharma/BaGet/pull/108)
 - uses [Carter](https://github.com/CarterCommunity/Carter) for routing rather than bare Asp routing.
 - adds ability to log to graylog
 - adds V2 implementation from LiGet to BaGet.
 - adds compatibility mode with LiGet to ease migration to BaGet.
 - we intend to merge all this upstream, but review is slow and we need working server now.

# Usage

See [releases](https://github.com/ai-traders/BaGet/releases) to get docker image version.

```
docker run -ti -p 9090:9090 tomzo/baget:<version>
```

For persistent data, you should mount **volumes**:
 - `/var/baget/packages` contains pushed private packages
 - `/var/baget/db` contains sqlite database
 - `/var/baget/cache` contains cached public packages

You should change the default api key (`NUGET-SERVER-API-KEY`) used for pushing packages,
by setting SHA256 into `ApiKeyHash` environment variable.

### Logging to graylog

BaGet is using [GELF provider for Microsoft.Extensions.Logging](https://github.com/mattwcole/gelf-extensions-logging)
to optionally configure logging via GELF to graylog.
To configure docker image for logging to your graylog, you can set following environment variables:
```
Graylog__Host=your-graylog.com
Graylog__Port=12201
Graylog__AdditionalFields__environment=development
```

## On client side

### Usage only as private repository

For **dotnet CLI and nuget** you need to configure nuget config `~/.nuget/NuGet/NuGet.Config` with something like:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="baget" value="http://baget:9090/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

For paket, in `paket.dependencies`, just specify another source:
```
source http://baget:9090/v3/index.json
```

### Pushing packages

```
dotnet nuget push mypackage.1.0.0.nupkg --source http://baget:9090/v3/index.json --api-key NUGET-SERVER-API-KEY
```

### Usage as caching proxy

For **dotnet CLI and nuget** you need to configure nuget config `~/.nuget/NuGet/NuGet.Config` with something like:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="baget" value="http://baget:9090/cache/v3/index.json" protocolVersion="3" />
    <add key="baget" value="http://baget:9090/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

For paket, in `paket.dependencies`, just specify baget as the 2 only sources
```
source http://baget:9090/cache/v3/index.json
# public packages...

source http://baget:9090/v3/index.json
# private packages...
```

## Migrating from LiGet

If you have been using LiGet before, then many of your nuget sources in projects,
 could look like this, e.g. in paket:
```
source http://my-nuget.com/api/cache/v3/index.json
# public packages

source http://my-nuget.com/api/v2
# private packages
```
Above endpoints end up in `paket.lock` too.
BaGet has different endpoints (no `/api` before endpoints).
If you want to deploy BaGet in place of LiGet and (at least temporarily) keep above endpoints,
you can enable LiGet compatibity mode in BaGet.
```
LiGetCompat__Enabled=true
```
This will enable following behavior:
 - `/api/cache/v3/index.json` returns same content as original BaGet's `/cache/v3/index.json`.
 - `/api/v2/*` returns **V2** resources, same as `/v2/*`

### Importing packages

To make transition from LiGet or any other server which keeps `.nupkg` files in a directory,
there is an `import` command:
```
dotnet BaGet.dll import --path dir
```
In the docker image you can setup environment variable - `BAGET_IMPORT_ON_BOOT=/data/simple`
which will cause baget to first search for `nupkg` files in `$BAGET_IMPORT_ON_BOOT`, before starting server.
Packages which were already added are skipped.
Setting `BAGET_IMPORT_ON_BOOT=/data/simple` is sufficient for migration from LiGet.

*Note: you only need to set this variable once to perform initial migration.
You should unset it in later deployments to avoid uncessary scanning.*

# Development

We rely heavily on docker to create reproducible development environment.
This allows to execute entire build process on any machine which has:
 - local docker daemon
 - docker-compose
 - `ide` script on path. It is a [CLI toolIDE](https://github.com/ai-traders/ide)
  wrapper around docker and docker-compose which deals with issues such as ownership of files,
  mounting proper volumes, cleanup, etc.

You can execute entire build from scratch to e2e tests (like [travis](.travis.yml)).
 - Install docker daemon if you haven't already
 - Install docker-compose
 - Install IDE
```
sudo bash -c "`curl -L https://raw.githubusercontent.com/ai-traders/ide/master/install.sh`"
```

Then to execute entire build:
```
./tasks.sh all
```

This will pull `dotnet-ide` [docker image](https://github.com/ai-traders/docker-dotnet-ide) which
has all build and test dependencies: dotnet SDK, mono, paket CLI, FAKE, Node.js.

## Release cycle

Releases are automated from the master branch, executed by GoCD pipeline, release is published only if all tests have passed.
[Travis](https://travis-ci.com/ai-traders/BaGet) executes the same tasks in the same environment and is for reference to the public community.
If there is `- Unreleased` note at the top of [Changelog](CHANGELOG.md),
then release is a preview, tagged as `<version>-<short-commit-sha>`.
Otherwise it is a full release, tagged as `<version>`.

### Submitting patches

1. Fork and create branch.
2. Commit your changes.
3. Submit a PR, travis will run all tests.
4. Address issues in the review and build failures.
5. Before merge rebase on master `git rebase -i master` and possibly squash some of the commits.

### Issues

If you have an idea or found a bug, open an issue to discuss it.
