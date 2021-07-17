using SharpChess.Infrastructure;
using SharpChess.Domain;

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
