using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Utilities;

public class InventorySystem : Singleton<InventorySystem>, IPointerDownHandler, IPointerUpHandler
{
    #region Attributes

    [Header("Parameters")]
    [SerializeField] [Range(1,9)] private int _nbColumns;
    [SerializeField] [Range(1,4)] private int _nbRows;

    [SerializeField] private float _splitInitialDelay = .25f;
    [SerializeField] private float _splitMinDelay = .02f;
    [SerializeField] private float _splitStep = .02f;

    [Header("Parents")] 
    [SerializeField] private Transform _quickSlotsParent;
    [SerializeField] private Transform _invSlotsParent;
    [SerializeField] private Transform _craftSlotsParent;
    [SerializeField] private Transform _craftSlotsTransform;

    [Header("Prefabs")]
    [SerializeField] private GameObject _slotPfb;
    [SerializeField] private GameObject _dropPfb;
    [SerializeField] private GameObject _craftSlotPfb;

    [Header("References")] 
    [SerializeField] private CraftingUI _craftingUI;
    
    private HandSlot _handSlot;
    
    private readonly List<InventorySlot> _quickSlots = new();
    private readonly List<InventorySlot> _inventorySlots = new();
    private readonly List<CraftingSlot> _craftingSlots = new();

    private int _selectedSlotIndex; 
    private int _craftSelectedSlotIndex;
    private Coroutine _splitRoutine;
    
    #endregion

    #region Properties

    private bool _invOpen => _invSlotsParent.gameObject.activeSelf;
    private bool _craftOpen => _craftSlotsTransform.gameObject.activeSelf;
    public bool IsInventoryOpen => _invSlotsParent.gameObject.activeSelf;
    private List<InventorySlot> _allSlots
    {
        get
        {
            var list = new List<InventorySlot>(_quickSlots);
            list.AddRange(_inventorySlots);
            return list;
        }
    }
    
    public List<CraftRecipe> craftables;

    private CraftingFlags _craftingFlags;
    public CraftingFlags CraftingFlags
    {
        get => _craftingFlags;
        set
        {
            _craftingFlags = value;
            OnCraftingFlagsChanged?.Invoke();
        }
    }

    #endregion

    #region Events

    public event Action OnCraftablesChanged;
    public event Action OnInventoryChanged;
    public event Action OnCraftingFlagsChanged;
    
    private void OnEnable()
    {
        OnInventoryChanged += RefreshCraftables;
        OnCraftingFlagsChanged += RefreshCraftables;
        OnCraftablesChanged += CraftablesChanged;

        // OnCraftablesChanged += () =>
        // {
        //     var craft = "";
        //     craftables.ForEach(c => craft += c.itemCrafted.itemID + " ");
        //     print("craftables changed: < " + craft + ">");
        // };
        // OnCraftingFlagsChanged += () => print("crafting flags changed: " + CraftingFlags);
        // OnInventoryChanged += () => print("inventory changed");
    }

    private void OnDisable()
    {
        OnInventoryChanged -= RefreshCraftables;
        OnCraftingFlagsChanged -= RefreshCraftables;
        OnCraftablesChanged -= CraftablesChanged;
    }
    
    protected override void SingletonAwake()
    {
        InitQuickslots();
        InitInventorySlots();
        _handSlot = FindObjectOfType<HandSlot>();
        _invSlotsParent.gameObject.SetActive(false);
        _craftSlotsTransform.gameObject.SetActive(false);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInventoryOpen)
            return;

        var results = Physics2D.RaycastAll(eventData.position, Vector2.zero);
        InventorySlot slot = results.FirstOrDefault(h => h.collider.GetComponent<InventorySlot>() is not null)
            .collider?.GetComponent<InventorySlot>();


