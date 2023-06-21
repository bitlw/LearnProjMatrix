cmake_minimum_required(VERSION 3.0 FATAL_ERROR)

# project name
SET(PROJECT_NAME GLMDemo)
project(${PROJECT_NAME})

# source code folder
aux_source_directory(src DIR_SRCS)
set(CMAKE_CXX_STANDARD 17)

# create exe
add_executable(${PROJECT_NAME} ${DIR_SRCS})
include_directories("D:\\Work\\Code\\Cpp\\vcpkg\\installed\\x64-windows\\include")
target_link_libraries(${PROJECT_NAME} "D:\\Work\\Code\\Cpp\\vcpkg\\installed\\x64-windows\\lib\\glew32.lib")

# # find GLFW
set(glfw3_DIR "D:\\Work\\Code\\Cpp\\vcpkg\\installed\\x64-windows\\share\\glfw3" )
find_package(glfw3 REQUIRED)
target_link_libraries(${PROJECT_NAME} glfw)

find_package(OpenGL REQUIRED)
SET(glm_DIR "D:\\Work\\Code\\Cpp\\vcpkg\\installed\\x64-windows\\share\\glm")
find_package(glm REQUIRED)
include_directories(${OPENGL_INCLUDE_DIRS})
include_directories(${GLM_INCLUDE_DIRS})
target_link_libraries(${PROJECT_NAME}  glm::glm)
target_link_libraries(${PROJECT_NAME} ${OPENGL_LIBRARIES})
