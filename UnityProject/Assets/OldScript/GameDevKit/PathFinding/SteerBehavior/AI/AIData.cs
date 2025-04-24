using System.Collections.Generic;
using UnityEngine;

public class AIData : MonoBehaviour
{
    [HideInInspector]
    public List<Transform> targets = null;
    [HideInInspector]
    public Collider2D[] obstacles = null;
    [HideInInspector]
    public Transform currentTarget;

    public int GetTargetsCount() => targets == null ? 0 : targets.Count;

    private void Awake()
    {
    }
}
