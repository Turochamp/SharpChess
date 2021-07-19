using SharpChess.Application;
using SharpChess.Infrastructure;

namespace SharpChess
{
    public static class GameFactory
    {
        public static InProcessGame CreateLocal()
        {
            return new InProcessGame(new WindowsRegistry(), new GameSaveFile());
        }
    }
}
