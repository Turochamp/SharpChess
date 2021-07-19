using SharpChess.Application;
using SharpChess.Infrastructure;

namespace SharpChess.Application.Tests
{
    // TODO: DRY with SharpChess.GameFactory
    public static class GameFactory
    {
        public static InProcessGame CreateLocal()
        {
            return new InProcessGame(new WindowsRegistry(), new GameSaveFile());
        }
    }
}
