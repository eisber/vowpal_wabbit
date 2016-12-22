@echo off
pushd csharp
c:\Tools\swigwin-3.0.10\swig.exe -c++ -namespace VW.API -O -outcurrentdir -csharp ..\vw_api.i
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
pushd %%i
c:\Tools\swigwin-3.0.10\swig.exe -c++ -O -outcurrentdir -%%i ..\vw_api.i
popd
)

