using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Drop : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private float _collectDistance = 0.5f;
    [SerializeField] private float _magnetSpeed = 20.0f;
    [SerializeField] private float _throwForce = 50.0f;
    [SerializeField] private BoxCollider2D _magnetCollision;
    [SerializeField] private BoxCollider2D _hardCollision;

    private Rigidbody2D _rb;
    private bool _hasBeenAdded;
    
    [SerializeField, ReadOnlyField] private ItemStack _itemStack;
    public ItemStack ItemStack
    {
        get => _itemStack;
        set
        {
            _itemStack = value;
            
            if (_itemStack != null)
            {
                _renderer.sprite = _itemStack.GetItem().sprite;
            }
            else
            {
                print("Should not happen");
            }
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void AddRandomForce()
    {
        var randomForce = new Vector2(Random.value * 2 - 1, Random.value * 2 - 1).normalized;
        _rb.AddForce(randomForce * 10.0f);
    }
    
    public void ThrowInPlayerDir()
    {
        Vector2 dir = GameManager.Instance.player.transform.localScale.x < 0 ? Vector2.right : Vector2.left;
        dir += Vector2.up * 0.35f;
        _rb.AddForce(dir * _throwForce);

        StartCoroutine(DisableMagnet(0.3f));
    }

    private IEnumerator DisableMagnet(float delay)
    {
        _rb.gravityScale = 0;
        _magnetCollision.enabled = false;
        yield return new WaitForSeconds(delay);
        _rb.gravityScale = 1;
        _magnetCollision.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.tag.Equals("Player"))
            return;
        
        _hardCollision.enabled = false;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.tag.Equals("Player"))
            return;
        
        _hardCollision.enabled = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.tag.Equals("Player"))
            return;

        if (_hasBeenAdded)
            return;
        
        Transform dest = other.GetComponent<Player>().DropPosition;

        if (Vector2.Distance(transform.position, dest.position) < _collectDistance)
        {
            if (InventorySystem.Instance.AddItem(ItemStack))
            {
                Player.DropLoot?.Invoke(ItemStack);
                _hasBeenAdded = true;
                Destroy(gameObject);
            }
            else
            {
                print("Can't add item to inventory");
            }
        }
        else
        {
            if (InventorySystem.Instance.CanAddItem(ItemStack))
            {
                _rb.velocity = (dest.position - transform.position).normalized * Time.deltaTime * _magnetSpeed;
            }
        }
    }
}
