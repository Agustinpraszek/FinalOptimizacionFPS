using System.Collections.Generic;

// Lista de updatables con alta y baja diferida, para no tocar la colección
// mientras se la está recorriendo.
public sealed class UpdateGroup
{
    private readonly List<IUpdatable> _items;
    private readonly List<IUpdatable> _toAdd = new List<IUpdatable>(32);
    private readonly List<IUpdatable> _toRemove = new List<IUpdatable>(32);

    public UpdateGroup(int capacity)
    {
        _items = new List<IUpdatable>(capacity);
    }

    public void Add(IUpdatable item)
    {
        if (item != null) _toAdd.Add(item);
    }

    public void Remove(IUpdatable item)
    {
        if (item != null) _toRemove.Add(item);
    }

    public void Tick(float deltaTime)
    {
        Flush();

        for (int i = 0; i < _items.Count; i++)
            _items[i].Tick(deltaTime);
    }

    public void Clear()
    {
        _items.Clear();
        _toAdd.Clear();
        _toRemove.Clear();
    }

    private void Flush()
    {
        // Una baja primero cancela un alta pendiente del mismo frame, y recién
        // ahí toca la lista viva. Así "alta + baja" no deja basura y
        // "baja + alta" (reuso de pool) no duplica ni pierde el registro.
        if (_toRemove.Count > 0)
        {
            for (int i = 0; i < _toRemove.Count; i++)
            {
                IUpdatable item = _toRemove[i];
                if (!_toAdd.Remove(item)) _items.Remove(item);
            }

            _toRemove.Clear();
        }

        if (_toAdd.Count > 0)
        {
            for (int i = 0; i < _toAdd.Count; i++)
                _items.Add(_toAdd[i]);

            _toAdd.Clear();
        }
    }
}
