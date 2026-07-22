// Lo implementan los sistemas y entidades con lógica por frame.
// El UpdateManager es el único que llama Tick().
public interface IUpdatable
{
    void Tick(float deltaTime);
}
