using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;

public class UpdateCamera : MonoBehaviour
{
    private Camera cam;
    private Vector3 O_, X_, Y_, Z_, P_, Q_;
    private Matrix4x4 K_OGL_;

    void PrintPointInfo(Vector2 v, string name)
    {
        print(name + " = (" + v.x + ", " + v.y + ")");
    }

    void CreateDefaultPoints()
    {
        O_ = new Vector3(0.0f, 0.0f, 0.0f);
        X_ = new Vector3(10.0f, 0.0f, 0.0f);
        Y_ = new Vector3(0.0f, 10.0f, 0.0f);
        Z_ = new Vector3(0.0f, 0.0f, 10.0f);
        P_ = new Vector3(10.0f, 15.0f, 20.0f);
        Q_ = new Vector3(-10.0f, 15.0f, -20.0f);
    }

    void CreateLine(Vector3 start, Vector3 end, Color color, string name, float width = 0.2f)
    {
        Shader shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;

        GameObject line = new GameObject("Line");
        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.material = material;
        lineRenderer.name = name;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    Vector3 TransformPoint(Matrix4x4 T, Vector3 p)
    {
        Vector4 q = new Vector4(p.x, p.y, p.z, 1.0f);
        q = T * q;
        return new Vector3(q.x/q.w, q.y/q.w, q.z/q.w); // actually q.w should be 1
    }

    Vector2 ProjectPointTo2d(Matrix4x4 K, Matrix4x4 T, Vector3 p)
    {
        Vector3 q = TransformPoint(T, p);
        Vector4 q_ = new Vector4(q.x, q.y, q.z, 1.0f);
        q_ = K * q_;
        return new Vector2(q_.x / q_.z, q_.y / q_.z);
    }

    void PrintProjectionInfo(Matrix4x4 K_OGL, Matrix4x4 T_COGL_WL)
    {
        PrintPointInfo(ProjectPointTo2d(K_OGL, T_COGL_WL, O_), "O");
        PrintPointInfo(ProjectPointTo2d(K_OGL, T_COGL_WL, X_), "X");
        PrintPointInfo(ProjectPointTo2d(K_OGL, T_COGL_WL, Y_), "Y");
        PrintPointInfo(ProjectPointTo2d(K_OGL, T_COGL_WL, Z_), "Z");
        PrintPointInfo(ProjectPointTo2d(K_OGL, T_COGL_WL, P_), "P");
        PrintPointInfo(ProjectPointTo2d(K_OGL, T_COGL_WL, Q_), "Q");
    }

    void PrintProjectionInfoSFM(Matrix4x4 K_OGL, Matrix4x4 T_CSFM_WR)
    {
        Matrix4x4 K_SFM = K_OGL;
        K_SFM[0, 0] = - K_SFM[0, 0];
        PrintPointInfo(ProjectPointTo2d(K_SFM, T_CSFM_WR, O_), "O");
        PrintPointInfo(ProjectPointTo2d(K_SFM, T_CSFM_WR, X_), "X");
        PrintPointInfo(ProjectPointTo2d(K_SFM, T_CSFM_WR, Y_), "Y");
        PrintPointInfo(ProjectPointTo2d(K_SFM, T_CSFM_WR, Z_), "Z");
        PrintPointInfo(ProjectPointTo2d(K_SFM, T_CSFM_WR, P_), "P");
        PrintPointInfo(ProjectPointTo2d(K_SFM, T_CSFM_WR, Q_), "Q");
    }

    void CreateAxis(Matrix4x4 T_WL_WR)
    {
        Vector3 O = TransformPoint(T_WL_WR, O_);
        Vector3 X = TransformPoint(T_WL_WR, X_);
        Vector3 Y = TransformPoint(T_WL_WR, Y_);
        Vector3 Z = TransformPoint(T_WL_WR, Z_);
        Vector3 P = TransformPoint(T_WL_WR, P_);
        Vector3 Q = TransformPoint(T_WL_WR, Q_);
        CreateLine(O, X, Color.red, "Axis_X");
        CreateLine(O, Y, Color.green, "Axis_Y");
        CreateLine(O, Z, Color.blue, "Axis_Z");
        CreateLine(O, P, Color.white, "Line_OP");
        CreateLine(O, Q, Color.cyan, "Line_OQ");
    }
    void SetProjection()
    {
        float fx = 1000.0f, fy = 1000.0f, u0 = 645.0f, v0 = 363.0f;
        float w = 1280.0f, h = 720.0f;
        float ratio_w = Screen.width / w;
        float ratio_h = Screen.height / h;
        fx *= ratio_w;
        fy *= ratio_h;
        u0 *= ratio_w;
        v0 *= ratio_h;

        float left, right, bottom, top;
        left = -u0 * cam.nearClipPlane / fx;
        right = (Screen.width - u0) * cam.nearClipPlane / fx;
        bottom = -(Screen.height - v0) * cam.nearClipPlane / fy;
        top = v0 * cam.nearClipPlane / fy;
        Matrix4x4 proj = Matrix4x4.Frustum(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
        cam.projectionMatrix = proj;

        // this is K for OpenGL default setting, so the first element is -fx.
        K_OGL_ = new Matrix4x4(
            new Vector4(-fx, 0.0f, 0.0f, 0.0f),
            new Vector4(0.0f, fy, 0.0f, 0.0f),
            new Vector4(u0, v0, 1.0f, 0.0f),
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
    }

    void SetCameraPoseBy_COGL_WL(Matrix4x4 T_COGL_WL)
    {
        cam.worldToCameraMatrix = T_COGL_WL;
        PrintProjectionInfo(K_OGL_, T_COGL_WL);
    }

    void SetCameraPoseBy_WL_CL(Matrix4x4 T_WL_CL, bool usingTransform)
    {
        if (usingTransform)
        {
            cam.transform.rotation = T_WL_CL.rotation;
            cam.transform.position = T_WL_CL.GetPosition();
        }
        else
        {
            Matrix4x4 T_COGL_CL = new Matrix4x4(
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            );
            Matrix4x4 T_COGL_WL = T_COGL_CL * T_WL_CL.inverse;
            cam.worldToCameraMatrix = T_COGL_WL;
        }

        PrintProjectionInfo(K_OGL_, cam.worldToCameraMatrix);
    }

    void SetCameraPoseByLookAt(Vector3 from, Vector3 to, Vector3 up, bool usingTransform)
    {
        Matrix4x4 T_WL_CL = Matrix4x4.LookAt(from, to, up);
        SetCameraPoseBy_WL_CL(T_WL_CL, usingTransform);
    }

    void CL_xUp_yLeft_zForward(bool usingLookAt, bool usingTransform)
    {
        // Create point directly, which should be under left hand system defined by Unity
        CreateAxis(Matrix4x4.identity); 

        // left hand camera: x-up, y-left, z-forward; CL means camera under left hand system.
        // rendering expectation: y(green)-right, z(blue)-down, x(red)-invisible
        Matrix4x4 T_WL_CL = new Matrix4x4(
                new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(-1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(80.0f, 0.0f, 0.0f, 1.0f)
            );
        
        if (usingLookAt)
        {
            Vector3 from = new Vector3(80.0f, 0.0f, 0.0f);
            Vector3 to = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 up = new Vector3(0.0f, 0.0f, -1.0f);
            // the return from lookat should be T_WL_CL above.
            SetCameraPoseByLookAt(from, to, up, usingTransform);
        }
        else
        {
            SetCameraPoseBy_WL_CL(T_WL_CL, usingTransform);
        }
    }

    void CL_xDown_yForward_zLeft(bool usingLookAt, bool usingTransform)
    {
        // Create point directly, which should be under left hand system defined by Unity
        CreateAxis(Matrix4x4.identity); 

        // left hand camera: x-down, y-forward, z-left; CL means camera under left hand system.
        // rendering expectation: x(red)-down, y(green)-left, z(blue)-invisible
        Matrix4x4 T_WL_CL = new Matrix4x4(
                new Vector4(0.0f, -1.0f, 0.0f, 0.0f),
                new Vector4(-1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 80.0f, 1.0f)
            );
        if (usingLookAt)
        {
            Vector3 from = new Vector3(0.0f, 0.0f, 80.0f);
            Vector3 to = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 up = new Vector3(-1.0f, 0.0f, 0.0f);
            // the return from lookat should be T_WL_CL above.
            SetCameraPoseByLookAt(from, to, up, usingTransform);
        }
        else
        {
            SetCameraPoseBy_WL_CL(T_WL_CL, usingTransform);
        }
    }

    void COGL_xUp_yLeft_zBack()
    {
        // Create point directly, which should be under left hand system defined by Unity
        CreateAxis(Matrix4x4.identity); 

        // right hand camera (OpenGL defaul): x-up, y-left, z-back; COGL means camera under OpenGL default system.
        // rendering expectation: x(red)-invisible, y(green)-right, z(blue)-down
        Matrix4x4 T_COGL_WL = new Matrix4x4(
                new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, -1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -80.0f, 1.0f)
            );
        SetCameraPoseBy_COGL_WL(T_COGL_WL);
    }

    void COGL_xDown_yForward_zRight()
    {
        // Create point directly, which should be under left hand system defined by Unity
        CreateAxis(Matrix4x4.identity); 

        // right hand camera (OpenGL defaul): x-down, y-forward, z-right; COGL means camera under OpenGL default system.
        // rendering expectation: x(red)-down, y(green)-left, z(blue)-invisible
        Matrix4x4 T_COGL_WL = new Matrix4x4(
                new Vector4(0.0f, -1.0f, 0.0f, 0.0f),
                new Vector4(-1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -80.0f, 1.0f)
            );
        SetCameraPoseBy_COGL_WL(T_COGL_WL);
    }

    void SetSFMPose(Matrix4x4 T_CSFM_WR, Matrix4x4 T_WL_WR, bool converPointCloud, bool usingTransform)
    {
        Matrix4x4 T_COGL_CSFM = new Matrix4x4(
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, -1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            );

        Matrix4x4 T_WL_WR_usage = Matrix4x4.identity;

        // usingTransform is true means we want to use camera.transform, then we have to convert 
        // all points to left hand system no matter whether converPointCloud is true or not.
        if (converPointCloud || usingTransform)
        {
            T_WL_WR_usage = T_WL_WR;
        }
        CreateAxis(T_WL_WR_usage);
        Matrix4x4 T_COGL_WL = T_COGL_CSFM * T_CSFM_WR * T_WL_WR_usage.inverse;
        if (usingTransform)
        {
            // we want to use camera.transform, then we should convert T_CSFM_WR to T_WL_CL
            Matrix4x4 T_COGL_CL = new Matrix4x4(
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            );
            Matrix4x4 T_WL_CL = T_COGL_WL.inverse * T_COGL_CL;
            cam.transform.rotation = T_WL_CL.rotation;
            cam.transform.position = T_WL_CL.GetPosition();
        }
        else
        {
            cam.worldToCameraMatrix = T_COGL_WL;
        }
        PrintProjectionInfoSFM(K_OGL_, T_CSFM_WR);
    }

    void Test_SFM_Input(Matrix4x4 T_CSFM_WR, bool z_negative_only, bool converPointCloud, bool usingTransform)
    {
        Matrix4x4 T_WL_WR;
        if (z_negative_only)
        {
            T_WL_WR = new Matrix4x4(
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            );
        }
        else
        {
            T_WL_WR = new Matrix4x4(
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
                new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            );
        }
        SetSFMPose(T_CSFM_WR, T_WL_WR, converPointCloud, usingTransform);
    }

    void SFM_xRight_yDown_zForward(bool z_negative_only, bool converPointCloud, bool usingTransform)
    {
        // rendering expectation: x(red)-invisible, y(green)-right, z(blue)-up
        Matrix4x4 T_CSFM_WR = new Matrix4x4(
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, -1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 80.0f, 1.0f)
            );

        Test_SFM_Input(T_CSFM_WR, z_negative_only, converPointCloud, usingTransform);
    }

    void SFM_xDown_yBack_zLeft(bool z_negative_only, bool converPointCloud, bool usingTransform)
    {
        // rendering expectation: x(red)-down, y(green)-invisible, z(blue)-left
        Matrix4x4 T_CSFM_WR = new Matrix4x4(
                new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(-1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 80.0f, 1.0f)
            );

        Test_SFM_Input(T_CSFM_WR, z_negative_only, converPointCloud, usingTransform);
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        CreateDefaultPoints();
        SetProjection();

        // enable one of the tests below:

        COGL_xUp_yLeft_zBack();
        //COGL_xDown_yForward_zRight();

        //CL_xUp_yLeft_zForward(false, false);
        //CL_xUp_yLeft_zForward(false, true);
        //CL_xUp_yLeft_zForward(true, false);
        //CL_xUp_yLeft_zForward(true, true);
        //CL_xDown_yForward_zLeft(false, false);
        //CL_xDown_yForward_zLeft(false, true);
        //CL_xDown_yForward_zLeft(true, false);
        //CL_xDown_yForward_zLeft(true, true);

        //SFM_xRight_yDown_zForward(false, false, false);
        //SFM_xRight_yDown_zForward(false, false, true);
        //SFM_xRight_yDown_zForward(false, true, false);
        //SFM_xRight_yDown_zForward(false, true, true);
        //SFM_xRight_yDown_zForward(true, false, false);
        //SFM_xRight_yDown_zForward(true, false, true);
        //SFM_xRight_yDown_zForward(true, true, false);
        //SFM_xRight_yDown_zForward(true, true, true);

        //SFM_xDown_yBack_zLeft(false, false, false);
        //SFM_xDown_yBack_zLeft(false, false, true);
        //SFM_xDown_yBack_zLeft(false, true, false);
        //SFM_xDown_yBack_zLeft(false, true, true);
        //SFM_xDown_yBack_zLeft(true, false, false);
        //SFM_xDown_yBack_zLeft(true, false, true);
        //SFM_xDown_yBack_zLeft(true, true, false);
        //SFM_xDown_yBack_zLeft(true, true, true);

        ScreenCapture.CaptureScreenshot("rst.png");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            float u = mousePosition.x;
            float v = Screen.height - mousePosition.y;
            PrintPointInfo(new Vector2(u, v), "mouse pos");
        }
    }
}
