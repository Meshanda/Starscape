using UnityEngine;

namespace UI
{
    public class CursorManager : Singleton<CursorManager>
    {
        [SerializeField] private Canvas _cursorCanvas;
        [SerializeField] private GameObject _cursorPfb;
        private GameObject _cursor;

        private void Start()
        {
            SetCursor();
        }

        private void Update()
        {
            _cursor.transform.position = Input.mousePosition;
        }

        public void SetCursor()
        {
            _cursor = Instantiate(_cursorPfb, _cursorCanvas.transform);
            Cursor.visible = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Cursor.visible = false;
        }
    }
}