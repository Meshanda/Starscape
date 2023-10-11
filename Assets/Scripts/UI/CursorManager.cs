using UnityEngine;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

namespace UI
{
    public class CursorManager : Singleton<CursorManager>
    {
        [SerializeField] private Canvas _cursorCanvas;
        [SerializeField] private GameObject _cursorPfb;
        
        private GameObject _cursor;

        protected override void SingletonAwake()
        {
            SetCursor();
        }

        private void Update()
        {
            _cursor.transform.position = Input.mousePosition;
        }

        public void SetSize(float size)
        {
            _cursor.transform.localScale = new Vector3(size, size, size);
            Settings.cursorSize = size;
        }

        public void SetColor(Color newColor)
        {
            _cursor.GetComponent<Image>().color = newColor;
            Settings.cursorColor = newColor;
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