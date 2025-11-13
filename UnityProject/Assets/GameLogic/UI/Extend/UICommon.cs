using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;

public static class UICommon
{
    public static void SetSprite(this Image obj, string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            Log.Warning("Sprite name is null or empty.");
            return;
        }

        Sprite sprite = GameModule.Resource.LoadAsset<Sprite>(spriteName);
        if (sprite == null)
        {
            Log.Warning("Sprite not found: " + spriteName);
            return;
        }

        obj.sprite = sprite;
    }
}