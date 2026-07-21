using UnityEngine;

// Vida de un enemigo, por composición: cualquier IEnemy la usa sin heredar nada.
public sealed class EnemyHealth
{
    private int _max;
    private int _current;

    public int Current => _current;
    public int Max => _max;
    public bool IsAlive => _current > 0;
    public float Normalized => _max > 0 ? (float)_current / _max : 0f;

    // Se llama en cada spawn porque la instancia viene reusada del pool.
    public void Reset(int maxHealth)
    {
        _max = Mathf.Max(1, maxHealth);
        _current = _max;
    }

    // Devuelve true solo en el golpe que mata, para disparar la muerte una sola vez.
    public bool TakeDamage(int amount)
    {
        if (amount <= 0 || _current <= 0) return false;

        _current -= amount;
        if (_current > 0) return false;

        _current = 0;
        return true;
    }

    public void Kill()
    {
        _current = 0;
    }
}
