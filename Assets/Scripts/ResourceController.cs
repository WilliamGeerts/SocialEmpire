using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ResourceDisplayConfig
{
    public string resourceType;
    public int maxAmount;
    public Sprite icon;
}

public class ResourceController : MonoBehaviour
{
    [Header("Prefab & Container")]
    public GameObject resourceItemPrefab;
    public Transform container;

    [Header("Configuration manuelle des ressources à afficher")]
    public List<ResourceDisplayConfig> displayedResources;

    private Dictionary<string, ResourceUI> activeUIs = new();

    public void InitUI(List<ResourceData> inventory)
    {
        // On crée un dictionnaire avec les quantités chargées depuis la sauvegarde
        Dictionary<string, int> inventoryLookup = new();
        foreach (var data in inventory)
            inventoryLookup[data.resourceType] = data.amount;

        // Ensuite, on instancie SEULEMENT les ressources configurées manuellement
        foreach (var config in displayedResources)
        {
            GameObject go = Instantiate(resourceItemPrefab, container);

            var ui = new ResourceUI
            {
                resourceType = config.resourceType,
                icon = go.transform.Find("ResourceIcon").GetComponent<Image>(),
                gauge = go.transform.Find("ResourceAmount/Slider").GetComponent<Slider>(),
                amountText = go.transform.Find("ResourceAmount/ResourceText").GetComponent<TMP_Text>()
            };

            int currentAmount = inventoryLookup.ContainsKey(config.resourceType)
                ? inventoryLookup[config.resourceType]
                : 0;

            ui.icon.sprite = config.icon;
            ui.gauge.maxValue = config.maxAmount;
            ui.gauge.value = currentAmount;
            ui.amountText.text = currentAmount.ToString();

            activeUIs[config.resourceType] = ui;
        }
    }

    public void UpdateResource(string type, int newAmount)
    {
        if (activeUIs.TryGetValue(type, out var ui))
        {
            ui.gauge.value = newAmount;
            ui.amountText.text = newAmount.ToString();
        }
    }
}