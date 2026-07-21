// Lo que una compra puede tocar. Se lo pasa el ShopSystem a cada PurchaseData,
// así los assets de compra no necesitan referencias propias ni buscar nada.
public sealed class ShopContext
{
    public readonly WeaponSystem Weapons;
    public readonly PlayerLogic Player;

    public ShopContext(WeaponSystem weapons, PlayerLogic player)
    {
        Weapons = weapons;
        Player = player;
    }
}
