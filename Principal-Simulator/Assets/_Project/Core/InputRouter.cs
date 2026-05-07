using UnityEngine;
using UnityEngine.EventSystems;
using TBS.Core.Events;
using TBS.Contracts.Events;

namespace TBS.Core
{
    /// <summary>
    /// 输入路由器 - 统一接管所有输入，通过 EventBus 事件转发给各系统
    /// </summary>
    public class InputRouter : MonoBehaviour
    {
        [SerializeField] private bool enabled = true;

        private Vector3 lastMousePosition;
        private bool isLeftDragging;
        private bool isRightDragging;

        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        private void Update()
        {
            if (!enabled) return;

            HandleMouseButtons();
            HandleMouseDrag();
            HandleScroll();
            HandleKeyboard();
        }

        private void HandleMouseButtons()
        {
            bool isOverUI = EventSystem.current.IsPointerOverGameObject();

            // 左键按下
            if (Input.GetMouseButtonDown(0))
            {
                isLeftDragging = true;
                lastMousePosition = Input.mousePosition;

                var evt = new MouseButtonDownEvent
                {
                    ScreenPos = Input.mousePosition,
                    Button = 0,
                    IsOverUI = isOverUI
                };
                EventBus.Emit(evt);
            }

            // 左键抬起
            if (Input.GetMouseButtonUp(0))
            {
                isLeftDragging = false;
                var evt = new MouseButtonUpEvent
                {
                    ScreenPos = Input.mousePosition,
                    Button = 0
                };
                EventBus.Emit(evt);
            }

            // 右键按下
            if (Input.GetMouseButtonDown(1))
            {
                isRightDragging = true;
                lastMousePosition = Input.mousePosition;

                var evt = new MouseButtonDownEvent
                {
                    ScreenPos = Input.mousePosition,
                    Button = 1,
                    IsOverUI = isOverUI
                };
                EventBus.Emit(evt);
            }

            // 右键抬起
            if (Input.GetMouseButtonUp(1))
            {
                isRightDragging = false;
                var evt = new MouseButtonUpEvent
                {
                    ScreenPos = Input.mousePosition,
                    Button = 1
                };
                EventBus.Emit(evt);
            }
        }

        private void HandleMouseDrag()
        {
            if (isLeftDragging || isRightDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                lastMousePosition = Input.mousePosition;

                int button = isLeftDragging ? 0 : 1;
                var evt = new MouseDragEvent
                {
                    Delta = delta,
                    Button = button
                };
                EventBus.Emit(evt);
            }
        }

        private void HandleScroll()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                var evt = new ScrollEvent { Delta = scroll };
                EventBus.Emit(evt);
            }
        }

        private void HandleKeyboard()
        {
            // WASD 和方向键
            if (Input.GetKey(KeyCode.W))
                EventBus.Emit(new KeyHeldEvent { Key = KeyCode.W });
            if (Input.GetKey(KeyCode.A))
                EventBus.Emit(new KeyHeldEvent { Key = KeyCode.A });
            if (Input.GetKey(KeyCode.S))
                EventBus.Emit(new KeyHeldEvent { Key = KeyCode.S });
            if (Input.GetKey(KeyCode.D))
                EventBus.Emit(new KeyHeldEvent { Key = KeyCode.D });

            if (Input.GetKey(KeyCode.UpArrow))
                EventBus.Emit(new KeyHeldEvent { Key = KeyCode.UpArrow });
            if (Input.GetKey(KeyCode.DownArrow))
                EventBus.Emit(new KeyHeldEvent { Key = KeyCode.DownArrow });
            if (Input.GetKey(KeyCode.LeftArrow))
                EventBus.Emit(new KeyHeldEvent { Key = KeyCode.LeftArrow });
            if (Input.GetKey(KeyCode.RightArrow))
                EventBus.Emit(new KeyHeldEvent { Key = KeyCode.RightArrow });

            // Q/E 旋转
            if (Input.GetKey(KeyCode.Q))
                EventBus.Emit(new KeyHeldEvent { Key = KeyCode.Q });
            if (Input.GetKey(KeyCode.E))
                EventBus.Emit(new KeyHeldEvent { Key = KeyCode.E });
        }
    }
}
