#!/bin/bash
set -e

if [ -z "${E2E_PAKET_VERSION}" ]; then
  echo "E2E_PAKET_VERSION is not set"
  exit 2;
fi

echo "Overriding nuget configuration in /home/ide/.nuget/NuGet/NuGet.Config"
cat << EOF > /home/ide/.nuget/NuGet/NuGet.Config
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="baget" value="http://baget:9090/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
EOF

echo "Sleeping 4s to wait for server to be ready"
sleep 4

E2E_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd $E2E_DIR
mono .paket/paket.bootstrapper.exe

cat << EOF > $E2E_DIR/.paket/paket.bootstrapper.exe.config
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="PreferNuget" value="True"/>
    <add key="PaketVersion" value="${E2E_PAKET_VERSION}"/>
  </appSettings>
</configuration>
EOF

for test_dir in `find $E2E_DIR -mindepth 1 -type d -name 'test_*'`
do
    echo "Running tests in $test_dir"
    cd $test_dir && bats .
done
