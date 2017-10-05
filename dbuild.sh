#!/bin/bash

set -e

docker build -t gtk-sharp-debian9 .

docker run gtk-sharp-debian9 ${@}
