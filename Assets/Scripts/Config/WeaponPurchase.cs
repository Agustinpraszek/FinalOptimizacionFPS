using UnityEngine;

// Desbloquea un arma. Se puede comprar una sola vez.
[CreateAssetMenu(menuName = "Game/Purchases/Weapon", fileName = "WeaponPurchase")]
public sealed class WeaponPurchase : PurchaseData
{
    public WeaponData Weapon;

    public override bool CanPurchase(ShopContext context)
    {
        return Weapon != null && !context.Weapons.IsOwned(Weapon);
    }

    public override void Apply(ShopContext context)
    {
        context.Weapons.Unlock(Weapon);
    }

    public override string GetUnavailableReason(ShopContext context)
    {
        return Weapon == null ? "Not Available" : "Already Bought";
    }
}
