using System;
using Microsoft.Win32;
using SharpChess.Domain;

namespace SharpChess.Infrastructure
{
    // TODO: Cover with integration tests
    public class WindowsRegistry : IWindowsRegistry
    {
        public string GetStringValue(string name)
        {
            RegistryKey registryKeySharpChess = CreateRegistryKey();
            return registryKeySharpChess?.GetValue(name)?.ToString();
        }

        public void SetStringValue(string name, string value)
        {
            RegistryKey registryKeySharpChess = CreateRegistryKey(); 
            registryKeySharpChess?.SetValue(name, value);
        }

        public void DeleteValue(string name)
        {
            RegistryKey registryKeySharpChess = CreateRegistryKey(); ;
            registryKeySharpChess?.DeleteValue(name);
        }

        private RegistryKey CreateRegistryKey()
        {
            RegistryKey registryKeySoftware = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey registryKeySharpChess = registryKeySoftware?.CreateSubKey(@"PeterHughes.org\SharpChess");
            return registryKeySharpChess;
        }
    }
}
