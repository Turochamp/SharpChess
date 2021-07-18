namespace SharpChess.Domain
{
    public interface IWindowsRegistry
    {
        string GetStringValue(string v);
        void DeleteValue(string v);
        void SetStringValue(string v, string saveGameFileName);
    }
}