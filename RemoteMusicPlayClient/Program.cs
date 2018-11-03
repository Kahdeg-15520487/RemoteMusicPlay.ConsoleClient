using PluginCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RemoteMusicPlayClient
{
    class Program
    {
        static string BaseAddress = "";
        static Dictionary<string, Dictionary<string, ICommand>> Modules;

        #region utility module
        class UtilityModule : IModule
        {
            public string Name { get; set; } = "utility";

            public string Uri { get; set; } = "";

            class BaseAddressSetter : ICommand
            {
                public IModule Module { get; set; }

                public string Name => "setBaseAddr";

                public int ArgumentCount => 1;

                public Result Do(params string[] args)
                {
                    BaseAddress = args[0];
                    return Result.Success(BaseAddress);
                }
            }

            class BaseAddressGetter : ICommand
            {
                public IModule Module { get; set; }

                public string Name => "getBaseAddr";

                public int ArgumentCount => 0;

                public Result Do(params string[] args)
                {
                    return Result.Success(BaseAddress);
                }
            }

            class HelpCommand : ICommand
            {
                public IModule Module { get; set; }

                public string Name => "help";

                public int ArgumentCount => -1;

                public Result Do(params string[] args)
                {
                    if (args.Length == 2)
                    {

                    }

                    Console.WriteLine("Modules: ");
                    //Console.WriteLine("help \"command\" for more help");
                    foreach (var module in Modules)
                    {
                        Console.WriteLine("  {0}", module.Key);
                        foreach (var command in module.Value.Keys)
                        {
                            Console.WriteLine("    {0}", command);
                        }
                    }

                    return Result.Success();
                }
            }

            class ClearScreen : ICommand
            {
                public IModule Module { get; set; }

                public string Name => "clear";

                public int ArgumentCount => -1;

                public Result Do(params string[] args)
                {
                    Console.Clear();
                    return Result.Success();
                }
            }

            class RunFile : ICommand
            {
                public IModule Module { get; set; }

                public string Name => "run";

                public int ArgumentCount => 1;

                public Result Do(params string[] args)
                {
                    var path = args[0] + ".run";
                    if (!File.Exists(path))
                    {
                        return Result.Error("file not found");
                    }

                    foreach (var line in File.ReadAllLines(path))
                    {
                        Run(Helper.ParseArgument(line.Trim()));
                    }

                    return Result.Success();
                }
            }

            class ViewFile : ICommand
            {
                public IModule Module { get; set; }

                public string Name => "view";

                public int ArgumentCount => 1;

                public Result Do(params string[] args)
                {
                    var path = args[0];
                    if (!File.Exists(path))
                    {
                        return Result.Error("file not found");
                    }
                    Console.WriteLine();
                    Console.WriteLine(path);
                    foreach (var line in File.ReadAllLines(path))
                    {
                        Console.WriteLine(line);
                    }
                    Console.WriteLine();
                    return Result.Success();
                }
            }

            class LoadModule : ICommand
            {
                public IModule Module { get; set; }

                public string Name => "loadMDL";

                public int ArgumentCount => 1;

                public Result Do(params string[] args)
                {
                    var path = args[0];

                    if (!File.Exists(path))
                    {
                        return Result.Error($"MDL {path} does not exist");
                    }

                    LoadCommands(Assembly.LoadFile(path));

                    return Result.Success($"loaded {args[0]}");
                }
            }

            class UploadMusic : ICommand
            {
                public IModule Module { get; set; }

                public string Name => "upload";

                public int ArgumentCount => 1;

                public Result Do(params string[] args)
                {
                    var filePath = args[0];
                    HttpResponseMessage result = null;
                    using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        try
                        {
                            result = Helper.UploadFile(new Uri(BaseAddress), file, Path.GetFileName(filePath));
                        }
                        catch (Exception ex)
                        {
                            return Result.Error(ex);
                        }
                    }

                    if (result.IsSuccessStatusCode)
                    {
                        return Result.Success($"{filePath} uploaded");
                    }

                    return Result.Error(result.ReasonPhrase);
                }
            }

            class ListMusic : ICommand
            {
                public IModule Module { get; set; }

                public string Name => "list";

                public int ArgumentCount => 0;

                public Result Do(params string[] args)
                {
                    var result = Helper.ListFile(new Uri(BaseAddress));

                    if (result.StatusCode.IsSuccessStatusCode())
                    {
                        return Result.Success(string.Join(Environment.NewLine, result.Result.Select(m => $"Name: {m.Name}, ID: {m.ID}")));
                    }

                    return Result.Error(result.StatusCode);
                }
            }

            class PlayMusic : ICommand
            {
                public IModule Module { get; set; }

                public string Name => "play";

                public int ArgumentCount => 1;

                public Result Do(params string[] args)
                {
                    var id = args[0];

                    var result = Helper.PlayFile(new Uri(BaseAddress), id);

                    if (result.StatusCode.IsSuccessStatusCode())
                    {
                        return Result.Success($"now playing {result.Result.Name}");
                    }

                    return Result.Error(result.StatusCode);
                }
            }

            class ShowMusic : ICommand
            {
                public IModule Module { get; set; }

                public string Name => "show";

                public int ArgumentCount => 1;

                public Result Do(params string[] args)
                {
                    var id = args[0];

                    var result = Helper.Show(new Uri(BaseAddress), id);

                    if (result.StatusCode.IsSuccessStatusCode())
                    {
                        if (result.Result)
                        {
                            return Result.Success("server is now showed");
                        }
                        else
                        {
                            return Result.Success("server is now hided");
                        }
                    }

                    return Result.Error(result.StatusCode);
                }
            }
        }
        #endregion

        static void Main(string[] args)
        {
            LoadCommands(AppDomain.CurrentDomain.GetAssemblies());

            bool running = true;
            while (running)
            {
                Console.Write(">");
                var rawInput = Console.ReadLine();

                var input = Helper.ParseArgument(rawInput);
                try
                {
                    running = Run(input);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(Result.Error(ex));
                }
            }
        }

        private static bool Run(string[] input)
        {
            if (input.Length == 0)
            {
                return true;
            }

            if (input[0] == "exit")
            {
                return false;
            }

            if (Modules.ContainsKey(input[0]))
            {
                var module = Modules[input[0]];
                if (input.Length == 1)
                {
                    Console.WriteLine(input[0]);
                    foreach (var cmd in module.Keys)
                    {
                        Console.WriteLine("  {0}", cmd);
                    }
                    return true;
                }

                if (module.ContainsKey(input[1]))
                {
                    var command = module[input[1]];

                    if (ValidateArgumentCount(command, input))
                    {
                        Console.WriteLine(command.Do(input.Skip(2).ToArray()));
                    }
                    else
                    {
                        Console.WriteLine("wrong amount of argument: {0} != {1}", input.Length - 2, command.ArgumentCount);
                    }
                }
            }
            else
            {
                var module = Modules["utility"];

                if (module.ContainsKey(input[0]))
                {
                    var command = module[input[0]];

                    if (ValidateArgumentCount(command, input, true))
                    {
                        Console.WriteLine(command.Do(input.Skip(1).ToArray()));
                    }
                    else
                    {
                        Console.WriteLine("wrong amount of argument: {0} != {1}", input.Length - 1, command.ArgumentCount);
                    }
                }
                else
                {
                    Console.WriteLine("command not found : {0}", input[0]);
                }
            }

            return true;
        }

        private static bool ValidateArgumentCount(ICommand cmd, string[] input, bool isStandAloneCmd = false)
        {
            if (!isStandAloneCmd)
            {
                return cmd.ArgumentCount == -1 || cmd.ArgumentCount == input.Length - 2;
            }
            else
            {
                return cmd.ArgumentCount == -1 || cmd.ArgumentCount == input.Length - 1;
            }
        }

        private static int LoadCommands(params Assembly[] assemblies)
        {
            var allModule = Helper.GetAllTypeImplementInterface<IModule>(assemblies);
            Modules = new Dictionary<string, Dictionary<string, ICommand>>();
            int cmdCount = 0;
            foreach (var module in allModule)
            {
                if (Modules.ContainsKey(module.Name))
                {
                    Console.WriteLine("{0} already loaded", module.Name);
                    continue;
                }
                Console.WriteLine("loaded {0}", module.Name);
                var commands = Helper.GetAllTypeImplementInterfaceInType<ICommand>(module);
                var Commands = new Dictionary<string, ICommand>();
                foreach (var cmd in commands)
                {
                    Console.WriteLine("  {0}", cmd.Name);
                    Commands.Add(cmd.Name, cmd);
                    cmdCount++;
                }
                Modules.Add(module.Name, Commands);
            }

            return cmdCount;

            //Commands = new Dictionary<string, IAPICaller>();
            //foreach (var obj in allModule)
            //{
            //    Commands.Add(obj.Name, obj);
            //    Console.WriteLine("Loaded {0}", obj.Name);
            //}
        }
    }
}
