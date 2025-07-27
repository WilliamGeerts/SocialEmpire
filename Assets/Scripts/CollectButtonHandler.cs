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
        if (buildingInstance != null)
        {
            GameController controller = FindFirstObjectByType<GameController>();
            controller?.CollectResources(buildingInstance);
        }
    }
}