using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Настройки предмета")]
    public string itemName = "Яблоко";
    public Sprite itemIcon;
    public GameObject itemPrefab;  // Ссылка на префаб для рук
    
    public float rotationSpeed = 50f;
    
    private Transform player;
    private Inventory inventory;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            inventory = playerObj.GetComponent<Inventory>();
        }
        
        // Диагностика
        if (itemPrefab == null)
        {
            Debug.LogWarning($"У предмета {itemName} нет префаба! Использую этот же объект");
            itemPrefab = gameObject;
        }
    }
    
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        if (player == null || inventory == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        
        if (distance < 2f && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }
    
    void Pickup()
    {
        if (itemPrefab == null)
        {
            Debug.LogError($"У {itemName} нет префаба! Заполни поле Item Prefab");
            return;
        }
        
        Inventory.InventoryItem newItem = new Inventory.InventoryItem();
        newItem.itemName = itemName;
        newItem.icon = itemIcon;
        newItem.itemPrefab = itemPrefab;
        
        if (inventory.AddItemDirect(newItem))
        {
            Debug.Log($"Подобран {itemName} с префабом {itemPrefab.name}");
            Destroy(gameObject);
        }
    }
    
    void OnGUI()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < 2f)
        {
            GUI.Box(new Rect(Screen.width/2 - 100, Screen.height - 80, 200, 25), $"Нажми E чтобы взять {itemName}");
        }
    }
}