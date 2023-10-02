using System;
using System.Collections;
using System.Collections.Generic;
using Inventory;
using UnityEngine;

public class CraftingUI : MonoBehaviour
{
    [Header("Horizontal")]
    [SerializeField] private Transform _horizontalContainer;
    [SerializeField] private GameObject _readOnlySlot;
    
    [Header("Vertical")]
    [SerializeField] private Transform _content;
    [SerializeField] private RectTransform _middle;
    private float _distance;
    private CraftRecipe _recipe;

    private Coroutine _scrollUpRoutine;
    private Coroutine _scrollDownRoutine;

    public void Select(RectTransform rt, CraftRecipe recipe)
    {
        StopAllCoroutines();
        _recipe = recipe;
        
        ClearHorizontalContainer();
        PopulateHorizontalContainer();
        
        _distance = rt.position.y - _middle.position.y;

        if (_distance < 0)
            _scrollUpRoutine = StartCoroutine(ScrollUp(rt));
        else if (_distance > 0)
            _scrollDownRoutine = StartCoroutine(ScrollDown(rt));
    }

    private void ClearHorizontalContainer()
    {
        foreach (Transform child in _horizontalContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulateHorizontalContainer()
    {
        foreach (var stack in _recipe.itemsRequired)
        {
            var readOnlySlot = Instantiate(_readOnlySlot, _horizontalContainer).GetComponent<ReadOnlySlot>();
            readOnlySlot.ItemStack = stack;
        }
    }

    private IEnumerator ScrollUp(RectTransform rt)
    {
        while (_distance < 0)
        {
            if (rt is null) StopCoroutine(_scrollUpRoutine);
            _distance = rt.position.y - _middle.position.y;
            _content.position += new Vector3(0, 1, 0);
            yield return null;
        }
    }
    
    private IEnumerator ScrollDown(RectTransform rt)
    {
        while (_distance > 0) 
        {
            if (rt is null) StopCoroutine(_scrollDownRoutine);
            _distance = rt.position.y - _middle.position.y;
            _content.position += new Vector3(0, -1, 0);
            yield return null;
        }
    }
}
