cmake_minimum_required (VERSION 3.5)
project (ds_explore_parent)

add_subdirectory(cpp)

if(WIN32)
add_subdirectory(csharp)
endif()
