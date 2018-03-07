cmake_minimum_required (VERSION 3.8)
project (ds_explore_cs LANGUAGES CSharp)

enable_language(CSharp)
set(PACKAGE_VERSION 1.0)
set(LIBRARY_NAME "Microsoft.DecisionService.Exploration")

add_library(${PROJECT_NAME} ExplorationStrategies.cs Sampling.cs PRG.cs MurMurHash3.cs)
set_target_properties(${PROJECT_NAME} PROPERTIES OUTPUT_NAME ${LIBRARY_NAME})
set_target_properties(${PROJECT_NAME} PROPERTIES VS_DOTNET_REFERENCES "System")

# just in case we need it
target_compile_options(${PROJECT_NAME} PRIVATE "/langversion:6" "/platform:anycpu" "/p:NET40")

add_executable("${PROJECT_NAME}_test" TestDriver.cs)
target_link_libraries("${PROJECT_NAME}_test" ${PROJECT_NAME})

# build .net standard 1.0 lib
configure_file(ds_explore_cs_standard.csproj.in netstandard/ds_explore_cs_standard.csproj)

add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
	COMMAND msbuild /t:restore ds_explore_cs_standard.csproj
	COMMAND msbuild ds_explore_cs_standard.csproj
	WORKING_DIRECTORY netstandard # need to change working_directory to avoid interference with other build
)

# build nuget
configure_file(Microsoft.DecisionService.Exploration.nuspec.in Microsoft.DecisionService.Exploration.nuspec)

add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
	COMMAND nuget pack ${CMAKE_CURRENT_BINARY_DIR}/Microsoft.DecisionService.Exploration.nuspec
)

# run tests
add_test(NAME ds_explore_cs_test1
	COMMAND $<TARGET_FILE:${PROJECT_NAME}_test> ../tests/test1-input.txt ../tests/test1-expected.txt
	WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR})
