using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bulldozer
{
    /// <summary>Invokes <see cref="OnRightClick"/> when the GameObject is right-clicked.</summary>
    public class RightClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public Action OnRightClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick?.Invoke();
            }
        }
    }
}
