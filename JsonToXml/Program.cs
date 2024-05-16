using Newtonsoft.Json;
using System.Xml.Linq;

namespace JsonToXml
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("Please provide both input and output directory paths.");
                return;
            }

            string inputDirectory = args[0];
            string outputDirectory = args[1];

            if(!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string[] jsonFiles = Directory.GetFiles(inputDirectory, "*.json");

            foreach(string jsonFile in jsonFiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(jsonFile);

                    XDocument xmlDoc = JsonConvert.DeserializeXNode(jsonContent, "Root") ?? new XDocument();

                    string outputFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(jsonFile) + ".xml");

                    xmlDoc.Save(outputFilePath);

                    Console.WriteLine($"Converted '{jsonFile}' to '{outputFilePath}'.");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error processing '{jsonFile}': {ex.Message}");
                }
            }

            Console.WriteLine("Conversion process completed.");
        }
    }
}