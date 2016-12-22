@echo off
pushd ..\api\csharp
c:\Tools\swigwin-3.0.10\swig.exe -c++ -namespace VW.API -O -outcurrentdir -csharp ..\..\vowpalwabbit\vw_api.i
popd

for %%i in (
  java
  lua
  php
  perl
  python
  r
  ) do (
echo building %%i binding
pushd ..\api\%%i
c:\Tools\swigwin-3.0.10\swig.exe -c++ -O -outcurrentdir -%%i ..\..\vowpalwabbit\vw_api.i
popd
)

