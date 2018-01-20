using System;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace SampleApp
{
    // https://msdn.microsoft.com/en-us/library/aa767914(v=vs.85).aspx
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine(args.Aggregate((x, y) => x + "," + y));
                Console.ReadLine();
            }
            else
            {
                ConfigureRegistry();
            }
        }

        private static void ConfigureRegistry()
        {
            if (NeedToAddKeys()) AddRegKeys();
        }

        const string RootKeyPath = @"Software\Classes";

        const string CustomUriScheme = "brock-sample";
        const string CustomUriSchemeKeyPath = RootKeyPath + @"\" + CustomUriScheme;
        const string CustomUriSchemeKeyValueName = "";
        const string CustomUriSchemeKeyValueValue = "URL:" + CustomUriScheme;

        const string ShellKeyName = "shell";
        const string OpenKeyName = "open";
        const string CommandKeyName = "command";

        const string CommandKeyPath = CustomUriSchemeKeyPath + @"\shell\open\command";
        const string CommandKeyValueName = "";
        const string CommandKeyValueFormat = "\"{0}\" \"%1\"";
        static string CommandKeyValueValue => String.Format(CommandKeyValueFormat, Assembly.GetExecutingAssembly().Location);

        const string UrlProtocolValueName = "URL Protocol";
        const string UrlProtocolValueValue = "";

        static bool NeedToAddKeys()
        {
            var addKeys = false;

            using (var commandKey = Registry.CurrentUser.OpenSubKey(CommandKeyPath))
            {
                var commandValue = commandKey?.GetValue(CommandKeyValueName);
                addKeys |= !CommandKeyValueValue.Equals(commandValue);
            }

            using (var customUriSchemeKey = Registry.CurrentUser.OpenSubKey(CustomUriSchemeKeyPath))
            {
                var uriValue = customUriSchemeKey?.GetValue(CustomUriSchemeKeyValueName);
                var protocolValue = customUriSchemeKey?.GetValue(UrlProtocolValueName);

                addKeys |= !CustomUriSchemeKeyValueValue.Equals(uriValue);
                addKeys |= !UrlProtocolValueValue.Equals(protocolValue);
            }

            return addKeys;
        }

        static void AddRegKeys()
        {
            using (var classesKey = Registry.CurrentUser.OpenSubKey(RootKeyPath, true))
            {
                using (var root = classesKey.OpenSubKey(CustomUriScheme, true) ??
                    classesKey.CreateSubKey(CustomUriScheme, true))
                {
                    root.SetValue(CustomUriSchemeKeyValueName, CustomUriSchemeKeyValueValue);
                    root.SetValue(UrlProtocolValueName, UrlProtocolValueValue);

                    using (var shell = root.OpenSubKey(ShellKeyName, true) ??
                            root.CreateSubKey(ShellKeyName, true))
                    {
                        using (var open = shell.OpenSubKey(OpenKeyName, true) ??
                                shell.CreateSubKey(OpenKeyName, true))
                        {
                            using (var command = open.OpenSubKey(CommandKeyName, true) ??
                                    open.CreateSubKey(CommandKeyName, true))
                            {
                                command.SetValue(CommandKeyValueName, CommandKeyValueValue);
                            }
                        }
                    }
                }
            }
        }
    }
}
