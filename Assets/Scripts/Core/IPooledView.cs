using UnityEngine;

// Lo mínimo para vivir en un Pool: exponer la vista que el pool prende y apaga.
public interface IPooledView
{
    GameObject GameObject { get; }
}
