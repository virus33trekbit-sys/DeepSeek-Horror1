using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Настройки инвентаря")]
    public int maxSlots = 3;
    public GameObject inventoryPanel;
    
    [Header("Отображение предмета в руке")]
    public Vector3 handOffset = new Vector3(0.79f, -0.27f, 1.1f);
    
    [Header("Бросание предмета")]
    public float throwForce = 5f;
    public float throwUpwardForce = 2f;
    public float throwDistance = 1.5f;
    
    [Header("Цвета UI")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    
    private List<InventoryItem> items = new List<InventoryItem>();
    private Image[] slotImages;
    private Text[] slotTexts;
    private GameObject[] slotBackgrounds;
    
    private int selectedSlot = -1;
    private GameObject currentItemInHand;
    private Camera playerCamera;
    private Transform handTransform;
    
    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        CreateHandTransform();
        
        if (inventoryPanel == null)
            CreateInventoryUI();
        
        SetupUI();
    }
    
    void CreateHandTransform()
    {
        GameObject hand = new GameObject("HandPosition");
        hand.transform.SetParent(playerCamera.transform);
        hand.transform.localPosition = handOffset;
        hand.transform.localRotation = Quaternion.identity;
        handTransform = hand.transform;
    }
    
    void CreateInventoryUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = inventoryPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 10);
        panelRect.sizeDelta = new Vector2(300, 80);
        
        Image panelBg = inventoryPanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.7f);
    }
    
    void SetupUI()
    {
        if (inventoryPanel == null) return;
        
        slotImages = new Image[maxSlots];
        slotTexts = new Text[maxSlots];
        slotBackgrounds = new GameObject[maxSlots];
        
        for (int i = 0; i < maxSlots; i++)
        {
            int slotIndex = i;
            
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(inventoryPanel.transform, false);
            
            RectTransform slotRect = slot.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0, 0.5f);
            slotRect.anchorMax = new Vector2(0, 0.5f);
            slotRect.pivot = new Vector2(0, 0.5f);
            slotRect.anchoredPosition = new Vector2(10 + i * 100, 0);
            slotRect.sizeDelta = new Vector2(80, 70);
            
            Image slotBg = slot.AddComponent<Image>();
            slotBg.color = normalColor;
            slotBackgrounds[i] = slot;
            
            Button button = slot.AddComponent<Button>();
            button.onClick.AddListener(() => SelectSlot(slotIndex));
            
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slot.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(50, 50);
            iconRect.anchoredPosition = new Vector2(0, 5);
            
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = new Color(1, 1, 1, 0);
            slotImages[i] = iconImage;
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(slot.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0);
            textRect.anchorMax = new Vector2(0.5f, 0);
            textRect.sizeDelta = new Vector2(70, 20);
            textRect.anchoredPosition = new Vector2(0, -5);
            
            Text itemText = textObj.AddComponent<Text>();
            itemText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            itemText.fontSize = 12;
            itemText.alignment = TextAnchor.MiddleCenter;
            itemText.color = Color.white;
            itemText.text = "";
            slotTexts[i] = itemText;
        }
    }
    
    void Update()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            ThrowCurrentItem();
        }
    }
    
    void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots) return;
        
        if (selectedSlot == slotIndex)
        {
            RemoveItemFromHand();
            selectedSlot = -1;
            UpdateUI();
            return;
        }
        
        RemoveItemFromHand();
        selectedSlot = slotIndex;
        
        if (slotIndex < items.Count)
        {
            ShowItemInHand(slotIndex);
        }
        
        UpdateUI();
    }
    
    void ShowItemInHand(int slotIndex)
    {
        if (slotIndex >= items.Count) return;
        
        InventoryItem item = items[slotIndex];
        if (item == null || item.itemPrefab == null) return;
        
        currentItemInHand = Instantiate(item.itemPrefab, handTransform.position, handTransform.rotation);
        currentItemInHand.transform.SetParent(handTransform);
        currentItemInHand.transform.localPosition = Vector3.zero;
        currentItemInHand.transform.localRotation = Quaternion.identity;
        
        Collider col = currentItemInHand.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        Rigidbody rb = currentItemInHand.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }
    
    void RemoveItemFromHand()
    {
        if (currentItemInHand != null)
        {
            Destroy(currentItemInHand);
            currentItemInHand = null;
        }
    }
    
    void ThrowCurrentItem()
    {
        if (selectedSlot < 0 || selectedSlot >= items.Count)
        {
            Debug.Log("Нет предмета в руке");
            return;
        }
        
        InventoryItem item = items[selectedSlot];
        if (item == null || item.itemPrefab == null)
        {
            Debug.LogError("Нет префаба для броска! Убедись что в ItemPickup заполнен Item Prefab");
            return;
        }
        
        Debug.Log($"Бросаем {item.itemName} с префабом {item.itemPrefab.name}");
        
        Vector3 throwPosition = transform.position + transform.forward * throwDistance + Vector3.up * 0.5f;
        
        GameObject thrownItem = Instantiate(item.itemPrefab, throwPosition, Quaternion.identity);
        
        Collider col = thrownItem.GetComponent<Collider>();
        if (col != null) col.enabled = true;
        
        Rigidbody rb = thrownItem.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = thrownItem.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.useGravity = true;
        
        Vector3 throwDirection = transform.forward + Vector3.up * 0.3f;
        rb.AddForce(throwDirection * throwForce + Vector3.up * throwUpwardForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        
        // ========== ГЛАВНОЕ: СОХРАНЯЕМ ПРЕФАБ ПРИ БРОСКЕ ==========
        ItemPickup pickup = thrownItem.GetComponent<ItemPickup>();
        if (pickup == null)
        {
            pickup = thrownItem.AddComponent<ItemPickup>();
        }
        pickup.itemName = item.itemName;
        pickup.itemIcon = item.icon;
        pickup.itemPrefab = item.itemPrefab;  // ← СОХРАНЯЕМ ССЫЛКУ НА ПРЕФАБ!
        
        Debug.Log($"Предмет выброшен, префаб сохранён: {pickup.itemPrefab != null}");
        
        items.RemoveAt(selectedSlot);
        RemoveItemFromHand();
        selectedSlot = -1;
        UpdateUI();
    }
    
    public bool AddItemDirect(InventoryItem item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Инвентарь полон");
            return false;
        }
        
        items.Add(item);
        UpdateUI();
        Debug.Log($"Добавлен {item.itemName} в инвентарь. Префаб: {item.itemPrefab != null}");
        return true;
    }
    
    void UpdateUI()
    {
        if (slotImages == null) return;
        
        for (int i = 0; i < maxSlots; i++)
        {
            if (i < items.Count)
            {
                if (slotImages[i] != null)
                {
                    if (items[i].icon != null)
                        slotImages[i].sprite = items[i].icon;
                    slotImages[i].color = Color.white;
                }
                
                if (slotTexts[i] != null)
                    slotTexts[i].text = items[i].itemName;
                
                if (slotBackgrounds[i] != null)
                {
                    Color bgColor = (selectedSlot == i) ? selectedColor : normalColor;
                    slotBackgrounds[i].GetComponent<Image>().color = bgColor;
                }
            }
            else
            {
                if (slotImages[i] != null)
                {
                    slotImages[i].sprite = null;
                    slotImages[i].color = new Color(1, 1, 1, 0);
                }
                
                if (slotTexts[i] != null)
                    slotTexts[i].text = "";
                
                if (slotBackgrounds[i] != null)
                    slotBackgrounds[i].GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);
            }
        }
    }
    
    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public Sprite icon;
        public GameObject itemPrefab;
    }
}