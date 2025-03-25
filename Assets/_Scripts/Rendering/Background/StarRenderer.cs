using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class StarRenderer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sprite sprite;

    /// <summary>
    /// </summary>
    public void Init(Sprite startingSprite, Vector2 pos)
    {
        sprite = startingSprite;

        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = sprite;

        gameObject.name = "Background_Stars";

        transform.position = new Vector3(pos.x, pos.y, 0f);

        int rand = Random.Range(0, 3);
        float angle = 0f;
        switch (rand)
        {
            case 0:
                angle = 90f;
                break;
            case 1:
                angle = 180f;
                break;
            case 2:
                angle = 270f;
                break;
            case 3:
                angle = 0f;
                break;

        }

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        spriteRenderer.sortingOrder = 1;
    }

    public void Move(Vector2 dir, float speed)
    {
        Vector3 immediateTarget = transform.position + (Vector3)dir * speed * Time.deltaTime;

        float smoothingFactor = 0.1f;
        transform.position = Vector3.Lerp(transform.position, immediateTarget, smoothingFactor);

        }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
