This is the python version of OpenGL with pyglet demo to show how to setup/calculate lookAt/projection matrix.

# Setup
```bash
pip install pyglet
pip install PyOpenGL
pip install pyrr
```

# Usage
Similar to proj_lookat_demo.html/glm_demo, this demo includes a complete test of lookAt and projection matrix. You can setup camera pose and projection parameters in pyglet_demo.py. Calculate the projection matrix and lookAt matrix yourself and verify if all 2d points on the rendering window are on the correct position. Please read [OpenGL_Projection.md](https://github.com/bitlw/LearnProjMatrix/blob/main/doc/OpenGL_Projection.md) and [lookAt.md](https://github.com/bitlw/LearnProjMatrix/blob/main/doc/lookAt.md) for how to calculate projection matrix; or you can look at projection.py for reference. \
pyglet_demo.py contains many comments, I think you can understand which parameters you need to setup. Just a reminder here, when you are using html test you will set viewportThreeJS (in projection.py) to true, but in this demo you need to set it to false because we are using OpenGL to render.

For how to verify your calculation, you can read the "README.md" in glm_demo folder. 

Only the difference I want to mansion: only pyglet_demo.py used normals of vertices (the example I got from pyglet used normals, so I just keep it).

```python
normals = [0, 0, 1] * (len(vertices) // 3)
if zPositive:
    normals = [0, 0, -1] * (len(vertices) // 3)
```
We need make sure the normal should be opposite to our view orientation, which is that dot(normal, to - from) should be negative, otherwise the face will be invisible.

And the other thing is that the start point of mouse is at the bottom left corner, but the start point of OpenGL is at the top left corner. So we need to convert the y coordinate of mouse position. 

***I'm talking about mouse position here, NOT viewport. the viewport of pyglet is the same as OpenGL, they are all different from Threejs *** 

```python
print(f"Mouse position: ({x}, {windowHeight - y})")
```