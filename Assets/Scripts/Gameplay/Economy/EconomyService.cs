using System;
using UnityEngine;

// Saldo del jugador. No sabe de dónde sale la plata ni en qué se gasta. El
// bootstrap le conecta las fuentes y la tienda le pide gastar.
public sealed class EconomyService
{
    private int _balance;

    public int Balance => _balance;

    public event Action<int> OnBalanceChanged;

    public EconomyService(int startingBalance)
    {
        _balance = Mathf.Max(0, startingBalance);
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;

        _balance += amount;
        OnBalanceChanged?.Invoke(_balance);
    }

    // Descuenta solo si alcanza. Devuelve false sin tocar el saldo si no.
    public bool TrySpend(int amount)
    {
        if (amount <= 0 || _balance < amount) return false;

        _balance -= amount;
        OnBalanceChanged?.Invoke(_balance);
        return true;
    }

    public bool CanAfford(int amount) => _balance >= amount;
}
