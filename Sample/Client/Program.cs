using System;
using System.Configuration;
using System.IO;
using HeyRed.Mime;
using Shuttle.ContentStore.Application;
using Shuttle.Core.Cli;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string help = "Enter the path to the file to process; or [enter] with empty input to quit:";

            var configuredEndpointUrl = ConfigurationManager.AppSettings["ContentServiceEndpointUrl"];

            var arguments = new Arguments(args);
            var count = arguments.Get("volume", 0);
            var volume = count > 0;

            if (volume)
            {
                Console.WriteLine();
                Console.WriteLine($"[volume] : count = {count}");
                Console.WriteLine();
            }

            Console.WriteLine($"[configured endpoint url] : {configuredEndpointUrl}");
            Console.WriteLine(
                "Enter the content service endpoint url; or [enter] with empty input to use the configured url:");

            var endpointUrl = Console.ReadLine();
            Console.WriteLine();

            var contentService =
                new ContentService(new ContentServiceConfiguration(string.IsNullOrEmpty(endpointUrl)
                    ? configuredEndpointUrl
                    : endpointUrl));

            string path;

            Console.WriteLine(help);

            while (!string.IsNullOrEmpty(path = Console.ReadLine()))
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"Could not find file: {path}");
                    Console.WriteLine();
                    Console.WriteLine(help);

                    continue;
                }

                var fileName = Path.GetFileName(path);

                var id = Guid.Empty;

                if (!volume)
                {
                    Console.WriteLine(
                        "Enter an id (guid) for the content reference; or [enter] with an empty input to assign a new one:");

                    if (!Guid.TryParse(Console.ReadLine(), out id))
                    {
                        id = Guid.NewGuid();
                        Console.WriteLine($"[id] : '{id}'");
                    }

                    count = 1;
                }

                for (var i = 0; i < count; i++)
                {
                    if (volume)
                    {
                        id = Guid.NewGuid();
                    }

                    try
                    {
                        contentService.Register(id, $"{id}-{fileName}", MimeTypesMap.GetMimeType(fileName),
                            File.ReadAllBytes(path), "content-store://sample",
                            Environment.UserDomainName + "\\" + Environment.UserName, DateTime.Now);

                        Console.WriteLine($"[file registered] : index = {i} / id = {id}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                Console.WriteLine();
                Console.WriteLine(help);
            }
        }
    }
}