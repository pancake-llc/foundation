namespace Pancake
{
    using UnityEngine;

    public static partial class C
    {
        /// <summary>
        /// Calculate normalized texturerect of a sprite (0->1)
        /// </summary>
        public static Rect LocalTextureRect(this Sprite sprite)
        {
            var texturePosition = sprite.textureRect.position;
            var textureSize = sprite.textureRect.size;
            texturePosition.x /= sprite.texture.width;
            texturePosition.y /= sprite.texture.height;
            textureSize.x /= sprite.texture.width;
            textureSize.y /= sprite.texture.height;
            return new Rect(texturePosition, textureSize);
        }

        /// <summary>
        /// Calculate object size of a sprite. Which is equal to the default size of a spriteRenderer with the sprite
        /// </summary>
        public static Vector2 ObjectSize(this Sprite sprite) { return sprite.rect.size / sprite.pixelsPerUnit; }
    }
}