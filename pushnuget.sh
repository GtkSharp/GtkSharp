#!/bin/bash

sudo nuget update -self

for f in BuildOutput/NugetPackages/*.nupkg
do
  echo "Processing $f..."
  nuget push -Verbosity detailed -Source https://www.nuget.org/api/v2/package "$f" $NUGETAPIKEY
done
