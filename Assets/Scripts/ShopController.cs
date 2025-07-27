using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopController : MonoBehaviour
{
    public GameObject shopPanel;
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    public BuildingPlacer buildingPlacer;

    void Start()
    {
        Debug.Log("[Shop] Start() called");

        if (buildingPlacer == null)
        {
            Debug.LogError("[Shop] buildingPlacer is NULL! Assure-toi de l’assigner dans l’inspecteur.");
            return;
        }

        if (buttonPrefab == null)
        {
            Debug.LogError("[Shop] buttonPrefab is NULL! Assure-toi de l’assigner dans l’inspecteur.");
            return;
        }

        if (buttonContainer == null)
        {
            Debug.LogError("[Shop] buttonContainer is NULL! Assure-toi de l’assigner dans l’inspecteur.");
            return;
        }

        Debug.Log($"[Shop] {buildingPlacer.availableBuildingPrefabs.Count} bâtiments à afficher.");

        foreach (var prefab in buildingPlacer.availableBuildingPrefabs)
        {
            BuildingInstance instance = prefab.GetComponent<BuildingInstance>();
            if (instance == null)
            {
                Debug.LogWarning($"[Shop] Le prefab '{prefab.name}' n'a pas de BuildingInstance.");
                continue;
            }

            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);

            var iconImage = buttonObj.transform.Find("BuildingImage")?.GetComponent<Image>();
            var titleText = buttonObj.transform.Find("BuildingText")?.GetComponent<TextMeshProUGUI>();
            var button = buttonObj.GetComponent<Button>();

            if (titleText != null)
                titleText.text = instance.buildingName;
            else
                Debug.LogWarning("[Shop] Aucun composant TextMeshProUGUI trouvé dans 'BuildingText'.");

            if (iconImage != null)
            {
                var spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                    iconImage.sprite = spriteRenderer.sprite;
                else
                    Debug.LogWarning($"[Shop] Aucun SpriteRenderer trouvé dans le prefab '{prefab.name}' !");
            }
            else
            {
                Debug.LogWarning("[Shop] Aucun composant Image trouvé dans 'BuildingImage'.");
            }

            if (button != null)
            {
                string name = prefab.name;
                button.onClick.AddListener(() => {
                    Debug.Log($"[Shop] Bouton '{name}' cliqué.");
                    buildingPlacer.StartPlacingBuildingByName(name);
                });
            }
            else
            {
                Debug.LogError("[Shop] Le prefab du bouton n’a pas de composant Button !");
            }
        }

        Debug.Log("[Shop] Tous les boutons ont été créés.");
    }

    public void ToggleShop()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
    }
}