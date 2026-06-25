using System.Collections.Generic;
using UnityEngine;


public sealed class UpdateManager : MonoBehaviour
{
    // El único Update() del juego, ejecuta el tick de todos los codigos registrados
    // Add/Remove son diferidos para no modificar la lista mientras se itera

    private readonly List<ITickable> _tickables = new List<ITickable>(256);
    private readonly List<ITickable> _toAdd     = new List<ITickable>(64);
    private readonly List<ITickable> _toRemove  = new List<ITickable>(64);

    public void Register(ITickable tickable)
    {
        if (tickable != null) _toAdd.Add(tickable);
    }

    public void Unregister(ITickable tickable)
    {
        if (tickable != null) _toRemove.Add(tickable);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        FlushPending();

        for (int i = 0; i < _tickables.Count; i++)
            _tickables[i].Tick(deltaTime);
    }

    private void FlushPending()
    {
        if (_toAdd.Count > 0)
        {
            for (int i = 0; i < _toAdd.Count; i++)
            {
                _tickables.Add(_toAdd[i]);
            }
            _toAdd.Clear();
        }

        if (_toRemove.Count > 0)
        {
            for (int i = 0; i < _toRemove.Count; i++)
            {
                _tickables.Remove(_toRemove[i]);
            }
            _toRemove.Clear();
        }
    }
}
