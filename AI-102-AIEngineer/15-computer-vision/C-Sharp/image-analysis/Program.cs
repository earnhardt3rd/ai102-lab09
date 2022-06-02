using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

// import namespaces
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;


namespace image_analysis
{
    class Program
    {

        private static ComputerVisionClient cvClient;
        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
                string cogSvcKey = configuration["CognitiveServiceKey"];

                // Get image
                string imageFile = "images/street.jpg";
                if (args.Length > 0)
                {
                    imageFile = args[0];
                }


                // Authenticate Computer Vision client
                ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(cogSvcKey);
                cvClient = new ComputerVisionClient(credentials)
                {
                    Endpoint = cogSvcEndpoint
                };


                // Analyze image
                await AnalyzeImage(imageFile);

                // Get thumbnail
                await GetThumbnail(imageFile);

               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task AnalyzeImage(string imageFile)
        {
            Console.WriteLine($"Analyzing {imageFile}");

            // Specify features to be retrieved
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects,
                VisualFeatureTypes.Adult
            };


            // Get image analysis
            using (var imageData = File.OpenRead(imageFile))
            {    
                var analysis = await cvClient.AnalyzeImageInStreamAsync(imageData, features);

                // get image captions
                foreach (var caption in analysis.Description.Captions)
                {
                    Console.WriteLine($"Description: {caption.Text} (confidence: {caption.Confidence.ToString("P")})");
                }

                // Get image tags


                // Get image categories


                // Get brands in the image


                // Get objects in the image


                // Get moderation ratings


            }     
                
        }

        static async Task GetThumbnail(string imageFile)
        {
            Console.WriteLine("Generating thumbnail");

            // Generate a thumbnail

        }


    }
}
