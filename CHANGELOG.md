### 0.1.0 (2018-Nov-09)

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
