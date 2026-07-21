using UnityEngine;

// Game loop del proyecto y único Update() propio.
// Maneja registro de sistemas, frecuencia, orden de ejecución y pausa.
// El orden lo define GameBootstrap con el orden en que registra cada sistema.
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

        // Always no se frena nunca: acá viven el flujo de partida y el input de menús.
        _always.Tick(deltaTime);

        if (IsPaused) return;

        _gameplay.Tick(deltaTime);

        _slowAccumulator += deltaTime;
        if (_slowAccumulator < SlowChannelInterval) return;

        // Se pasa el acumulado y no el delta del frame, para que la lógica lenta
        // no quede atada al framerate.
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
