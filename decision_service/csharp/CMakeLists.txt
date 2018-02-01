cmake_minimum_required (VERSION 2.8)
project (ds_cs)

set (CMAKE_CXX_STANDARD 14)

include_directories(${Boost_INCLUDE_DIRS} ${cpprest_INCLUDE_DIRS} ${vw_INCLUDE_DIRS} ${ds_INCLUDE_DIRS} ${CMAKE_CURRENT_SOURCE_DIR}) 
set_property(SOURCE ds_api_csharp.i ../ds_api.i PROPERTY CPLUSPLUS ON)

enable_language(CSharp)

set(CMAKE_SWIG_OUTDIR "swig")
set(${PROJECT_NAME}_SWIG_SOURCE_DIR "${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_SWIG_OUTDIR}")

set_property(SOURCE ds_api_csharp.i PROPERTY SWIG_FLAGS "-namespace" "Microsoft.DecisionService")

set_source_files_properties("swig/ds_api_csharpCSHARP_wrap"
	PROPERTIES COMPILE_FLAGS "-DSWIG_CSHARP")

IF (${CMAKE_MAJOR_VERSION}.${CMAKE_MINOR_VERSION} GREATER 3.8)
	# ../ds_api.i
	swig_add_library(${PROJECT_NAME} LANGUAGE csharp SOURCES ds_api_csharp.i)
ELSE()
	SWIG_ADD_MODULE(${PROJECT_NAME} csharp ds_api_csharp.i)
ENDIF()

SWIG_LINK_LIBRARIES(${PROJECT_NAME} ds)

INSTALL(TARGETS ${PROJECT_NAME} LIBRARY DESTINATION bin)

set(${PROJECT_NAME}_dll "${CMAKE_INSTALL_PREFIX}/bin/${PROJECT_NAME}.dll")
Install_Dependencies(${PROJECT_NAME}_dll)

add_subdirectory(managed)
