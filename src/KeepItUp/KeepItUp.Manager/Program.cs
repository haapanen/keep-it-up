using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using KeepItUp.Core;
using KeepItUp.Data;

namespace KeepItUp.Manager
{
    [Verb("add", HelpText = "Add a server")]
    internal class AddOptions
    {
        [Option('n', "name", Required = true, HelpText = "Name of the server")]
        public string Name { get; set; }
        [Option('p', "port", Required = true, HelpText = "Port of the server")]
        public short Port { get; set; }
        [Option('h', "homepath", Required = true, HelpText = "Home path of the server")]
        public string HomePath { get; set; }
        [Option('b', "basepath", Required = true, HelpText = "Base path of the server")]
        public string BasePath { get; set; }
    }

    [Verb("start", HelpText = "Start a server")]
    internal class StartOptions
    {
        [Option('n', "name", HelpText = "Name of the server")]
        public string Name { get; set; }
    }

    [Verb("stop", HelpText = "Stop a server")]
    internal class StopOptions
    {
        [Option('n', "name", HelpText = "Name of the server")]
        public string Name { get; set; }
    }

    [Verb("list", HelpText = "List servers")]
    internal class ListOptions
    {
        [Option('n', "name", Required = false, HelpText = "List servers like <name>")]
        public string Name { get;set; }
    }

    [Verb("remove", HelpText = "Remove server")]
    internal class RemoveOptions
    {
        [Option('n', "name", Required = false, HelpText = "Name of the server")]
        public string Name { get;set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<AddOptions, StartOptions, StopOptions, ListOptions, RemoveOptions>(args)
                .MapResult(
                    (AddOptions opts) => Add(opts),
                    (StartOptions opts) => Start(opts),
                    (StopOptions opts) => Stop(opts),
                    (ListOptions opts) => ListServers(opts),
                    (RemoveOptions opts) => RemoveServer(opts),
                    errs => 1);
        }

        private static int Add(AddOptions opts)
        {
            var ctx = new KeepItUpContext();
            ctx.Add(new Server
            {
                Name = opts.Name,
                HomePath = opts.HomePath,
                BasePath = opts.BasePath,
                Port = opts.Port
            });
            ctx.SaveChanges();
            return 0;
        }

        private static int Start(StartOptions opts)
        {
            var ctx = new KeepItUpContext();
            var match = ctx.Servers.FirstOrDefault(s => s.Name == opts.Name);
            if (match != null)  
            {
                match.Enabled = true;
            }
            ctx.SaveChanges();
            return 0;
        }

        private static int Stop(StopOptions opts)
        {
            var ctx = new KeepItUpContext();
            var match = ctx.Servers.FirstOrDefault(s => s.Name == opts.Name);
            if (match != null)
            {
                match.Enabled = false;
            }
            ctx.SaveChanges();
            return 0;
        }

        private static int ListServers(ListOptions opts)
        {
            var ctx = new KeepItUpContext();

            var servers = string.IsNullOrEmpty(opts.Name)
                ? ctx.Servers.ToList()
                : ctx.Servers.Where(s => s.Name.Contains(opts.Name)).ToList();

            foreach (var server in ctx.Servers)
            {
                Console.WriteLine(server.Name);
                Console.WriteLine($"\tname: {server.Name}");
                Console.WriteLine($"\tport: {server.Port}");
                Console.WriteLine($"\tbasepath: {server.BasePath}");
                Console.WriteLine($"\thomepath: {server.HomePath}");
            }

            return 0;
        }

        private static int RemoveServer(RemoveOptions opts)
        {
            var ctx = new KeepItUpContext();
            var match = ctx.Servers.FirstOrDefault(s => s.Name == opts.Name);
            if (match != null)
            {
                ctx.Remove(match);
            }
            ctx.SaveChanges();
            return 0;
        }
    }
}
