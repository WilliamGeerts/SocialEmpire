using UnityEngine;
using UnityEngine.UI;

public class CollectButtonHandler : MonoBehaviour
{
    private BuildingInstance buildingInstance;

    public void Initialize(BuildingInstance instance)
    {
        buildingInstance = instance;
        GetComponent<Button>().onClick.AddListener(OnCollect);
    }

    private void OnCollect()
    {
        Debug.Log("[CollectButtonHandler] Bouton cliqué");
        if (buildingInstance != null)
        {
            Debug.Log($"[CollectButtonHandler] Instance trouvée : {buildingInstance.buildingName}");
            GameController controller = FindFirstObjectByType<GameController>();
            if (controller != null)
            {
                controller.CollectResources(buildingInstance);
            }
            else
            {
                Debug.LogError("[CollectButtonHandler] GameController introuvable !");
            }
        }
        else
        {
            Debug.LogWarning("[CollectButtonHandler] buildingInstance est null !");
        }
    }
}