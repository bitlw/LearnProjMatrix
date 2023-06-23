import pyglet
from pyglet.gl import *
import pyrr
import numpy as np

def getProjectionMatrix(fx, fy, u0, v0, near, far, zPositive, rightHand):
    l = - u0 / fx * near
    r = (viewWidth - u0) / fx * near
    t = - v0 / fy * near
    b = (viewHeight - v0) / fy * near
    
    if rightHand:
        if zPositive: #right hand system and z positive, usually used in 3d reconstruction/SFM/SLAM
            return getProjectionMatrixFromClip(l, r, b, t, near, far, zPositive)
        else : #right hand system, z negative, usually used in OpenGL
            return getProjectionMatrixFromClip(l, r, -b, -t, near, far, zPositive)
    else :
        if (zPositive) : #left hand system, z positive
            return getProjectionMatrixFromClip(l, r, -b, -t, near, far, zPositive)
        else : #left hand system, z negative
            return getProjectionMatrixFromClip(l, r, b, t, near, far, zPositive)

def getProjectionMatrixFromClip (l, r, b, t, near, far, zPositive) :
    projectionMatrix = []
    if (zPositive) :
        A =  - (far + near) / (near - far)
        B = 2 * (far * near) / (near - far)
        projectionMatrix = [[2*near/(r - l), 0, - (r + l) / (r - l), 0],
                            [0, 2*near/(t - b), - (t + b) / (t - b), 0],
                            [0, 0, A, B],
                            [0, 0, 1, 0]]
    else :
        A =  (far + near) / (near - far)
        B = 2 * (far * near) / (near - far)
        projectionMatrix = [[2*near/(r - l), 0, (r + l) / (r - l), 0],
                            [0, 2*near/(t - b), (t + b) / (t - b), 0],
                            [0, 0, A, B],
                            [0, 0, -1, 0]]
    return np.array(projectionMatrix, dtype=np.float32)


def lookAt(cam_from, to, up, zPositive, rightHand) :
    #we stands on "from" and look at "to"
    #if zPositive = true, the destination is in the area of the positive half-axis of z, otherwise it is negative
    #if rightHand = true, the coordinate system is right-handed, otherwise it is left-handed
    zAxis = to - cam_from
    zAxis /= np.linalg.norm(zAxis)
    yAxis = up / np.linalg.norm(up)

    if rightHand:
        if zPositive: #rightHand = true and zPositive = true, usually used in 3d reconstruction
            yAxis = -yAxis
        else : #rightHand = true and zPositive = false, usually used in OpenGL
            zAxis = -zAxis
    else : #left-hand
        if not zPositive:
            zAxis = -zAxis
            yAxis = -yAxis

    xAxis = np.cross(yAxis, zAxis)
    xAxis /= np.linalg.norm(xAxis)
    yAxis = np.cross(zAxis, xAxis) 
    yAxis /= np.linalg.norm(yAxis)

    R = np.array([xAxis, yAxis, zAxis], dtype=np.float32)
    print("R:", R)

    t = - R @ cam_from.reshape(3, 1)
    print("t:", t)
    
    m = np.identity(4, dtype=np.float32)
    m[0:3, 0:3] = R
    m[0:3, 3] = t.reshape(3)
    return m

#-------- 你可以修改下面的值来进行测试 --------- 
#-------- You can modify the data below for testing --------- 

#设置为 true 则观察方向为 z 的正半轴，设置为 false 则观察方向为 z 的负半轴
#Set to true to observe the positive half-axis of z, and set to false to observe the negative half-axis of z
zPositive = False 

#设置为 true 则为右手坐标系，设置为 false 则为左手坐标系
#Set to true for right-handed coordinate system, and set to false for left-handed coordinate system
rightHand = False 

#设置整个窗口大小
#setup window size of the entire UI
windowWidth = 640
windowHeight = 480

#用来渲染图形的窗口大小，它不同于窗口大小
#setup viewport of the rendering area, which is different from the window size
viewStartX = 40 #viewStartX and viewStartY are actually set to 0 in z_positive.html and z_negative.html
viewStartY = 60
viewWidth = 580 # viewStartX + viewWidth <= windowWidth So does viewHeight 
viewHeight = 400

#设置摄像机内参矩阵 K
#zPositive = true, K = [[fx, 0, u0],  [0, fy, v0], [0, 0, 1]
#zPositive = false, K = [[-fx, 0, u0], [0, fy, v0], [0, 0, 1]
fx = 565.5
fy = 516.3
u0 = 328.2 
v0 = 238.8
calibrationWidth = 640 #摄像机在标定时候的分辨率
calibrationHeight = 480 #the resolution of the image which is used for camera calibration

#setup camera position and direction
cam_from = np.array([40., 50., 45.], dtype=np.float32) #摄像机位置 (camera position)
to = np.array([2., 8., 5.], dtype=np.float32) #摄像机观察方向 (camera direction)
up = np.array([0., 0., 1.], dtype=np.float32) #摄像机上方向 (camera up direction)

#设置远近裁剪面
#setup near and far clipping plane
near = 0.5
far = 1000
#-------- 你可以修改上面的值来进行测试 --------- #
#-------- You can modify the data above for testing --------- #

