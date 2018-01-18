#!/bin/bash

for f in BuildOutput/NugetPackages/*.nupkg
do
  echo "Processing $f..."
  mono tools/nuget.exe push -Verbosity detailed -Source https://www.nuget.org/api/v2/package "$f" $NUGETAPIKEY
done
