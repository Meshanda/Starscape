using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySystem : Singleton<InventorySystem>, IPointerDownHandler, IPointerUpHandler
{
    #region Attributes

    [Header("Parameters")]
    [SerializeField] [Range(1,9)] private int _nbColumns;
    [SerializeField] [Range(1,4)] private int _nbRows;
    [SerializeField] public int _nbChestSlots = 18;

    public float _splitInitialDelay = .25f;
    public float _splitMinDelay = .02f;
    public float _splitStep = .02f;

    [Header("Parents")] 
    [SerializeField] private Transform _quickSlotsParent;
    [SerializeField] private Transform _invSlotsParent;
    [SerializeField] private Transform _craftSlotsParent;
    [SerializeField] private Transform _craftSlotsTransform;
    [SerializeField] private Transform _chestSlotsTransform;
    [SerializeField] private Transform _buttonTransform;

    [Header("Prefabs")]
    [SerializeField] private GameObject _slotPfb;
    [SerializeField] private GameObject _craftSlotPfb;
    [SerializeField] private GameObject _chestSlotPfb;

    [Header("References")] 
    [SerializeField] private CraftingUI _craftingUI;
    
    private HandSlot _handSlot;
    
    private readonly List<InventorySlot> _quickSlots = new();
    private readonly List<InventorySlot> _inventorySlots = new();
    private readonly List<CraftingSlot> _craftingSlots = new();
    private readonly List<InventorySlot> _chestSlots = new();

    private int _selectedSlotIndex; 
    private int _craftSelectedSlotIndex;
    private Coroutine _splitRoutine;

    private bool _chestOpen;
    private Chest _currentChest;
    private bool _isInCreative;
    
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
        InitChestInventory();
        _handSlot = FindObjectOfType<HandSlot>();
        _invSlotsParent.gameObject.SetActive(false);
        _craftSlotsTransform.gameObject.SetActive(false);
    }

    private void Start()
    {
        OnInventoryChanged?.Invoke();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCreative();
        }
        
        if (_currentChest is null) return;
        
        if (Vector2.Distance(GameManager.Instance.player.transform.position, _currentChest.transform.position) >= _currentChest.distance)
        {
            CloseChest(_currentChest);
        }
    }

    public void ToggleCreative()
    {
        _isInCreative = !_isInCreative;
        DoRefreshCraftables();
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

                if (ItemStack.IsValid(slot.ItemStack))
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

        World.Instance.GenerateDrop(slot.ItemStack.Clone(), GameManager.Instance.player.DropPosition.position).ThrowInPlayerDir();
        slot.ItemStack = null;
        OnInventoryChanged?.Invoke();
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
        _buttonTransform.gameObject.SetActive(_invOpen);

        if (_invOpen is false)
            CloseChest(_currentChest);
        
        _chestSlotsTransform.gameObject.SetActive(_invOpen && _chestOpen);
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
    
    private void InitChestInventory()
    {
        _chestSlotsTransform.gameObject.SetActive(false);
        for (var i = 0; i < _nbChestSlots; i++)
        {
            var slot = Instantiate(_chestSlotPfb, _chestSlotsTransform).GetComponent<InventorySlot>();
            _chestSlots.Add(slot);
        }
    }

    #endregion

    #region Slots

    public void OpenChest(Chest chest)
    {
        if (_currentChest is not null && _currentChest != chest && _currentChest.ChestOpen) 
        {
            CloseChest(_currentChest);
        }
        
        _currentChest = chest;
        _chestOpen = true;
        
        for (int i = 0; i < _nbChestSlots; i++)
        {
            _chestSlots[i].ItemStack = chest.itemStacks[i];
        }

        if (!_invOpen)
            ToggleInventory();
        else
            _chestSlotsTransform.gameObject.SetActive(_chestOpen);
    }

    public void CloseChest(Chest chest)
    {
        if (chest is null) return;
        
        chest.UpdateChest(_chestSlots);
        chest.ChestOpen = false;
        _currentChest = null;
        _chestOpen = false;
        _chestSlotsTransform.gameObject.SetActive(_chestOpen);
        EmptyChestSlot();
    }

    public void EmptyChestSlot()
    {
        foreach (var slot in _chestSlots)
        {
            slot.ItemStack = null;
        }
    }
    
    public void SelectSlot(int slotIndex)
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
        if (_handSlot.ItemStack is not null || slot.ItemStack is not null)
            SoundManager.Instance.PlayInventoryClickSound();

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
        
        SoundManager.Instance.PlayInventoryClickSound();
        
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
        {
            _handSlot.ItemStack.Add(recipe.itemCrafted.number);
            SoundManager.Instance.PlayInventoryClickSound();
        }
    }

    private void RefreshCraftables()
    {
        if (_isInCreative) // don't refresh on every change, we don't care!
        {
            return;
        }
        
        DoRefreshCraftables();
    }
    
    private void DoRefreshCraftables()
    {
        List<CraftRecipe> newCraftables = new List<CraftRecipe>();

        var database = GameManager.Instance.database;
        if (_isInCreative)
        {
            List<string> uniqueIDs = new List<string>();
            foreach (var item in database.items)
            {
                if (string.IsNullOrEmpty(item.id))
                    continue;
                
                if (!uniqueIDs.Contains(item.id))
                {
                    uniqueIDs.Add(item.id);
                    newCraftables.Add(new CraftRecipe()
                    {
                        itemCrafted = new ItemStack()
                        {
                            itemID = item.id,
                            number = 1,
                        }
                    });
                }
            }
        }
        else
        {
            foreach (var craftRecipe in database.craftRecipes)
            {
                if (craftRecipe.CanBeCrafted())
                {
                    newCraftables.Add(craftRecipe);
                }
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

    #region Click Buttons

    public void ClickSettings()
    {
        GameManager.Instance.ToggleSettings();
        ToggleInventory();
    }

    public void ClickBackToMenu()
    {
        GameManager.Instance.TogglePopUpConfirmer();
        ToggleInventory();
    }

    #endregion
}
