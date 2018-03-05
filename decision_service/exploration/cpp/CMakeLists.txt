cmake_minimum_required (VERSION 3.5)
project (ds_explore_cpp)

set (CMAKE_CXX_STANDARD 11)

include_directories(${Boost_INCLUDE_DIRS} ${cpprest_INCLUDE_DIRS} ${vw_INCLUDE_DIRS} 
        ${CMAKE_CURRENT_SOURCE_DIR} "${CMAKE_CURRENT_SOURCE_DIR}/../../../explore/cpp") 

set(PROJECT_SOURCES ds_explore.cc)

add_library(${PROJECT_NAME} ${PROJECT_SOURCES})

set(${PROJECT_NAME}_INCLUDE_DIRS ${PROJECT_SOURCE_DIR}
    CACHE INTERNAL "${PROJECT_NAME}: Include Directories" FORCE)

# add_subdirectory(test)
