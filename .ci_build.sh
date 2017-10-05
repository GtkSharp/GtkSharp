#!/bin/bash

set -e

export BUILD_DIR="/${SOURCE_DIR}/build"

cd "${SOURCE_DIR}"
rm -Rf "${BUILD_DIR}"
meson "${BUILD_DIR}"

cd "${BUILD_DIR}"
ninja
ninja -C "${BUILD_DIR}" test
