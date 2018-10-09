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
 - we intend to merge all this upstream, but review is slow and we need working server now.
 - we intend to move V2 from LiGet to BaGet.

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
