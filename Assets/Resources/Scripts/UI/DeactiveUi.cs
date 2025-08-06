using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DeactiveUi : MonoBehaviour
{

    [SerializeField] private List<GameObject> uiList;

    public static event Action OnDeactiveSpecificUi;
    public static event Action OnActiveSpecificUi;

    /// <summary>
    /// Se <c>true</c>, desativa as UI chamando <c>OnDeactiveSpecificUi</c>;
    /// caso contrário, ativa as UI chamando <c>OnActiveSpecificUi</c>.
    /// </summary>
    public static void ControlUi(bool isActive) =>
        (isActive ? OnDeactiveSpecificUi : OnActiveSpecificUi)?.Invoke();


    private void Start()
    {
        OnDeactiveSpecificUi += OnDeactiveUi;
        OnActiveSpecificUi += OnActiveUi;
    }

    private void OnDisable()
    {
        OnDeactiveSpecificUi -= OnDeactiveUi;
        OnActiveSpecificUi -= OnActiveUi;
    }

    public void OnDeactiveUi()
    {
        if (uiList == null) return;

        foreach (var i in uiList)
        {
            if (i != null) i.SetActive(false);
        }
    }

    public void OnActiveUi()
    {
        if (uiList == null) return;

        foreach (var i in uiList)
        {
            if (i != null) i.SetActive(true);
        }
    }


}