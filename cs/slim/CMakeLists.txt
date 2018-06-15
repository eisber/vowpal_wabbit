cmake_minimum_required (VERSION 3.8)
project (cs_slim LANGUAGES CSharp)

enable_language(CSharp)
set(PACKAGE_VERSION 1.0)

set(LIBRARY_NAME "VowpalWabbit.Prediction")

add_library(${PROJECT_NAME} MurMurHash3.cs OptionParser.cs ContextualBanditExample.cs Example.cs Feature.cs Model.cs Exploration.cs)
set_target_properties(${PROJECT_NAME} PROPERTIES OUTPUT_NAME ${LIBRARY_NAME})
set_target_properties(${PROJECT_NAME} PROPERTIES VS_DOTNET_REFERENCES "System")

target_compile_options(${PROJECT_NAME} PRIVATE "/langversion:6" "/platform:anycpu" "/p:NET40")

# build .net standard 1.0 lib
configure_file(vw_slim_cs_standard.csproj.in netstandard/vw_slim_cs_standard.csproj)

add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
	COMMAND msbuild /t:restore vw_slim_cs_standard.csproj
	COMMAND msbuild vw_slim_cs_standard.csproj
	WORKING_DIRECTORY netstandard # need to change working_directory to avoid interference with other build
)

configure_file(vw_slim_cs_standard_test.csproj.in netstandard_test/vw_slim_cs_standard_test.csproj)

add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
	COMMAND msbuild /t:restore vw_slim_cs_standard_test.csproj
	COMMAND msbuild vw_slim_cs_standard_test.csproj
	WORKING_DIRECTORY netstandard_test # need to change working_directory to avoid interference with other build
)

# build nuget
# configure_file(VowpalWabbit.Prediction.nuspec.in VowpalWabbit.Prediction.nuspec)

# 
# add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
#	COMMAND nuget pack ${CMAKE_CURRENT_BINARY_DIR}/VowpalWabbit.Prediction.nuspec
#)
