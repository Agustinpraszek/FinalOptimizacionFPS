using System;
using TMPro;
using UnityEngine;

// Un puesto de compra en el mundo: el collider al que hay que apuntar y qué vende.
// El cartel es opcional; si no lo asignás solo se usa el prompt del HUD.
[Serializable]
public sealed class BuyStationSetup
{
    [Tooltip("Collider al que hay que apuntar. Conviene que sea trigger, así no " +
             "frena al jugador ni las balas.")]
    [SerializeField] private Collider _collider;

    [SerializeField] private PurchaseData _purchase;

    [Tooltip("Cartel en world space. Opcional.")]
    [SerializeField] private TMP_Text _label;

    public Collider Collider => _collider;
    public PurchaseData Purchase => _purchase;
    public TMP_Text Label => _label;

    public bool IsValid => _collider != null && _purchase != null;
}
