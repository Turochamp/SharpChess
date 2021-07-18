using SharpChess.Application;
using SharpChess.Infrastructure;

namespace SharpChess.Domain.Tests
{
    // Move to Application.Tests
    public static class GameFactory
    {
        // TODO: Move to UI startup
        public static InProcessGame CreateLocal()
        {
            return new InProcessGame(new WindowsRegistry(), new GameSaveFile());
        }
    }
}
