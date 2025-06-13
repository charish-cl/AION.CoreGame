using UnityEngine;

namespace GameDevKit
{
    public static class SpriteExtend
    {
        public static void SetColor(this GameObject sprite, Color color)
        {
            sprite.GetComponent<SpriteRenderer>().color = color;
        }
        public static void SetSprite(this GameObject go, Sprite sprite)
        {
            go.GetComponent<SpriteRenderer>().sprite = sprite;
        }
        /// <summary>
        /// 通过包围盒 获取到Rect
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public static FRect GetFRect(this SpriteRenderer sprite)
        {
           var bounds= sprite.bounds;
           
           return new FRect(bounds.center, bounds.size.x, bounds.size.y);
        }
        
        public static Texture2D SpriteToTexture2d(this Sprite sprite)
        {
            //t2d为待转换的Texture2D对象
            //sprite为图集中的某个子Sprite对象
            var targetTex = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
            targetTex = duplicateTexture(targetTex);
            var pixels = sprite.texture.GetPixels(
                (int) sprite.textureRect.x,
                (int) sprite.textureRect.y,
                (int) sprite.textureRect.width,
                (int) sprite.textureRect.height);
            targetTex.SetPixels(pixels);
            targetTex.Apply();
            return targetTex;
        }

        private static Texture2D duplicateTexture(this Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        /// <summary>
        /// 通过Texture加载Sprite
        /// </summary>
        /// <param name="t2d"></param>
        /// <returns></returns>
        // public static Sprite[] GetTextureSprite(this Texture2D t2d)
        // {
        //    return ResourcesKit.ResLoadAll<Sprite>(t2d.GetResourcesPath());
        // }
        
        public static Sprite Texture2dToSprite(this Texture2D t2d)
        {
            //t2d为待转换的Texture2D对象
            //保持原有大小,这里的长度宽度都是像素大小比如256,256
            return Sprite.Create(t2d,
                new Rect(0, 0, t2d.width, t2d.height),
                new Vector2(0.5f, 0.5f));
        }
    }
}