using System.Text.Json;
using System.Xml;

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
                    var jsonDocument = JsonDocument.Parse(jsonContent);

                    var xml = ConvertJsonToXml(jsonDocument);

                    if(xml != null)
                    {
                        string outputFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(jsonFile) + ".xml");
                        xml.Save(outputFilePath);

                        Console.WriteLine($"Converted '{jsonFile}' to '{outputFilePath}'.");
                    }
                    else
                    {
                        Console.WriteLine($"No root element found in '{jsonFile}'.");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error processing '{jsonFile}': {ex.Message}");
                }
            }

            Console.WriteLine("Conversion process completed.");



            XmlDocument ConvertJsonToXml(JsonDocument jsonDocument)
            {
                XmlDocument xmlDoc = new();

                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(xmlDeclaration);

                JsonElement rootElement = jsonDocument.RootElement;
                JsonProperty rootProperty = rootElement.EnumerateObject().First();
                XmlElement xmlRoot = xmlDoc.CreateElement(SanitizeElementName(rootProperty.Name));
                xmlDoc.AppendChild(xmlRoot);

                ProcessJsonElement(rootProperty.Value, xmlRoot, xmlDoc);

                return xmlDoc;
            }

            void ProcessJsonElement(JsonElement element, XmlElement parentXmlElement, XmlDocument xmlDoc)
            {
                switch(element.ValueKind)
                {
                    case JsonValueKind.Object:
                    foreach(JsonProperty property in element.EnumerateObject())
                    {
                        if(property.Name.StartsWith("@xmlns"))
                        {
                            string[] parts = property.Name.Split(':');
                            if(parts.Length > 1)
                            {
                                parentXmlElement.SetAttribute("xmlns:" + parts[1], property.Value.GetString());
                            }
                            else
                            {
                                parentXmlElement.SetAttribute("xmlns", property.Value.GetString());
                            }
                        }
                        else if(property.Name.StartsWith("@"))
                        {
                            parentXmlElement.SetAttribute(property.Name.Substring(1), property.Value.GetString());
                        }
                        else
                        {
                            XmlElement childElement = xmlDoc.CreateElement(SanitizeElementName(property.Name));
                            parentXmlElement.AppendChild(childElement);
                            ProcessJsonElement(property.Value, childElement, xmlDoc);
                        }
                    }
                    break;
                    case JsonValueKind.Array:
                    foreach(JsonElement arrayElement in element.EnumerateArray())
                    {
                        XmlElement itemElement = xmlDoc.CreateElement(parentXmlElement.Name);
                        parentXmlElement.ParentNode.InsertAfter(itemElement, parentXmlElement);
                        ProcessJsonElement(arrayElement, itemElement, xmlDoc);
                    }
                    parentXmlElement.ParentNode.RemoveChild(parentXmlElement);
                    break;
                    case JsonValueKind.String:
                    case JsonValueKind.Number:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                    parentXmlElement.InnerText = element.ToString();
                    break;
                    case JsonValueKind.Null:
                    parentXmlElement.InnerText = "null";
                    break;
                }
            }

            string SanitizeElementName(string name)
            {
                if(name.StartsWith("_"))
                    return name.Substring(1);

                return System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9_\-\.]", "_");
            }

        }


    }
}