namespace SharpChess.Model
{
    public interface IRegistryService
    {
        string GetStringValue(string v);
        void DeleteValue(string v);
        void SetStringValue(string v, string saveGameFileName);
    }
}