using UnityEngine;

// Base de todo lo comprable. Agregar un tipo de compra nuevo (más vida máxima,
// daño doble, munición) es heredar de acá y crear el asset: el ShopSystem no
// se entera.
public abstract class PurchaseData : ScriptableObject
{
    [Tooltip("Solo el nombre del ítem, sin verbo. El 'Buy' lo agrega la tienda " +
             "cuando corresponde. Ej: 'Shotgun', no 'Buy Shotgun'.")]
    public string DisplayName = "Item";

    [Min(0)] public int Cost = 100;

    // False cuando ya no tiene sentido comprarlo (arma que ya tenés, vida llena).
    public abstract bool CanPurchase(ShopContext context);

    public abstract void Apply(ShopContext context);

    // Motivo por el que no se puede comprar, para mostrar en pantalla.
    public virtual string GetUnavailableReason(ShopContext context) => "Unavailable";
}
