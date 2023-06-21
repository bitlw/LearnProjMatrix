#include <iostream>
#include <GL/glew.h>
#include <GLFW/glfw3.h>
#include <glm/glm.hpp>
#include <glm/gtc/type_ptr.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <vector>

const char* vertexShaderSource =
        "#version 330 core\n"
        "layout (location = 0) in vec3 aPos;\n"
        "uniform mat4 view;\n"
        "uniform mat4 projection;\n"
        "void main()\n"
        "{\n"
        "    gl_Position = projection * view * vec4(aPos, 1.0);\n"
        "}\n";

const char* fragmentShaderSource =
        "#version 330 core\n"
        "uniform vec4 color;\n"
        "out vec4 FragColor;\n"
        "void main()\n"
        "{\n"
        "    FragColor = color;\n"
        "}\n";

GLFWwindow* Init(GLuint width, GLuint height)
{
    // Initialize GLFW
    if (!glfwInit())
    {
        std::cerr << "Failed to initialize GLFW" << std::endl;
        return NULL;
    }

    // Create a window
    GLFWwindow* window = glfwCreateWindow(width, height, "OpenGL Window", NULL, NULL);
    if (!window)
    {
        std::cerr << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return NULL;
    }

    // Make the window's context current
    glfwMakeContextCurrent(window);

    // Initialize GLEW
    if (glewInit() != GLEW_OK)
    {
        std::cerr << "Failed to initialize GLEW" << std::endl;
        glfwTerminate();
        return NULL;
    }

    return window;
}

struct VertexProperty
{
    GLuint VBO;
    GLuint VAO;
    GLuint shaderProgram;
};

void cursor_position_callback(GLFWwindow* window, double xpos, double ypos)
{
    std::cout << "Mouse position: " << xpos << ", " << ypos << std::endl;
}

void PrintMat4(const glm::mat4& m)
{
    for (int i = 0; i < 4; ++i)
    {
        for (int j = 0; j < 4; ++j)
        {
            std::cout << m[i][j] << " ";
        }
        std::cout << std::endl;
    }
    std::cout << std::endl;
}

void PrintVec4(const glm::vec4& v)
{
    for (int i = 0; i < 4; ++i)
    {
        std::cout << v[i] << " ";
    }
    std::cout << std::endl;
}

void CreateLine(const std::vector<GLfloat>& v1, const std::vector<GLfloat>& v2, const glm::vec4& color, const glm::mat4& view, const glm::mat4& projection,
    std::vector<VertexProperty>& vertexProperties)
{
    // Create a vertex shader
    GLuint vertexShader = glCreateShader(GL_VERTEX_SHADER);
    glShaderSource(vertexShader, 1, &vertexShaderSource, NULL);
    glCompileShader(vertexShader);

    // Create a fragment shader
    GLuint fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
    glShaderSource(fragmentShader, 1, &fragmentShaderSource, NULL);
    glCompileShader(fragmentShader);

    // Create a shader program
    GLuint shaderProgram = glCreateProgram();
    glAttachShader(shaderProgram, vertexShader);
    glAttachShader(shaderProgram, fragmentShader);
    glLinkProgram(shaderProgram);

    glDeleteShader(vertexShader);
    glDeleteShader(fragmentShader);

    std::vector<GLfloat> v(v1);
    v.insert(v.end(), v2.begin(), v2.end());

    GLuint VAO, VBO;
    // Create a vertex buffer object (VBO) and vertex array object (VAO)
    glGenVertexArrays(1, &VAO);
    glGenBuffers(1, &VBO);

    // Bind the VAO and VBO
    glBindVertexArray(VAO);
    glBindBuffer(GL_ARRAY_BUFFER, VBO);

    // Copy the vertex data to the VBO
    glBufferData(GL_ARRAY_BUFFER, v.size() * sizeof(GLfloat), v.data(), GL_STATIC_DRAW);

    // Set the vertex attribute pointers
    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(GLfloat), (void*)0);
    glEnableVertexAttribArray(0);

    // Unbind the VBO and VAO
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    glBindVertexArray(0);

    // Set the shader program and uniforms
    glm::mat4 model = glm::mat4(1.0f);

    glUseProgram(shaderProgram);
    glUniformMatrix4fv(glGetUniformLocation(shaderProgram, "view"), 1, GL_FALSE, glm::value_ptr(view));
    glUniformMatrix4fv(glGetUniformLocation(shaderProgram, "projection"), 1, GL_FALSE, glm::value_ptr(projection));
    glUniform4fv(glGetUniformLocation(shaderProgram, "color"), 1, glm::value_ptr(color));

    VertexProperty vertexProperty;
    vertexProperty.VBO = VBO;
    vertexProperty.VAO = VAO;
    vertexProperty.shaderProgram = shaderProgram;
    vertexProperties.push_back(vertexProperty);
}

