import numpy as np

viewportThreeJS = True
zPositive = True

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
R = np.array([[0.4608, 0, -0.8875],
              [0.4045, -0.8901, 0.2100],
              [-0.7900, -0.4558, -0.4102]])
t = np.array([[3.5841], [9.0800], [70.0336]])

if not zPositive:
    K[0, 0] = - K[0, 0]

    R = np.array([[-0.7071, 0.7071, 0],
                  [-0.4083, -0.4083, 0.8165],
                  [0.5774, 0.5774, 0.5774]])
    t = np.array([[0.0], [0.0], [-86.603]])

points_world = [[0, 0, 0], [10, 0, 0], [0, 10, 0], [0, 0, 10], [10, 15, 20], [-10, 15, -20]]
points_uv = []

for pt in points_world:
    P = np.array(pt, dtype=np.float32).reshape(3, 1)
    q = K @ (R @ P + t)
    q /= q[2, 0]
    u = q[0, 0] + viewStartX
    if viewportThreeJS:
        v = q[1, 0] + viewStartY
    else:
        v = q[1, 0] + (windowHeight - viewStartY - viewHeight)
    
    points_uv.append([u, v])

print(points_uv)
