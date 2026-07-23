using UnityEngine;

// Game loop del juego y único Update() del proyecto.
// Registra sistemas, controla frecuencia, orden y pausa.
// El orden de ejecución sale del orden en que los registra GameBootstrap.
public sealed class UpdateManager : MonoBehaviour
{
    private const float SlowChannelInterval = 0.2f;

    private readonly UpdateGroup _always = new UpdateGroup(16);
    private readonly UpdateGroup _gameplay = new UpdateGroup(256);
    private readonly UpdateGroup _slow = new UpdateGroup(64);

    private float _slowAccumulator;

    public bool IsPaused { get; private set; }

    public void Register(IUpdatable updatable, UpdateChannel channel = UpdateChannel.Gameplay)
    {
        GroupOf(channel).Add(updatable);
    }

    public void Unregister(IUpdatable updatable, UpdateChannel channel = UpdateChannel.Gameplay)
    {
        GroupOf(channel).Remove(updatable);
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // Always nunca se frena. Acá va el flujo de partida y el input de menús.
        _always.Tick(deltaTime);

        if (IsPaused) return;

        _gameplay.Tick(deltaTime);

        _slowAccumulator += deltaTime;
        if (_slowAccumulator < SlowChannelInterval) return;

        // Le paso el tiempo acumulado y no el delta del frame, así no depende del framerate.
        _slow.Tick(_slowAccumulator);
        _slowAccumulator = 0f;
    }

    private UpdateGroup GroupOf(UpdateChannel channel)
    {
        switch (channel)
        {
            case UpdateChannel.Always: return _always;
            case UpdateChannel.Slow: return _slow;
            default: return _gameplay;
        }
    }
}
