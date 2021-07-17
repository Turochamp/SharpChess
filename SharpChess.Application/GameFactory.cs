using SharpChess.Infrastructure;
using SharpChess.Model;

namespace SharpChess.Application
{
    public static class GameFactory
    {
        public static Game Create()
        {
            return new Game(new RegistryService());
        }
    }
}