/*--------------------------------------------------------

You don't need to look at the code above.

-----------------------------------------------------------*/

glm::mat4 GetProjectionMatrixFromClip(float l, float r, float b, float t, float near, float far, bool zPositive)
{
    if (zPositive) 
    {
        float A =  - (far + near) / (near - far);
        float B = 2 * (far * near) / (near - far);
        glm::mat4 T = glm::mat4(
                        2*near/(r - l), 0, - (r + l) / (r - l), 0,
                        0, 2*near/(t - b), - (t + b) / (t - b), 0,
                        0, 0, A, B,
                        0, 0, 1, 0);
        T = glm::transpose(T); // let's transpose it back to column major
        return T;
    }
    else 
    {
        float A =  (far + near) / (near - far);
        float B = 2 * (far * near) / (near - far);
        glm::mat4 T = glm::mat4(
                        2*near/(r - l), 0, (r + l) / (r - l), 0,
                        0, 2*near/(t - b), (t + b) / (t - b), 0,
                        0, 0, A, B,
                        0, 0, -1, 0);
        T = glm::transpose(T); // let's transpose it back to column major
        return T;
    }
}

glm::mat4 GetProjectionMatrix(float fx, float fy, float u0, float v0, float near, float far, float viewWidth , float viewHeight,  bool zPositive, bool rightHand) 
{
    float l = - u0 / fx * near;
    float r = (viewWidth - u0) / fx * near;
    float t = - v0 / fy * near;
    float b = (viewHeight - v0) / fy * near;
    
    if (rightHand) 
    {
        if (zPositive) 
        { // right hand system and z positive, usually used in 3d reconstruction/SFM/SLAM
            return GetProjectionMatrixFromClip(l, r, b, t, near, far, zPositive);
        }
        else 
        { // right hand system, z negative, usually used in OpenGL
            return GetProjectionMatrixFromClip(l, r, -b, -t, near, far, zPositive);
        }
    }
    else 
    {
        if (zPositive) 
        { // left hand system, z positive
            return GetProjectionMatrixFromClip(l, r, -b, -t, near, far, zPositive);
        }
        else 
        { // left hand system, z negative
            return GetProjectionMatrixFromClip(l, r, b, t, near, far, zPositive);
        }
    }
} 

glm::mat4 LookAt(const glm::vec3& from, const glm::vec3& to, const glm::vec3& up, bool zPositive, bool rightHand) 
{
    // we stands on "from" and look at "to"
    // if zPositive = true, the destination is in the area of the positive half-axis of z, otherwise it is negative
    // if rightHand = true, the coordinate system is right-handed, otherwise it is left-handed
    glm::vec3 zAxis = glm::normalize(to - from);
    glm::vec3 yAxis = glm::normalize(up);
    glm::vec3 xAxis;

    if (rightHand) 
    {
        if (zPositive) 
        { // rightHand = true and zPositive = true, usually used in 3d reconstruction
            yAxis = -yAxis;
        }
        else 
        { // rightHand = true and zPositive = false, usually used in OpenGL
            zAxis = -zAxis;
        }
    }
    else 
    { // left-hand
        if (!zPositive) 
        {
            zAxis = -zAxis;
            yAxis = -yAxis;
        }
    }

    xAxis = glm::normalize(glm::cross(yAxis, zAxis));
    yAxis = glm::normalize(glm::cross(zAxis, xAxis));

    // glm matrix is column major, so if you set like this, you will notice that the matrix is transposed when you use it.
    glm::mat3 R = glm::mat3(
            xAxis.x, xAxis.y, xAxis.z,
            yAxis.x, yAxis.y, yAxis.z,
            zAxis.x, zAxis.y, zAxis.z
            );
    R = glm::transpose(R); // Let's transpose it back
    glm::vec3 t = -R * from; // t = -R * from;

    glm::mat4 m = glm::mat4(
            xAxis.x, xAxis.y, xAxis.z, t.x,
            yAxis.x, yAxis.y, yAxis.z, t.y,
            zAxis.x, zAxis.y, zAxis.z, t.z,
            0, 0, 0, 1);
    m = glm::transpose(m); // Let's transpose it back
    return m;
}

