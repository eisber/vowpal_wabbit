cmake_minimum_required (VERSION 3.5)
project (vw-predict)

set(CMAKE_CXX_STANDARD 11)

# TODO make this optional
find_package(RapidJSON REQUIRED)
include_directories(${RAPIDJSON_INCLUDES})

add_executable(${PROJECT_NAME} main.cc cb_example.cc example.cc features.cc model.cc 
	parser_json.cc)

install(TARGETS ${PROJECT_NAME} RUNTIME DESTINATION bin)
