using UnityEditor;
using UnityEngine;

namespace _Project.World.Planet.Scripts
{
    [InitializeOnLoad]
    public static class PlanetarySceneView
    {
        private const string PrefKey = "PlanetarySceneView_IsEnabled";

        private static readonly Vector3 PlanetCenter = Vector3.zero;
        private static float _flySpeed = 20f;
        private const float MouseSensitivity = 0.3f;

        private static bool _wPressed, _aPressed, _sPressed, _dPressed, _qPressed, _ePressed, _shiftPressed;
        private static double _lastTime;
        private static Vector3 _currentForward;

        private static bool IsEnabled
        {
            get => EditorPrefs.GetBool(PrefKey, false);
            set => EditorPrefs.SetBool(PrefKey, value);
        }

        static PlanetarySceneView()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        [MenuItem("Tools/Planetary Scene View")]
        private static void Toggle()
        {
            IsEnabled = !IsEnabled;

            if (!IsEnabled)
            {
                DisableAndReset();
            }

            Debug.Log("Planetary Scene View " + (IsEnabled ? "active" : "inactive"));
        }

        [MenuItem("Tools/Planetary Scene View", true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked("Tools/Planetary Scene View", IsEnabled);
            return true;
        }

        private static void DisableAndReset()
        {
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
            EditorGUIUtility.editingTextField = false;
            ResetKeys();

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.camera != null)
            {
                Vector3 forward = sceneView.rotation * Vector3.forward;
                if (Mathf.Abs(Vector3.Dot(forward, Vector3.up)) < 0.99f)
                {
                    Quaternion resetRot = Quaternion.LookRotation(forward, Vector3.up);
                    sceneView.LookAt(sceneView.pivot, resetRot);
                }

                sceneView.Repaint();
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!IsEnabled || sceneView.camera == null) return;

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID("PlanetaryFly".GetHashCode(), FocusType.Keyboard);

            if (e.type == EventType.ContextClick)
            {
                e.Use();
                return;
            }

            if (e.button == 1)
            {
                if (e.type == EventType.MouseDown)
                {
                    GUIUtility.hotControl = controlID;
                    GUIUtility.keyboardControl = controlID;
                    EditorGUIUtility.editingTextField = true;
                    _lastTime = EditorApplication.timeSinceStartup;
                    _currentForward = sceneView.rotation * Vector3.forward;
                    ResetKeys();
                    e.Use();
                }
                else if (e.type == EventType.MouseUp && GUIUtility.hotControl == controlID)
                {
                    GUIUtility.hotControl = 0;
                    GUIUtility.keyboardControl = 0;
                    EditorGUIUtility.editingTextField = false;
                    ResetKeys();
                    e.Use();
                }
            }

            if (GUIUtility.hotControl == controlID)
            {
                if (e.type == EventType.ValidateCommand || e.type == EventType.ExecuteCommand)
                {
                    e.Use();
                    return;
                }

                if (e.isKey || e.type == EventType.KeyDown || e.type == EventType.KeyUp)
                {
                    HandleKeyEvents(e);
                    e.Use();
                }
                else if (e.type == EventType.ScrollWheel)
                {
                    _flySpeed = Mathf.Max(1f, _flySpeed - e.delta.y * 2f);
                    e.Use();
                }

                ProcessFlying(sceneView, e);
                sceneView.Repaint();
            }

            if (!e.alt || e.button != 0) return;
            switch (e.type)
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = controlID + 1;
                    e.Use();
                    break;
                case EventType.MouseUp when GUIUtility.hotControl == controlID + 1:
                    GUIUtility.hotControl = 0;
                    e.Use();
                    break;
                case EventType.MouseDrag when GUIUtility.hotControl == controlID + 1:
                    ProcessOrbit(sceneView, e);
                    e.Use();
                    break;
            }
        }

