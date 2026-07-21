using UnityEngine;

// Cura un porcentaje de la vida máxima. Se puede comprar todas las veces que
// haga falta, mientras no estés al full.
[CreateAssetMenu(menuName = "Game/Purchases/Heal", fileName = "HealPurchase")]
public sealed class HealPurchase : PurchaseData
{
    [Range(1, 100)] public int HealPercent = 25;

    public override bool CanPurchase(ShopContext context)
    {
        PlayerLogic player = context.Player;
        return player.IsAlive && player.Health < player.MaxHealth;
    }

    public override void Apply(ShopContext context)
    {
        PlayerLogic player = context.Player;
        int amount = Mathf.CeilToInt(player.MaxHealth * (HealPercent / 100f));
        player.Heal(amount);
    }

    public override string GetUnavailableReason(ShopContext context)
    {
        return "Full Health";
    }
}
