using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Utilities;

public class InventorySystem : Singleton<InventorySystem>, IPointerClickHandler
{
    [Header("Parameters")]
    [SerializeField] [Range(1,9)] private int _nbColumns;
    [SerializeField] [Range(1,4)] private int _nbRows;

    [Header("Parents")] 
    [SerializeField] private Transform _quickSlotsParent;
    [SerializeField] private Transform _invSlotsParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject _slotPfb;
    
    private HandSlot _handSlot;
    
    private readonly List<InventorySlot> _quickSlots = new();
    private readonly List<InventorySlot> _inventorySlots = new();

    private int _selectedSlotIndex;
    private bool _invOpen => _invSlotsParent.gameObject.activeSelf;
    private bool _craftingActive;
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

    protected override void SingletonAwake()
    {
        InitQuickslots();
        InitInventorySlots();
        _handSlot = FindObjectOfType<HandSlot>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsInventoryOpen)
            return;

        var hits = Physics2D.RaycastAll(eventData.position, Vector2.zero);
        foreach (var hit in hits)
        {
            if (hit.collider && hit.collider.transform.TryGetComponent(out InventorySlot slot))
            {
                GrabSlot(slot);
                TooltipSystem.Instance.Hide();
            }
        }
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
    }

    private void InitInventorySlots()
    {
        for (var i = 0; i < _nbRows; i++)
        {
            for (var j = 0; j < _nbColumns; j++)
            {
                var slot = Instantiate(_slotPfb, _invSlotsParent).GetComponent<InventorySlot>();
                slot.pos = new Vector2Int(i+1, j);
                _inventorySlots.Add(slot);
            }
        }
    }

    private void InitQuickslots()
    {
        for (var i = 0; i < _nbColumns; i++)
        {
            var quickSlot = Instantiate(_slotPfb, _quickSlotsParent).GetComponent<InventorySlot>();
            quickSlot.pos = new Vector2Int(0,i);
            _quickSlots.Add(quickSlot);
        }
        
        SelectSlot(0);
    }
    
    public void SetCraftingActive(bool active)
    {
        if (_craftingActive == active)
        {
            return;
        }

        _craftingActive = active;

        // print("SetCraftingActive " + active);
        // TODO
    }

    public void ToggleInventory()
    {
        _invSlotsParent.gameObject.SetActive(!_invOpen);

        if (!IsInventoryOpen)
            TooltipSystem.Instance.Hide();
    }
    
    public TileBase GetSelectedTile()
    {
        if (_quickSlots[_selectedSlotIndex].ItemStack == null)
            return null;

        return _quickSlots[_selectedSlotIndex].ItemStack.GetItem().tile;
    }

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

        return true;
    }

    public void RemoveItem()
    {
        _quickSlots[_selectedSlotIndex].ItemStack.number--;

        if (_quickSlots[_selectedSlotIndex].ItemStack.number <= 0)
            _quickSlots[_selectedSlotIndex].ItemStack = null;
    }
    
    public void RemoveItem(string itemID, int count)
    {
        ForAllSlots(s =>
        {
            if (s.ItemStack is not null && s.ItemStack.itemID == itemID)
            {
                while (count > 0)
                {
                    s.ItemStack.number--;
                    count--;

                    if (s.ItemStack.number <= 0)
                    {
                        s.ItemStack = null;
                        break;
                    }
                }
            }
        });
    }

    private InventorySlot GetClosestSlot(Item item)
    {
        foreach (var slot in _allSlots.Where(slot => slot.ItemStack?.GetItem() == item))
        {
            return slot;
        }

        return _allSlots.FirstOrDefault(slot => slot.ItemStack == null);
    }

    private int CountItem(string itemID)
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

    public bool TryCraft(CraftRecipe recipe)
    {
        if (!_craftingActive)
        {
            return false;
        }
        
        foreach (var item in recipe.itemsRequired)
        {
            if (CountItem(item.itemID) < item.number)
            {
                return false;
            }
        }
        
        foreach (var item in recipe.itemsRequired)
        {
            RemoveItem(item.itemID, item.number);
        }

        AddItem(recipe.itemCrafted.Clone());
        return true;
    }
    
    public void TryCraftTest()
    {
        print(TryCraft(GameManager.Instance.database.craftRecipes[0]));
    }
}
