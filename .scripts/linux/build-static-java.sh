#!/bin/bash
set -e
set -x

GCC=`which gcc`
GPP=`which g++`

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_DIR=$SCRIPT_DIR/../../
cd $REPO_DIR

mkdir -p build
cd build
# Boost unit tests don't like the static linking
# /usr/local/bin/gcc + g++ is 9.2.0 version
cmake .. -DCMAKE_BUILD_TYPE=Release -DWARNINGS=Off -DBUILD_JAVA=On -DBUILD_DOCS=Off \
 -DBUILD_PYTHON=Off -DSTATIC_LINK_VW_JAVA=On -DCMAKE_C_COMPILER=$GCC -DCMAKE_CXX_COMPILER=$GPP \
 -DBUILD_TESTS=Off
NUM_PROCESSORS=$(cat nprocs.txt)
make all -j ${NUM_PROCESSORS}
