using System;
using UnityEngine;

[Serializable]
public sealed class ShopSettings
{
    [Tooltip("A qué distancia se puede comprar.")]
    [Min(0.5f)] public float InteractDistance = 3.5f;

    [Tooltip("Capas que el rayo de compra puede tocar. Incluí los puestos y la " +
             "geometría del mapa: así una pared en el medio bloquea la compra.")]
    public LayerMask InteractMask = ~0;
}
