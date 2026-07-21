using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Compras en el mundo, estilo wall buy: apuntás a un puesto y comprás con F.
// No sabe qué se está comprando: delega en PurchaseData, así sumar tipos de
// compra no lo obliga a cambiar.
public sealed class ShopSystem : IUpdatable
{
    private const string BuyPrompt = "[F] Buy";
    private const string NotEnoughMoneyPrompt = "Not Enough Money";

    private readonly Transform _aim;
    private readonly IReadOnlyList<BuyStationSetup> _stations;
    private readonly EconomyService _economy;
    private readonly ShopContext _context;
    private readonly float _interactDistance;
    private readonly LayerMask _interactMask;

    // Último estado escrito en cada cartel, para reescribirlo solo cuando cambia.
    private readonly bool[] _labelState;

    private BuyStationSetup _focused;
    private bool _lastCanPurchase;
    private bool _lastCanAfford;

    // Texto de acción para el HUD. Vacío cuando no estás apuntando a nada.
    public event Action<string> OnPromptChanged;

    public event Action<PurchaseData> OnPurchased;

    public ShopSystem(
        Transform aim,
        IReadOnlyList<BuyStationSetup> stations,
        EconomyService economy,
        ShopContext context,
        ShopSettings settings)
    {
        _aim = aim;
        _stations = stations;
        _economy = economy;
        _context = context;
        _interactDistance = settings.InteractDistance;
        _interactMask = settings.InteractMask;

        _labelState = new bool[stations.Count];
        RefreshLabels(true);
    }

    public void Tick(float deltaTime)
    {
        RefreshLabels(false);
        RefreshFocus(FindStationInSight());

        if (_focused == null) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.fKey.wasPressedThisFrame) TryPurchase();
    }

    private BuyStationSetup FindStationInSight()
    {
        // Un solo raycast por frame. Al tomar el primer impacto, una pared en el
        // medio bloquea la compra sin lógica extra.
        if (!Physics.Raycast(_aim.position, _aim.forward, out RaycastHit hit,
                _interactDistance, _interactMask, QueryTriggerInteraction.Collide))
        {
            return null;
        }

        for (int i = 0; i < _stations.Count; i++)
        {
            BuyStationSetup station = _stations[i];
            if (station.IsValid && station.Collider == hit.collider) return station;
        }

        return null;
    }

    // Solo arma el string cuando cambia algo. En reposo no allocá nada.
    private void RefreshFocus(BuyStationSetup station)
    {
        if (station == null)
        {
            if (_focused == null) return;

            _focused = null;
            OnPromptChanged?.Invoke(string.Empty);
            return;
        }

        bool canPurchase = station.Purchase.CanPurchase(_context);
        bool canAfford = _economy.CanAfford(station.Purchase.Cost);

        bool unchanged = station == _focused
            && canPurchase == _lastCanPurchase
            && canAfford == _lastCanAfford;

        if (unchanged) return;

        _focused = station;
        _lastCanPurchase = canPurchase;
        _lastCanAfford = canAfford;
        OnPromptChanged?.Invoke(BuildPrompt(station.Purchase, canPurchase, canAfford));
    }

    // El prompt solo dice la acción o el motivo. El nombre y el precio ya están
    // en el cartel del puesto, no hace falta repetirlos.
    private string BuildPrompt(PurchaseData purchase, bool canPurchase, bool canAfford)
    {
        if (!canPurchase) return purchase.GetUnavailableReason(_context);

        return canAfford ? BuyPrompt : NotEnoughMoneyPrompt;
    }

    private void TryPurchase()
    {
        PurchaseData purchase = _focused.Purchase;

        if (!purchase.CanPurchase(_context)) return;
        if (!_economy.TrySpend(purchase.Cost)) return;

        purchase.Apply(_context);
        OnPurchased?.Invoke(purchase);

        // Se fuerza el rearmado del prompt porque el estado cambió.
        _focused = null;
        RefreshFocus(FindStationInSight());
    }

    // Se chequea todos los frames pero solo se reescribe el TMP cuando el estado
    // cambió de verdad. Escribir un texto en world space dispara un rebuild del
    // canvas, así que no conviene hacerlo por frame.
    // El chequeo importa para la cura, que se habilita sola al recibir daño.
    private void RefreshLabels(bool force)
    {
        for (int i = 0; i < _stations.Count; i++)
        {
            BuyStationSetup station = _stations[i];
            if (!station.IsValid || station.Label == null) continue;

            PurchaseData purchase = station.Purchase;
            bool canPurchase = purchase.CanPurchase(_context);

            if (!force && canPurchase == _labelState[i]) continue;
            _labelState[i] = canPurchase;

            // El "Buy" es parte de la plantilla, no del nombre: cuando ya no se
            // puede comprar queda solo el ítem y el motivo.
            station.Label.text = canPurchase
                ? $"Buy {purchase.DisplayName}\n${purchase.Cost}"
                : $"{purchase.DisplayName}\n{purchase.GetUnavailableReason(_context)}";
        }
    }
}
