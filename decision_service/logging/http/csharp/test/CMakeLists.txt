cmake_minimum_required (VERSION 3.8)
project (ds_logging_http_cs_test LANGUAGES CSharp)

enable_language(CSharp)
set(PACKAGE_VERSION 1.0)
set(LIBRARY_NAME "Microsoft.DecisionService.Logging.Http.Tests")

# unit tests
add_library(${PROJECT_NAME} UnitTest.cs)
set_target_properties(${PROJECT_NAME} PROPERTIES SUFFIX ".dll")
target_link_libraries("${PROJECT_NAME}" ds_logging_http_cs)

set_target_properties("${PROJECT_NAME}" PROPERTIES
	OUTPUT_NAME ${LIBRARY_NAME}
	VS_DOTNET_REFERENCES "System;System.Web;System.Net.Http;Microsoft.VisualStudio.QualityTools.UnitTestFramework;Newtonsoft.Json"
	VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.5.2")

find_program(NUGET nuget)
target_compile_options(${PROJECT_NAME} PRIVATE "/langversion:6" "/platform:anycpu")
# add_custom_command(TARGET ${PROJECT_NAME} PRE_BUILD
#	COMMAND ${NUGET} install "-Version" 5.2.4 Microsoft.AspNet.WebApi.OwinSelfHost "-OutputDirectory" packages
#	WORKING_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR})

configure_file(${CMAKE_CURRENT_SOURCE_DIR}/packages.config ${CMAKE_CURRENT_BINARY_DIR}/packages.config COPYONLY)
add_custom_target(${PROJECT_NAME}_nuget-restore COMMAND ${NUGET} restore ${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}.sln)
add_dependencies(${PROJECT_NAME} ${PROJECT_NAME}_nuget-restore)

# TODO move to CMake modules
find_program(VSTEST vstest.console.exe
	HINTS "C:/Program Files (x86)/Microsoft Visual Studio 14.0/Common7/IDE/CommonExtensions/Microsoft/TestWindow"
		  "C:/Program Files (x86)/Microsoft Visual Studio 12.0/Common7/IDE/CommonExtensions/Microsoft/TestWindow")

add_test(NAME "${PROJECT_NAME}"
	COMMAND ${VSTEST} $<TARGET_FILE:${PROJECT_NAME}>)
