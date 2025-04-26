using System;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace GameDevKit
{
    public class DebugWordUtil
    {
         // Create Text in the World
        public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 15, Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 0) {
            if (color == null) color = Color.white;
            return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
        }
        public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder) {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return textMesh;
        }
        
        // Create a Sprite in the World, no parent
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position">位置</param>
        /// <param name="size">大小</param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static GameObject CreateWorldSprite(string name, 
            Vector3 position, Vector3 size, Color color)
        {
            return CreateWorldSprite(null, name,GetDefaultSprite(), position, size, color);
        }
        
        // Create a Sprite in the World
        public static GameObject CreateWorldSprite(Transform parent, string name, Sprite sprite, Vector3 localPosition, Vector3 localsize,  Color color) {
            GameObject gameObject = new GameObject(name, typeof(SpriteRenderer));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            transform.localScale = localsize;

            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            
            //spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.color = color;
            return gameObject;
        }

        public static Texture2D GetDefaultTexture()
        {
            //没文档,找不到还是用AssetDataBase把
           //return Resources.GetBuiltinResource<Texture2D>("Square.png");
          return AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Square.png");
        }
        public static Sprite GetDefaultSprite()
        {
            //没文档,找不到还是用AssetDataBase把
            //return Resources.GetBuiltinResource<Texture2D>("Square.png");
            return AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Square.png");
        }
    }
    
}