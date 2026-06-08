using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// MonoBehaviour #1 (de 3 permitidos).
    /// Custom Update Manager: contiene el ÚNICO Update() del juego.
    /// No tiene lógica de gameplay: solo distribuye el tick a los ITickable registrados.
    ///
    /// Reglas que cumple:
    ///  - No es Singleton: el Bootstrap lo crea y reparte la referencia por inyección.
    ///  - Add/Remove diferidos: nunca modifica la lista mientras la recorre (evita bugs/leaks).
    /// </summary>
    public sealed class UpdateManager : MonoBehaviour
    {
        readonly List<ITickable> _tickables = new List<ITickable>(256);
        readonly List<ITickable> _toAdd = new List<ITickable>(64);
        readonly List<ITickable> _toRemove = new List<ITickable>(64);

        public void Register(ITickable tickable)
        {
            if (tickable != null)
                _toAdd.Add(tickable);
        }

        public void Unregister(ITickable tickable)
        {
            if (tickable != null)
                _toRemove.Add(tickable);
        }

        // El único Update() permitido: el del manager. No contiene lógica de juego.
        void Update()
        {
            float deltaTime = Time.deltaTime;

            FlushPending();

            // for indexado (no foreach) para evitar asignaciones del enumerador.
            for (int i = 0; i < _tickables.Count; i++)
                _tickables[i].Tick(deltaTime);
        }

        void FlushPending()
        {
            if (_toAdd.Count > 0)
            {
                for (int i = 0; i < _toAdd.Count; i++)
                    _tickables.Add(_toAdd[i]);
                _toAdd.Clear();
            }

            if (_toRemove.Count > 0)
            {
                for (int i = 0; i < _toRemove.Count; i++)
                    _tickables.Remove(_toRemove[i]);
                _toRemove.Clear();
            }
        }
    }
}
