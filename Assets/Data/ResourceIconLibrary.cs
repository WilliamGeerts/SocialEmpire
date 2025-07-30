using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceIconLibrary", menuName = "UI/Resource Icon Library")]
public class ResourceIconLibrary : ScriptableObject
{
    [System.Serializable]
    public class ResourceIcon
    {
        public string resourceType;
        public Sprite icon;
    }

    public List<ResourceIcon> icons = new();

    private Dictionary<string, Sprite> iconLookup;

    public Sprite GetIcon(string resourceType)
    {
        if (iconLookup == null)
        {
            iconLookup = new Dictionary<string, Sprite>();
            foreach (var icon in icons)
            {
                if (!iconLookup.ContainsKey(icon.resourceType))
                {
                    iconLookup.Add(icon.resourceType, icon.icon);
                }
            }
        }

        iconLookup.TryGetValue(resourceType, out var sprite);
        return sprite;
    }
}