cmake_minimum_required (VERSION 3.8)
project (ds_logging_http_cs LANGUAGES CSharp)

enable_language(CSharp)
set(PACKAGE_VERSION 1.0)
set(LIBRARY_NAME "Microsoft.DecisionService.Logging.Http")

# main library
add_library(${PROJECT_NAME} DataLogger.cs RewardLogger.cs EventHubClient.cs EventHubConnection.cs SasTokenHelper.cs SasTokenProvider.cs)

set_target_properties(${PROJECT_NAME} PROPERTIES
	OUTPUT_NAME ${LIBRARY_NAME}
	VS_DOTNET_REFERENCES "System;System.Web;System.Net.Http"
	VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.5.2")

target_compile_options(${PROJECT_NAME} PRIVATE "/langversion:6" "/platform:anycpu" "/p:NET40")

find_program(NUGET nuget)
configure_file(${CMAKE_CURRENT_SOURCE_DIR}/packages.config ${CMAKE_CURRENT_BINARY_DIR}/packages.config COPYONLY)
add_custom_target(${PROJECT_NAME}_nuget-restore COMMAND ${NUGET} restore ${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}.sln)
add_dependencies(${PROJECT_NAME} ${PROJECT_NAME}_nuget-restore)


# build .net standard 1.0 lib
configure_file(ds_logging_cs_standard.csproj.in netstandard/ds_logging_cs_standard.csproj)

add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
	COMMAND msbuild /t:restore ds_logging_cs_standard.csproj
	COMMAND msbuild ds_logging_cs_standard.csproj
	WORKING_DIRECTORY netstandard # need to change working_directory to avoid interference with other build
)

# build nuget
configure_file(Microsoft.DecisionService.Logging.nuspec.in Microsoft.DecisionService.Logging.nuspec)

add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
	COMMAND nuget pack ${CMAKE_CURRENT_BINARY_DIR}/Microsoft.DecisionService.Logging.nuspec
)


add_subdirectory(test)