# K 的参数往往需要根据渲染窗口大小进行缩放，注意这里用的是 viewWidth 而不是 windowWidth
# parameters of often need to be scaled according to the size of the rendering window. 
# Note that viewWidth is used here rather than windowWidth
ratioX = viewWidth / calibrationWidth
ratioY = viewHeight / calibrationHeight
fx = fx * ratioX
fy = fy * ratioY
u0 = u0 * ratioX
v0 = v0 * ratioY

#这里包含了4中情况，刚开始的时候建议看 z_positive.html 和 z_negative.html
#There are 4 cases here, it is recommended to look at z_positive.html and z_negative.html at the beginning
projectionMatrix = getProjectionMatrix(fx, fy, u0, v0, near, far, zPositive, rightHand)
print("projectionMatrix:", projectionMatrix)

#旋转和平移矩阵 (rotation and translation matrix) [R t 0 0 0 1]
#q = R * p + t, p 是空间中的一点在世界坐标系中的坐标，q 是该点在摄像机坐标系中的坐标
#这里 R/t 的定义仅仅是我习惯的定义
#q = R * p + t, p is the coordinate of a point in space in the world coordinate system, 
#and q is the coordinate of the point in the camera coordinate system
transformMatrix = lookAt(cam_from, to, up, zPositive, rightHand) 
print("transformMatrix:", transformMatrix)

# You can also use the following code to get the transformMatrix,
# but this function is only used for right-hand, z negative coordinate system
# view_matrix = pyrr.matrix44.create_look_at(
# pyrr.vector3.create(40, 50, 45), #camera position
# pyrr.vector3.create(2, 8, 5), #camera target
# pyrr.vector3.create(0, 0, 1)  #camera up vector
# )

window = pyglet.window.Window(width=windowWidth, height=windowHeight)

@window.event
def on_draw():
    window.clear()
    batch.draw()

# this function is very important, I can see nothing if I remove it
@window.event
def on_resize(width, height):
    # I suggest not resize your window, otherwise you may confuse why the 2d coordinate is not as your expectation.
    # but in your real project, you should call glViewport here
    return pyglet.event.EVENT_HANDLED

def setup():
    glClearColor(0, 0, 0, 1)
    glEnable(GL_DEPTH_TEST)
    glEnable(GL_CULL_FACE)

@window.event
def on_mouse_motion(x, y, dx, dy):
    # The mouse position starts from the lower left corner of the window, and the y axis is reversed
    print(f"Mouse position: ({x}, {windowHeight - y})")
    
def create_torus(shader, batch):
    # Create the vertex and normal arrays.
    O = [0, 0, 0]
    X = [10, 0, 0]
    Y = [0, 10, 0]
    Z = [0, 0, 10]
    P = [10, 15, 20]
    Q = [-10, 15, -20]

    red = [1, 0, 0, 1]
    green = [0, 1, 0, 1]
    blue = [0, 0, 1, 1]
    white = [1, 1, 1, 1]
    pink = [1, 0, 1, 1]

    vertices = []
    vertices.extend(O)
    vertices.extend(X)
    vertices.extend(O)
    vertices.extend(Y)
    vertices.extend(O)
    vertices.extend(Z)
    vertices.extend(O)
    vertices.extend(P)
    vertices.extend(O)
    vertices.extend(Q)

    normals = [0, 0, 1] * (len(vertices) // 3)
    if zPositive:
        normals = [0, 0, -1] * (len(vertices) // 3)
    
    # indices is from 0 to len of vertices
    indices = [i for i in range(len(vertices))]

    colors = []
    colors.extend(red)
    colors.extend(red) # OX
    colors.extend(green)
    colors.extend(green) # OY
    colors.extend(blue)
    colors.extend(blue) # OZ
    colors.extend(white)
    colors.extend(white) # OP
    colors.extend(pink)
    colors.extend(pink) # OQ

    # Create a Material and Group for the Model
    diffuse = [0.5, 0.0, 0.3, 1.0]
    ambient = [0.5, 0.0, 0.3, 1.0]
    specular = [1.0, 1.0, 1.0, 1.0]
    emission = [0.0, 0.0, 0.0, 1.0]
    shininess = 50

    material = pyglet.model.Material("custom", diffuse, ambient, specular, emission, shininess)
    group = pyglet.model.MaterialGroup(material=material, program=shader)

    vertex_list = shader.vertex_list_indexed(len(vertices) // 3, GL_LINES, indices, batch, group,
                                             position=('f', vertices),
                                             normals=('f', normals),
                                             colors=('f', colors))

    return pyglet.model.Model([vertex_list], [group], batch)


setup()
batch = pyglet.graphics.Batch()
shader = pyglet.model.get_default_shader()
torus_model = create_torus(shader, batch)

window.view = transformMatrix.T.flatten() # column major, I need to transpose it
window.projection = projectionMatrix.T.flatten() # column major, I need to transpose it
window.viewport = (viewStartX, viewStartY, viewWidth, viewHeight) # similar to OpenGL glViewport (different from Threejs), the start point is the lower left corner of the window

pyglet.app.run()