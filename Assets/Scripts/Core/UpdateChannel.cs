// Define cada cuánto corre un sistema y si la pausa lo afecta.
public enum UpdateChannel
{
    // Lógica de juego. Cada frame, se frena al pausar.
    Gameplay = 0,

    // Lógica que no necesita correr cada frame. También se frena al pausar.
    Slow = 1,

    // Sigue vivo con el juego pausado: flujo de partida e input de menús.
    Always = 2
}
