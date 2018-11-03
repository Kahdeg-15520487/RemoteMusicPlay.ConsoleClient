namespace PluginCore.Core
{
    public interface ICommand
    {
        IModule Module { get; set; }
        string Name { get; }
        int ArgumentCount { get; }

        Result Do(params string[] args);
    }
}