        private static void ProcessFlying(SceneView sceneView, Event e)
        {
            double currentTime = EditorApplication.timeSinceStartup;
            float dt = (float)(currentTime - _lastTime);
            _lastTime = currentTime;
            if (dt > 0.1f) dt = 0.016f;

            Vector3 camPos = sceneView.camera.transform.position;
            Vector3 planetUp = (camPos - PlanetCenter).normalized;
            if (planetUp == Vector3.zero) planetUp = Vector3.up;

            if (e.type == EventType.MouseDrag || e.type == EventType.MouseMove)
            {
                float dx = e.delta.x * MouseSensitivity;
                float dy = e.delta.y * MouseSensitivity;

                _currentForward = Quaternion.AngleAxis(dx, planetUp) * _currentForward;

                Vector3 right = Vector3.Cross(planetUp, _currentForward).normalized;
                _currentForward = Quaternion.AngleAxis(dy, right) * _currentForward;

                e.Use();
            }

            float dot = Vector3.Dot(_currentForward, planetUp);
            if (Mathf.Abs(dot) > 0.98f)
            {
                _currentForward = Vector3.RotateTowards(_currentForward, dot > 0 ? planetUp : -planetUp, -0.01f, 0f);
            }

            Quaternion camRot = Quaternion.LookRotation(_currentForward, planetUp);

            Vector3 localInput = Vector3.zero;
            if (_wPressed) localInput += Vector3.forward;
            if (_sPressed) localInput += Vector3.back;
            if (_dPressed) localInput += Vector3.right;
            if (_aPressed) localInput += Vector3.left;

            if (localInput != Vector3.zero || _qPressed || _ePressed)
            {
                Vector3 moveDir = camRot * localInput.normalized;
                if (_ePressed) moveDir += planetUp;
                if (_qPressed) moveDir -= planetUp;
                moveDir.Normalize();

                float speed = _flySpeed * (_shiftPressed ? 3f : 1f);
                float stepDist = speed * dt;

                float currentRadius = Vector3.Distance(camPos, PlanetCenter);
                float radialComp = Vector3.Dot(moveDir, planetUp);
                Vector3 tangentComp = moveDir - radialComp * planetUp;

                currentRadius += radialComp * stepDist;

                if (tangentComp.sqrMagnitude > 0.0001f)
                {
                    Vector3 tangentDir = tangentComp.normalized;
                    float tangentDist = tangentComp.magnitude * stepDist;

                    Vector3 rotAxis = Vector3.Cross(planetUp, tangentDir).normalized;
                    float angleDeg = (tangentDist / currentRadius) * Mathf.Rad2Deg;
                    Quaternion sphereRot = Quaternion.AngleAxis(angleDeg, rotAxis);

                    Vector3 relPos = camPos - PlanetCenter;
                    relPos = sphereRot * relPos;
                    camPos = PlanetCenter + relPos.normalized * currentRadius;

                    _currentForward = sphereRot * _currentForward;
                }
                else
                {
                    camPos = PlanetCenter + planetUp * currentRadius;
                }

                planetUp = (camPos - PlanetCenter).normalized;
                camRot = Quaternion.LookRotation(_currentForward, planetUp);
            }

            float dist = sceneView.cameraDistance > 1f ? sceneView.cameraDistance : 10f;
            sceneView.pivot = camPos + _currentForward * dist;
            sceneView.rotation = camRot;
        }

        private static void ProcessOrbit(SceneView sceneView, Event e)
        {
            Vector3 up = (sceneView.pivot - PlanetCenter).normalized;
            if (up == Vector3.zero) up = Vector3.up;

            float dx = e.delta.x * 0.4f;
            float dy = e.delta.y * 0.4f;

            Quaternion yaw = Quaternion.AngleAxis(dx, up);
            Vector3 right = sceneView.rotation * Vector3.right;
            Quaternion pitch = Quaternion.AngleAxis(dy, right);

            sceneView.rotation = yaw * pitch * sceneView.rotation;
        }

        private static void HandleKeyEvents(Event e)
        {
            if (e.type != EventType.KeyDown && e.type != EventType.KeyUp) return;
            
            bool isDown = e.type == EventType.KeyDown;

            KeyCode code = e.keyCode;
            if (code == KeyCode.None)
            {
                code = e.character switch
                {
                    'a' or 'A' => KeyCode.A,
                    'w' or 'W' => KeyCode.W,
                    's' or 'S' => KeyCode.S,
                    'd' or 'D' => KeyCode.D,
                    'q' or 'Q' => KeyCode.Q,
                    'e' or 'E' => KeyCode.E,
                    _ => code
                };
            }

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (code)
            {
                case KeyCode.W: _wPressed = isDown; break;
                case KeyCode.S: _sPressed = isDown; break;
                case KeyCode.A: _aPressed = isDown; break;
                case KeyCode.D: _dPressed = isDown; break;
                
                case KeyCode.Q: _qPressed = isDown; break;
                case KeyCode.E: _ePressed = isDown; break;
                
                case KeyCode.LeftShift:
                case KeyCode.RightShift: _shiftPressed = isDown; break;
            }
        }

        
        private static void ResetKeys()
        {
            _wPressed = _aPressed = _sPressed = _dPressed = _qPressed = _ePressed = _shiftPressed = false; //bulk set everything to false
        }
    }
}