using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NebulaRenderer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite sprite {get; private set; }
    public Vector2 direction {get; private set; }

    /// <summary>
    /// </summary>
    public void Init(Sprite startingSprite, Vector2 pos)
    {
        sprite = startingSprite;

        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = sprite;

        gameObject.name = "Background_Nebula";

        transform.position = new Vector3(pos.x, pos.y, 0f);

        spriteRenderer.sortingOrder = 0;
    }

    public void ChangeNebula(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public void Move(Vector2 dir, float speed)
    {
        Vector3 immediateTarget = transform.position + (Vector3)dir * speed * Time.deltaTime;

        float smoothingFactor = 0.1f;
        transform.position = Vector3.Lerp(transform.position, immediateTarget, smoothingFactor);

        direction = dir;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
