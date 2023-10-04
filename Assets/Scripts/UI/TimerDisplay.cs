using ScriptableObjects.Variables;
using TMPro;
using UnityEngine;

public class TimerDisplay : MonoBehaviour
{
    [SerializeField] private FloatVariable _timerVariable;
    [SerializeField] private TextMeshProUGUI _timerText;

    private void Update()
    {
        float minutes = Mathf.Floor(_timerVariable.value / 60);
        float seconds = Mathf.RoundToInt(_timerVariable.value%60);
            
        _timerText.text = $"{minutes:00}:{seconds:00}";
    }
}