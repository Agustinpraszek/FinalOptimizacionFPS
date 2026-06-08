namespace Game.Core
{
    /// <summary>
    /// Cualquier objeto que necesite "actualizarse" por frame implementa esto.
    /// El UpdateManager es el ÚNICO que llama Tick(): así no usamos Update() nativo
    /// en la lógica de gameplay (consigna del parcial).
    /// </summary>
    public interface ITickable
    {
        void Tick(float deltaTime);
    }
}
