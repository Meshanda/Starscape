using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class RocketInteraction : MonoBehaviour
    {
        private void Awake()
        {
            Physics2D.queriesHitTriggers = true;
        }

        private void OnMouseOver()
        {
            if (!Input.GetMouseButtonDown(1)) return;
            
            GameManager.Instance.ToggleRocketPopUp();
        }

        private void Update()
        {
            transform.position = transform.parent.position;
        }
    }
}