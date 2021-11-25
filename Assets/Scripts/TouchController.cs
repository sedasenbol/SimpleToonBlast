using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchController : MonoBehaviour, IPointerDownHandler
{
    public static event Action<PointerEventData> OnPlayerTapped;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        OnPlayerTapped?.Invoke(eventData);
    }
}
