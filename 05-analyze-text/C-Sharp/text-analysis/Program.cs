using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Azure.AI.TextAnalytics;
using Azure;
using Microsoft.Extensions.Configuration;

// Import namespaces


namespace text_analysis
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string svcEndpoint = configuration["CognitiveServicesEndpoint"];
                string svcKey = configuration["CognitiveServiceKey"];

                // Create client using endpoint and key
                AzureKeyCredential credentials = new AzureKeyCredential(svcKey);
                Uri endpoint = new Uri(svcEndpoint);
                TextAnalyticsClient textAnalyticsClient = new TextAnalyticsClient(endpoint, credentials);

                // Analyze each text file in the reviews folder
                var folderPath = Path.GetFullPath("./reviews");  
                DirectoryInfo folder = new DirectoryInfo(folderPath);
                foreach (var file in folder.GetFiles("*.txt"))
                {
                    // Read the file contents
                    Console.WriteLine("\n-------------\n" + file.Name);
                    StreamReader sr = file.OpenText();
                    var text = sr.ReadToEnd();
                    sr.Close();
                    // Get language
                    Console.WriteLine("DETECT LANGUAGE");
                    DetectedLanguage detectedLanguage = textAnalyticsClient.DetectLanguage(text);
                    Console.WriteLine($"Language: {detectedLanguage.Name}");

                    // Get sentiment
                    DocumentSentiment documentSentiment = textAnalyticsClient.AnalyzeSentiment(text);
                    Console.WriteLine($"Sentiments: {documentSentiment.Sentiment}");
                    
                    // Get key phrases
                    KeyPhraseCollection phrases = textAnalyticsClient.ExtractKeyPhrases(text);
                    foreach(var phrase in phrases)
                    {
                        Console.WriteLine($"{phrase}");
                    }

                    // Get entities
                    CategorizedEntityCollection entities = textAnalyticsClient.RecognizeEntities(text);
                    foreach(var entity in entities) 
                    {
                        Console.WriteLine($"{entity.Text} - {entity.Category} - {entity.ConfidenceScore}");
                    }


                    // Get linked entities
                    LinkedEntityCollection linkedEntities = textAnalyticsClient.RecognizeLinkedEntities(text);
                    if (linkedEntities.Count > 0)
                    {
                        Console.WriteLine("\nLinks:");
                        foreach (LinkedEntity linkedEntity in linkedEntities)
                        {
                            Console.WriteLine($"\t{linkedEntity.Name} ({linkedEntity.Url})");
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



    }
}
