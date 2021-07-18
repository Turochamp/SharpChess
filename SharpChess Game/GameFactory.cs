using SharpChess.Application;
using SharpChess.Infrastructure;

namespace SharpChess
{
    public static class GameFactory
    {
        // TODO: Move to UI startup
        public static InProcessGame CreateLocal()
        {
            return new InProcessGame(new WindowsRegistry(), new GameSaveFile());
        }
    }
}
