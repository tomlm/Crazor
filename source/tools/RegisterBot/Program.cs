// See https://aka.ms/new-console-template for more information
using CShellNet;


await new Script().Main(args);

class Script : CShell
{
    public async Task Main(IList<string> args)
    {
        Console.WriteLine("What is the name of your bot?");
        var botName = Console.ReadLine().Trim();
        
        Console.WriteLine("What region do you want for your resource group?");
        var region = Console.ReadLine().Trim();

        Console.WriteLine("Creating resource group...");
        dynamic output = await Run($"az group create --name {botName} --location WestUs").AsJson();

        Console.WriteLine("Creating identity...");
        output = await Run($"az identity create --resource-group {region} --name {botName} ").AsJson();

        Console.WriteLine()
        Console.WriteLine(output.ToString());

    }
}