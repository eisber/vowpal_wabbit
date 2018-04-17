cmake_minimum_required (VERSION 3.5)
project (ds)

set (CMAKE_CXX_STANDARD 14)

# BOOST python
find_package(Boost ${Boost_VW_VERSION} COMPONENTS system REQUIRED)
find_package(cpprestsdk REQUIRED)
find_package(OpenSSL REQUIRED)
# find_package(PythonLibs REQUIRED)


if(WIN32)
	# disable warning on exception as it's required for SWIG
	add_compile_options(/wd4290)
endif()

include_directories(${Boost_INCLUDE_DIRS} ${cpprest_INCLUDE_DIRS} ${vw_INCLUDE_DIRS} 
        ${CMAKE_CURRENT_SOURCE_DIR} "${CMAKE_CURRENT_SOURCE_DIR}/../explore/cpp") 
# include_directories(${vw_INCLUDE_DIRS} ${PYTHON_INCLUDE_DIRS}) 

set(PROJECT_SOURCES ds_client.cc ds_internal.cc ds_rank_response.cc ds_config.cc ds_vw.cc event_hub_client.cc 
    ds_explore.cc ds_explore_epsilon.cc ds_explore_bag.cc ds_explore_softmax.cc ds_action_probabilities.cc
    ds_predictor_container.cc ds_predictors.cc)

add_library(${PROJECT_NAME} ${PROJECT_SOURCES})
target_link_libraries(${PROJECT_NAME} PRIVATE vw ${Boost_LIBRARIES} ${cpprest_LIBRARY} OpenSSL::SSL OpenSSL::Crypto)

set(${PROJECT_NAME}_INCLUDE_DIRS ${PROJECT_SOURCE_DIR}
    CACHE INTERNAL "${PROJECT_NAME}: Include Directories" FORCE)

find_package(SWIG REQUIRED)
include(UseSWIG)

add_subdirectory(exploration)
add_subdirectory(logging)
add_subdirectory(prediction)
# add_subdirectory(python)
add_subdirectory(test)

IF (WIN32)
	add_subdirectory(csharp)
ENDIF()

