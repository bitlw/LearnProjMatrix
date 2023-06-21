import numpy as np

def lookAt(pointFrom, pointTo, upVector, zPositive, rightHand):
    zAxis = pointTo - pointFrom
    zAxis /= np.linalg.norm(zAxis)
    yAxis = upVector
    yAxis /= np.linalg.norm(yAxis)

    if rightHand:
        if zPositive:
            yAxis = - yAxis
        else:
            zAxis = - zAxis
    else: # left hand
        if not zPositive:
            zAxis = - zAxis
            yAxis = - yAxis

    xAxis = np.cross(yAxis, zAxis)
    xAxis /= np.linalg.norm(xAxis)
    yAxis = np.cross(zAxis, xAxis)
    yAxis /= np.linalg.norm(yAxis)

    R = np.array([xAxis, yAxis, zAxis])
    t = - R @ pointFrom.reshape(3, 1)

    return R, t

# if you are testing OpenGL with GLM, remember to set viewportThreeJS to False
viewportThreeJS = False
zPositive = False
rightHand = False

windowWidth = 640
windowHeight = 480
viewStartX = 40
viewStartY = 60
viewWidth = 580
viewHeight = 400
fx = 565.5
fy = 516.3
u0 = 328.2
v0 = 238.8
calibrationWidth = 640
calibrationHeight = 480

ratioX = viewWidth / calibrationWidth
ratioY = viewHeight / calibrationHeight
fx = fx * ratioX
fy = fy * ratioY
u0 = u0 * ratioX
v0 = v0 * ratioY

K = np.array([[fx, 0, u0], [0, fy, v0], [0, 0, 1]])
# R = np.array([[0.4608, 0, -0.8875],
#               [0.4045, -0.8901, 0.2100],
#               [-0.7900, -0.4558, -0.4102]])
# t = np.array([[3.5841], [9.0800], [70.0336]])

R, t = lookAt(np.array([40, 50, 45], dtype=np.float32), np.array([2, 8, 5], dtype=np.float32), np.array([0, 0, 1], dtype=np.float32), zPositive, rightHand)

if rightHand:
    if not zPositive:
        K[0, 0] = - K[0, 0] # OpenGL default: z negative, right hand
    else:  
        pass # SFM/SLAM default: z positive, right hand 
else: # left hand
    if zPositive: # I never use, but my college tried it
        K[1, 1] = - K[1, 1]
    else: # I don't know who use it
        K[0, 0] = - K[0, 0]
        K[1, 1] = - K[1, 1]

points_world = [[0, 0, 0], [10, 0, 0], [0, 10, 0], [0, 0, 10], [10, 15, 20], [-10, 15, -20]]
points_uv = []

for pt in points_world:
    P = np.array(pt, dtype=np.float32).reshape(3, 1)
    # here the fomulation is: [u; v; 1] = K * （R * P + t）/ zc
    q = K @ (R @ P + t)
    q /= q[2, 0]
    u = q[0, 0] + viewStartX
    if viewportThreeJS:
        v = q[1, 0] + viewStartY
    else:
        v = q[1, 0] + (windowHeight - viewStartY - viewHeight)
    
    points_uv.append([u, v])

print(points_uv)
