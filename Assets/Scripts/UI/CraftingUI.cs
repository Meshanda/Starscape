using System.Collections;
using Inventory;
using UnityEngine;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] private float _speed = 1000.0f;
    
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
    private Coroutine _scrollToRoutine;

    public int SelectedIndex { get; private set; } = -1;
    public bool HasCenteredSelection { get; private set; } = false;

    public void Select(Transform rt, CraftRecipe recipe, int selectedIndex)
    {
        if (_scrollToRoutine != null)
            CoroutineHelper.Instance.StopCoroutine(_scrollToRoutine);
        
        _recipe = recipe;
        
        ClearHorizontalContainer();
        PopulateHorizontalContainer();
        
        SelectedIndex = selectedIndex;
        HasCenteredSelection = false;

        _scrollToRoutine = CoroutineHelper.Instance.StartCoroutine(ScrollTo(rt));
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
    
    private IEnumerator ScrollTo(Transform rt)
    {
        yield return null;
        
        if (!rt)
        {
            yield break;
        }
        
        _distance = rt.position.y - _middle.position.y;
        
        while (Mathf.Abs(_distance) > 0.001f)
        {
            if (!rt)
            {
                yield break;
            }
            
            if (_distance > 0)
            {
                _content.position += new Vector3(0, Mathf.Max(-Mathf.Abs(_distance), -_speed * Time.deltaTime), 0);
            }
        
            if (_distance < 0)
            {
                _content.position += new Vector3(0, Mathf.Min(Mathf.Abs(_distance), _speed * Time.deltaTime), 0);
            }
            
            _distance = rt.position.y - _middle.position.y;
            
            yield return null;
        }

        HasCenteredSelection = true;
    }
}
