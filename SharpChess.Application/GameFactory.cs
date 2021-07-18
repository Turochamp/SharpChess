using SharpChess.Infrastructure;
using SharpChess.Domain;

namespace SharpChess.Application
{
    public static class GameFactory
    {
        // TODO: Move to UI startup
        public static Game Create()
        {
            return new Game(new WindowsRegistry(), new GameSaveFile());
        }
    }
}
