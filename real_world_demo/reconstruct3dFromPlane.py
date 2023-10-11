import numpy as np
import json
import os

def reconstructFromPlane(fx, fy, u0, v0, n_camera, d_camera, R, t, u, v):
    u_ = (u - u0) / fx # == xc/ zc
    v_ = (v - v0) / fy # == yc/ zc
    zc = -d_camera / (n_camera[0] * u_ + n_camera[1] * v_ + n_camera[2])
    xc = u_ * zc
    yc = v_ * zc

    point_camera = np.array([xc, yc, zc])
    point_world = np.dot(R.T, point_camera) - np.dot(R.T, t)
    return point_world

def buildAxisPoints(O, X, Y):
    x = X - O
    y_ = Y - O
    z = np.cross(x, y_)
    y = np.cross(z, x)

    length = np.linalg.norm(x)

    return O, X, O + (y / np.linalg.norm(y)) * length, O + (z / np.linalg.norm(z)) * length


dir_path = os.path.dirname(os.path.realpath(__file__))
with open(os.path.join(dir_path, 'textures', 'colmap_rst.json'), 'r') as f:
    raw_info = json.load(f)

planePara = np.array(raw_info['PlanePara'][0])
n_world = planePara[0:3]
d_world = planePara[3]

pose = raw_info['Pose'][0]
R = np.array([pose[0:3], pose[3:6], pose[6:9]])
t = np.array(pose[9:12])

n_camera = np.dot(R, n_world) # R * n_world
d_camera = d_world - np.dot(n_camera, t) # d_world - n_camera.t() * t

fx = raw_info['K']['fx']
fy = raw_info['K']['fy']
u0 = raw_info['K']['u0']
v0 = raw_info['K']['v0']

A_world = reconstructFromPlane(fx, fy, u0, v0, n_camera, d_camera, R, t, 1072, 646)
B_world = reconstructFromPlane(fx, fy, u0, v0, n_camera, d_camera, R, t, 1130, 630)
C_world = reconstructFromPlane(fx, fy, u0, v0, n_camera, d_camera, R, t, 1198, 644)
D_world = reconstructFromPlane(fx, fy, u0, v0, n_camera, d_camera, R, t, 1150, 687)
E_world = reconstructFromPlane(fx, fy, u0, v0, n_camera, d_camera, R, t, 1086, 670)

print('A_world: ', A_world)
print('B_world: ', B_world)
print('C_world: ', C_world)
print('D_world: ', D_world)
print('E_world: ', E_world)