        if (slot is not null)
        {
            if (eventData.button.Equals(PointerEventData.InputButton.Left))
            {
                GrabSlot(slot);

                if (slot.ItemStack is not null)
                    TooltipSystem.Instance.Show(slot.ItemStack.ItemName, slot.ItemStack.ItemDescription);
            }
            else if (eventData.button.Equals(PointerEventData.InputButton.Right))
            {
                _splitRoutine = StartCoroutine(SplitRoutine(slot));
            }
        }
        else 
        {
            if (eventData.button.Equals(PointerEventData.InputButton.Left))
            {
                DropItemStack(_handSlot);
            }
        }
    }

    private void DropItemStack(Slot slot)
    {
        if (slot.ItemStack is null)
            return;

        var drop = Instantiate(_dropPfb,
            GameManager.Instance.player.transform.position,
            Quaternion.identity).GetComponent<Drop>();

        drop.transform.position = new Vector3(drop.transform.position.x, drop.transform.position.y, 0);
        
        drop.ItemStack = slot.ItemStack;
        slot.ItemStack = null;
        OnInventoryChanged?.Invoke();
        
        drop.ThrowInPlayerDir();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInventoryOpen || !eventData.button.Equals(PointerEventData.InputButton.Right))
            return;

        if (_splitRoutine is not null)  
            StopCoroutine(_splitRoutine);
    }
    
    public void ToggleInventory()
    {
        _invSlotsParent.gameObject.SetActive(!_invOpen);
        _craftSlotsTransform.gameObject.SetActive(craftables.Count > 0 && _invOpen);
        _quickSlots[_selectedSlotIndex].Select(!_invOpen);

        ForAllSlots(slot => slot.Refresh());

        if (!IsInventoryOpen)
        {
            TooltipSystem.Instance.Hide();
        }
        else
        {
            SelectCraftSlot(Mathf.Clamp(_craftSelectedSlotIndex, 0, _craftingSlots.Count - 1));
        }
    }

    private void CraftablesChanged()
    {
        ClearCrafts();
        PopulateCrafts();

        _craftSlotsTransform.gameObject.SetActive(craftables.Count > 0 && _invOpen);
        
        if (_craftingSlots.Count > 0)
            SelectCraftSlot(Mathf.Clamp(_craftSelectedSlotIndex, 0, _craftingSlots.Count - 1));
    }

    #endregion

    #region InitInventory

    private void InitQuickslots()
    {
        for (var i = 0; i < _nbColumns; i++)
        {
            var quickSlot = Instantiate(_slotPfb, _quickSlotsParent).GetComponent<InventorySlot>();
            _quickSlots.Add(quickSlot);
        }
        
        SelectSlot(0);
    }
    
    private void InitInventorySlots()
    {
        for (var i = 0; i < _nbRows; i++)
        {
            for (var j = 0; j < _nbColumns; j++)
            {
                var slot = Instantiate(_slotPfb, _invSlotsParent).GetComponent<InventorySlot>();
                _inventorySlots.Add(slot);
            }
        }
    }

    #endregion

    #region Slots

    private void SelectSlot(int slotIndex)
    {
        if (_quickSlots[_selectedSlotIndex])
            _quickSlots[_selectedSlotIndex].Select(false);

        _quickSlots[slotIndex].Select(true);
        _selectedSlotIndex = slotIndex;
    }

    public void SelectNextSlot()
    {
        var nextSlot = _selectedSlotIndex + 1;

        if (_selectedSlotIndex.Equals(_quickSlots.Count - 1))
            nextSlot = 0;
        
        SelectSlot(nextSlot);
    }

    public void SelectPreviousSlot()
    {
        var previousSlot = _selectedSlotIndex - 1;

        if (_selectedSlotIndex.Equals(0))
            previousSlot = _quickSlots.Count - 1;

        SelectSlot(previousSlot);
    }

    public void OnClickCraftSlot(CraftingSlot slot)
    {
        var newIndex = _craftingSlots.IndexOf(slot);

        if (_craftingUI.SelectedIndex == newIndex && _craftingUI.HasCenteredSelection)
        {
            TryCraft(slot.Recipe);
        }
        else if (_craftingUI.SelectedIndex != newIndex)
        {
            SelectCraftSlot(newIndex);
        }
    }
    
    private void SelectCraftSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex > _craftingSlots.Count - 1)
            return;
        
        if (_craftingSlots.Count > _craftSelectedSlotIndex)
            _craftingSlots[_craftSelectedSlotIndex].Select(false);

        _craftingSlots[slotIndex].Select(true);
        _craftSelectedSlotIndex = slotIndex;

        _craftingUI.Select(_craftingSlots[_craftSelectedSlotIndex].transform, _craftingSlots[_craftSelectedSlotIndex].Recipe, _craftSelectedSlotIndex);
    }

    public void SelectNextCraftSlot()
    {
        if (!_craftOpen) return;
        
        var nextSlot = _craftSelectedSlotIndex + 1;

        if (_craftSelectedSlotIndex.Equals(_craftingSlots.Count - 1))
            nextSlot = _craftingSlots.Count - 1;
        
        SelectCraftSlot(nextSlot);
    }

    public void SelectPreviousCraftSlot()
    {
        if (!_craftOpen) return;
        
        var previousSlot = _craftSelectedSlotIndex - 1;

        if (_craftSelectedSlotIndex.Equals(0))
            previousSlot = 0;

        SelectCraftSlot(previousSlot);
    }
    
    private void GrabSlot(InventorySlot slot)
    {
        if (_handSlot.ItemStack is not null &&
            slot.ItemStack is not null &&
            _handSlot.ItemStack.itemID.Equals(slot.ItemStack.itemID))
        {
            slot.ItemStack.Add(_handSlot.ItemStack.number);
            _handSlot.ItemStack = null;
        }
        else
        {
            var aux = _handSlot.ItemStack;
        
            _handSlot.ItemStack = slot.ItemStack;
            slot.ItemStack = aux;  
        }
        
        OnInventoryChanged?.Invoke();
    }
    
    private void SplitSlot(InventorySlot slot)
    {
        if (slot.ItemStack is null)
            return;
        
        if (_handSlot.ItemStack is null)
        {
            var stack = new ItemStack
            {
                itemID = slot.ItemStack.itemID,
                number = 1
            };
            slot.ItemStackRemoveNumber(1);
            _handSlot.ItemStack = stack;
        }
        else
        {
            if (!_handSlot.ItemStack.itemID.Equals(slot.ItemStack.itemID))
                return;

            _handSlot.ItemStack.number++;
            slot.ItemStackRemoveNumber(1);
        }
        
        OnInventoryChanged?.Invoke();
    }

    #endregion

    #region Getters

    public Slot GetSelectedSlot()
    {
        if (_handSlot.ItemStack is not null)
            return _handSlot;
        
        if (_quickSlots[_selectedSlotIndex].ItemStack == null)
            return null;

        return _quickSlots[_selectedSlotIndex];
    }

    private InventorySlot GetClosestSlot(Item item)
    {
        foreach (var slot in _allSlots.Where(slot => slot.ItemStack?.GetItem() == item))
        {
            return slot;
        }

        return _allSlots.FirstOrDefault(slot => slot.ItemStack == null);
    }
    
    #endregion

    #region Items

    public bool CanAddItem(ItemStack itemStack)
    {
        return GetClosestSlot(itemStack.GetItem());
    }

    public bool AddItem(ItemStack itemStack)
    {
        var slot = GetClosestSlot(itemStack.GetItem());
        if (!slot)
            return false;

        if (slot.ItemStack == null)
        {
            slot.ItemStack = itemStack;
        }
        else
        {
            slot.ItemStack.number += itemStack.number;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }
    
    public void RemoveItem(string itemID, int count)
    {
        ForAllSlots(s =>
        {
            if (s.ItemStack is not null && s.ItemStack.itemID == itemID)
            {
                while (count > 0)
                {
                    s.ItemStackRemoveNumber(1);
                    count--;
                    OnInventoryChanged?.Invoke();
                    
                    if (s.ItemStack is null)
                        break;
                }
            }
        });
    }

    #endregion

    #region Craft

    public void TryCraft(CraftRecipe recipe)
    {
        if (!recipe.CanBeCrafted())
            return;

        if (_handSlot.ItemStack is not null && !_handSlot.ItemStack.itemID.Equals(recipe.itemCrafted.itemID))
            return;
        
        foreach (var item in recipe.itemsRequired)
        {
            RemoveItem(item.itemID, item.number);
        }

        if (_handSlot.ItemStack is null)
            _handSlot.ItemStack = recipe.itemCrafted.Clone();
        else
            _handSlot.ItemStack.Add(recipe.itemCrafted.number);
    }

    private void RefreshCraftables()
    {
        List<CraftRecipe> newCraftables = new List<CraftRecipe>();

        var database = GameManager.Instance.database;
        foreach (var craftRecipe in database.craftRecipes)
        {
            if (craftRecipe.CanBeCrafted())
            {
                newCraftables.Add(craftRecipe);
            }
        }

        if (!newCraftables.SequenceEqual(craftables))
        {
            craftables = newCraftables;
            OnCraftablesChanged?.Invoke();
        }
    }

    private void ClearCrafts()
    {
        foreach (Transform craft in _craftSlotsParent)
        {
            Destroy(craft.gameObject);
        }
        
        _craftingSlots.Clear();
    }

    private void PopulateCrafts()
    {
        foreach (var craft in craftables)
        {
            var craftSlot = Instantiate(_craftSlotPfb, _craftSlotsParent).GetComponent<CraftingSlot>();
            _craftingSlots.Add(craftSlot);
            craftSlot.Recipe = craft;
        }
    }

    #endregion

    #region Utils

    public int CountItem(string itemID)
    {
        int total = 0;
        ForAllSlots(s =>
        {
            if (s.ItemStack is not null && s.ItemStack.itemID == itemID)
            {
                total += s.ItemStack.number;
            }
        });
        return total;
    }
    
    private void ForAllSlots(Action<InventorySlot> functor)
    {
        foreach (var slot in _quickSlots)
        {
            functor.Invoke(slot);
        }
        
        foreach (var slot in _inventorySlots)
        {
            functor.Invoke(slot);
        }
    }

    #endregion

    #region Coroutines

    private IEnumerator SplitRoutine(InventorySlot slot)
    {
        var delay = _splitInitialDelay;
        
        while (true)
        {
            SplitSlot(slot);
            yield return new WaitForSeconds(delay);
            delay = Mathf.Max(delay - _splitStep, _splitMinDelay);
        }
    }

    #endregion
    
}
