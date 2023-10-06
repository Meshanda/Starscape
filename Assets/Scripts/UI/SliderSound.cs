using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class SliderSound : MonoBehaviour, IPointerClickHandler
    {
        public void ClickSound()
        {
            SoundManager.Instance?.PlayClickSound();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ClickSound();
        }
    }
}