int main()
{
    // -------- 你可以修改下面的值来进行测试 --------- //
    // -------- You can modify the data below for testing --------- //

    // 设置为 true 则观察方向为 z 的正半轴，设置为 false 则观察方向为 z 的负半轴
    // Set to true to observe the positive half-axis of z, and set to false to observe the negative half-axis of z
    bool zPositive = false; 

    // 设置为 true 则为右手坐标系，设置为 false 则为左手坐标系
    // Set to true for right-handed coordinate system, and set to false for left-handed coordinate system
    bool rightHand = false; 

    // 设置整个窗口大小
    // setup window size of the entire UI
    GLuint windowWidth = 640;
    GLuint windowHeight = 480;

    // 用来渲染图形的窗口大小，它不同于窗口大小
    // setup viewport of the rendering area, which is different from the window size
    GLuint viewStartX = 40; // viewStartX and viewStartY are actually set to 0 in z_positive.html and z_negtive.html
    GLuint viewStartY = 60;
    GLuint viewWidth = 580; //  viewStartX + viewWidth <= windowWidth; So does viewHeight 
    GLuint viewHeight = 400;

    // 设置摄像机内参矩阵 K
    // zPositive = true, K = [[fx, 0, u0],  [0, fy, v0], [0, 0, 1]
    // zPositive = false, K = [[-fx, 0, u0], [0, fy, v0], [0, 0, 1]
    GLfloat fx = 565.5f;
    GLfloat fy = 516.3f;
    GLfloat u0 = 328.2f; 
    GLfloat v0 = 238.8f;
    GLfloat calibrationWidth = 640.0f; // 摄像机在标定时候的分辨率
    GLfloat calibrationHeight = 480.0f; // the resolution of the image which is used for camera calibration

    // setup camera position and direction
    glm::vec3 from = glm::vec3(40, 50, 45); // 摄像机位置 (camera position)
    glm::vec3 to = glm::vec3(2, 8, 5); // 摄像机观察方向 (camera direction)
    glm::vec3 up = glm::vec3(0.0f, 0.0f, 1.0f); // 摄像机上方向 (camera up direction)

    // 设置远近裁剪面
    // setup near and far clipping plane
    GLfloat near = 0.5f;
    GLfloat far = 1000.0f;
    // -------- 你可以修改上面的值来进行测试 --------- //
    // -------- You can modify the data above for testing --------- //
    
    GLFWwindow* window = Init(windowWidth, windowHeight);
    if (window == NULL)
    {
        return -1;
    }

    // K 的参数往往需要根据渲染窗口大小进行缩放，注意这里用的是 viewWidth 而不是 windowWidth
    // parameters of often need to be scaled according to the size of the rendering window. 
    // Note that viewWidth is used here rather than windowWidth
    GLfloat ratioX = viewWidth / calibrationWidth;
    GLfloat ratioY = viewHeight / calibrationHeight;
    fx = fx * ratioX;
    fy = fy * ratioY;
    u0 = u0 * ratioX;
    v0 = v0 * ratioY;

    // 这里包含了4中情况，刚开始的时候建议看 z_positive.html 和 z_negative.html
    // There are 4 cases here, it is recommended to look at z_positive.html and z_negative.html at the beginning

    // I don't like to use glm::perspective because it doesn't support the case which u0, v0 of K is not equal to the center of the image.
    // And as I said in document, I like to calculate all matrix by myself to avoid the misunderstanding of the function provided by others.
    //glm::mat4 projection = glm::perspective(glm::radians(45.0f), 640.0f / 480.0f, 0.1f, 1000.0f);
    glm::mat4 projection = GetProjectionMatrix(fx, fy, u0, v0, near, far, GLfloat(viewWidth), GLfloat(viewHeight), zPositive, rightHand); 
    PrintMat4(projection);

    // 旋转和平移矩阵 (rotation and translation matrix) [R t; 0 0 0 1]
    // q = R * p + t, p 是空间中的一点在世界坐标系中的坐标，q 是该点在摄像机坐标系中的坐标
    // 这里 R/t 的定义仅仅是我习惯的定义
    // q = R * p + t, p is the coordinate of a point in space in the world coordinate system, 
    // and q is the coordinate of the point in the camera coordinate system

    // Based on my test, I can see glm::lookAt is one of my case (zPositive = false, rightHand = true).
    // In this demo, I still use the LookAt created by myself.
    // glm::mat4 view = glm::lookAt(glm::vec3(40, 50, 45), glm::vec3(2, 8, 5), glm::vec3(0.0f, 0.0f, 1.0f));
    glm::mat4 transformMatrix = LookAt(from, to, up, zPositive, rightHand);
    PrintMat4(transformMatrix);

    std::vector<VertexProperty> vertexProperties;
    glm::vec4 red(1.0f, 0.0f, 0.0f, 1.0f);
    glm::vec4 green(0.0f, 1.0f, 0.0f, 1.0f);
    glm::vec4 blue(0.0f, 0.0f, 1.0f, 1.0f);
    glm::vec4 white(1.0f, 1.0f, 1.0f, 1.0f);
    glm::vec4 pink(1.0f, 0.0f, 1.0f, 1.0f);

    std::vector<GLfloat> O = {0.0f, 0.0f, 0.0f};
    std::vector<GLfloat> X = {10.0f, 0.0f, 0.0f};
    std::vector<GLfloat> Y = {0.0f, 10.0f, 0.0f};
    std::vector<GLfloat> Z = {0.0f, 0.0f, 10.0f};
    std::vector<GLfloat> P = {10.0f, 15.0f, 20.0f};
    std::vector<GLfloat> Q = {-10.0f, 15.0f, -20.0f};
    CreateLine(O, X, red, transformMatrix, projection, vertexProperties);
    CreateLine(O, Y, green, transformMatrix, projection, vertexProperties);
    CreateLine(O, Z, blue, transformMatrix, projection, vertexProperties);
    CreateLine(O, P, white, transformMatrix, projection, vertexProperties);
    CreateLine(O, Q, pink, transformMatrix, projection, vertexProperties);

    glViewport(viewStartX, viewStartY, viewWidth, viewHeight); // starts from left-bottom corner which is different from Threejs
    // you don't need to care about the code below, it is just for rendering

    // Loop until the user closes the window
    glfwSetCursorPosCallback(window, cursor_position_callback);
    while (!glfwWindowShouldClose(window))
    {
        // Clear the screen
        glClear(GL_COLOR_BUFFER_BIT);
        for (auto& property : vertexProperties) 
        {
            glUseProgram(property.shaderProgram);
            glBindVertexArray(property.VAO);
            glDrawArrays(GL_LINES, 0, 2);
            glBindVertexArray(0);
        }

        // Swap buffers and poll for events
        glfwSwapBuffers(window);
        glfwPollEvents();
    }

    // Clean up
    for (auto& property : vertexProperties) 
    {
        glDeleteProgram(property.shaderProgram);
        glDeleteBuffers(1, &property.VBO);
        glDeleteVertexArrays(1, &property.VAO);
    }
    glfwTerminate();
    return 0;
}