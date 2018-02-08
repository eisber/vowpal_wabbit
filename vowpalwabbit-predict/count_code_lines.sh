#!/bin/sh
echo "level 1: no hashing, no model parsing"
cat cb_example.cc example.h features.h model.cc cb_example.h example.cc features.cc model.h | wc -l
