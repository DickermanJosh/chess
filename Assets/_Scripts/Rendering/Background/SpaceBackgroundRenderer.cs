using UnityEngine;
using System.Collections.Generic;
using System;

public class SpaceBackgroundRenderer : MonoBehaviour
{
    [Header("Space Sprites")]
    [SerializeField] private Sprite aqua;
    [SerializeField] private Sprite blue;
    [SerializeField] private Sprite red;

    [Header("Star Sprites")]
    [SerializeField] private Sprite small;
    [SerializeField] private Sprite large;

    private readonly List<NebulaRenderer> activeNebulas = new();

    private Vector2 currentDirection = Vector2.zero;  
    private const float tolerance = 0.1f;

    private void Start()
    {
        // in view row
        CreateBackgroundSection(aqua, new Vector2(-16, 0)); // left
        CreateBackgroundSection(aqua, new Vector2()); // middle
        CreateBackgroundSection(aqua, new Vector2(16, 0)); // right

        // out of view row
        CreateBackgroundSection(aqua, new Vector2(-16, -16));
        CreateBackgroundSection(aqua, new Vector2(0, -16));
        CreateBackgroundSection(aqua, new Vector2(16, -16));

        currentDirection = new Vector2(0, 1);

        DontDestroyOnLoad(gameObject);
    }

   
    private void Update()
    {
        for (int i = 0; i < activeNebulas.Count; i++)
        {
            NebulaRenderer neb = activeNebulas[i];
            Vector2 pos = neb.gameObject.transform.position;

            // Aligned to destroy out of view section and create new one to feed
            if (Math.Abs(pos.y) > 16)
            {
                RemoveBackgroundSection(i);
                CreateBackgroundSection(aqua, new Vector2(-16, -16));
                CreateBackgroundSection(aqua, new Vector2(0, -16));
                CreateBackgroundSection(aqua, new Vector2(16, -16));
                continue;
            }

            MoveSection(i, currentDirection);
        }

    }

    private void MoveSection(int i, Vector2 dir)
    {
        activeNebulas[i].Move(dir, 1);
    }

    private void CreateBackgroundSection(Sprite nebulaSprite, Vector2 pos)
    {
        GameObject nebulaObj = new("Nebula");
        nebulaObj.transform.parent = transform;

        NebulaRenderer nebulaRend = nebulaObj.AddComponent<NebulaRenderer>();
        nebulaRend.Init(nebulaSprite, pos);

        GameObject smallStarObj = new("Small_Stars");
        GameObject bigStarObj = new("Big_Stars");

        smallStarObj.transform.SetParent(nebulaObj.transform, false);
        bigStarObj.transform.SetParent(nebulaObj.transform, false);

        StarRenderer smallStarRend = smallStarObj.AddComponent<StarRenderer>();
        StarRenderer bigStarRend = smallStarObj.AddComponent<StarRenderer>();
        
        smallStarRend.Init(small, pos);
        bigStarRend.Init(small, pos);

        activeNebulas.Add(nebulaRend);
    }

    private void RemoveBackgroundSection(int i)
    {
        activeNebulas[i].Destroy();
        activeNebulas.RemoveAt(i);
    }

}
