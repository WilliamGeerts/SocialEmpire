using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Shop : MonoBehaviour
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

        Debug.Log($"[Shop] {buildingPlacer.availableBuildings.Count} bâtiments à afficher.");

        foreach (var data in buildingPlacer.availableBuildings)
        {
            Debug.Log($"[Shop] Création d’un bouton pour : {data.name}");

            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);

            var iconImage = buttonObj.transform.Find("BuildingImage")?.GetComponent<Image>();
            var titleText = buttonObj.transform.Find("BuildingText")?.GetComponent<TextMeshProUGUI>();
            var button = buttonObj.GetComponent<Button>();

            if (titleText != null)
            {
                titleText.text = data.name;
            }
            else
            {
                Debug.LogWarning("[Shop] Aucun composant TextMeshProUGUI trouvé dans 'BuildingText'.");
            }

            if (iconImage != null)
            {
                var spriteRenderer = data.prefab.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    iconImage.sprite = spriteRenderer.sprite;
                }
                else
                {
                    Debug.LogWarning($"[Shop] Aucun SpriteRenderer trouvé dans le prefab de '{data.name}' !");
                }
            }
            else
            {
                Debug.LogWarning("[Shop] Aucun composant Image trouvé dans 'BuildingImage'.");
            }

            if (button != null)
            {
                string name = data.name; // capture locale
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