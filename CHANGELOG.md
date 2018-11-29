### 0.3.0 (2018-Nov-29)

Cherry-pick of upstream changes
 * improve database search
 * package deletion service with different behaviors
 * refactoring of storage services and tests

Fix not returning unlisted packages in caching proxy.

### 0.2.1 (2018-Oct-22)

 * Fixed missing dependencies in V3 endpoints \#12

### 0.2.0 (2018-Oct-11)

 * Added V2 implementation from LiGet
 * Added compatibility mode with LiGet to keep the same endpoints
 * V2 includes dependencies in package query results
 * Switch production base image to slim stretch
 * added importer to complete migration from LiGet
 * fix/adjust for deployments with root-owned volumes

### 0.1.0 (2018-Oct-09)

First release with a changelog.
 - added unit, integration tests and e2e tests with paket and nuget cli.
 - added release cycle and testing of docker image using continuous delivery practices.
 - implements read-through cache, which [does not work upstream](https://github.com/loic-sharma/BaGet/issues/93)
 - uses paket and FAKE for build system.
 - uses [Carter](https://github.com/CarterCommunity/Carter) for routing rather than bare Asp routing.
 - adds ability to log to graylog
 - builds SPA as part of pipeline
 - fixes handling package upload by [older clients and paket](https://github.com/loic-sharma/BaGet/issues/106)
 - added flag to run database migrations only when enabled
