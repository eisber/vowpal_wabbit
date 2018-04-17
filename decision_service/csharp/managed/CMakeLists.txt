cmake_minimum_required (VERSION 3.8)
project (ds_cs_managed LANGUAGES CSharp)

enable_language(CSharp)

set(CS_SOURCE
	"${ds_cs_SWIG_SOURCE_DIR}/BagExplorer.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/DecisionServiceClient.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/DecisionServiceConfiguration.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/DecisionServiceLogger.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/DecisionServiceLogLevel.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/DecisionServicePredictors.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/decision_service.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/decision_servicePINVOKE.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/EpsilonGreedyExplorer.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/IExplorer.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/RankResponse.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/SoftmaxExplorer.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/ListInt.cs"
	"${ds_cs_SWIG_SOURCE_DIR}/ListFloat.cs")

set_source_files_properties(${CS_SOURCE} PROPERTIES GENERATED TRUE)

add_library(${PROJECT_NAME} ${CS_SOURCE})

# set_property(TARGET ${PROJECT_NAME} PROPERTY VS_WINRT_COMPONENT TRUE)
# set_property(TARGET Example PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.6.1")
# set_property(TARGET Example PROPERTY WIN32_EXECUTABLE TRUE)

# TODO: just copy & dependencies for the pinvoke fun
# set_property(TARGET ${PROJECT_NAME} PROPERTY VS_DOTNET_REFERENCES
#	${ds_cs_dll})
