﻿using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

// Import namespaces
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;



namespace analyze_faces
{
    class Program
    {

        private static FaceClient faceClient;
        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
                string cogSvcKey = configuration["CognitiveServiceKey"];

                // Authenticate Face client
                ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(cogSvcKey);
                faceClient = new FaceClient(credentials)
                {
                    Endpoint = cogSvcEndpoint
                };


                // Menu for face functions
                Console.WriteLine("1: Detect faces\n2: Compare faces\n3: Train a facial recognition model\n4: Recognize faces\n5: Verify a face\nAny other key to quit");
                Console.WriteLine("Enter a number:");
                string command = Console.ReadLine();
                switch (command)
                {
                    case "1":
                        await DetectFaces("images/people.jpg");
                        break;
                    case "2":
                        string personImage = "images/person1.jpg"; // Also try person2.jpg
                        await CompareFaces(personImage, "images/people.jpg");
                        break;
                    case "3":
                        List<string> names = new List<string>(){"Aisha", "Sama"};
                        await TrainModel("employees_group", "employees", names);
                        break;
                    case "4":
                        await RecognizeFaces("images/people.jpg", "employees_group");
                        break;
                    case "5":
                        await VerifyFace("images/person1.jpg", "Aisha", "employees_group");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task DetectFaces(string imageFile)
        {
            Console.WriteLine($"Detecting faces in {imageFile}");

            // Specify facial features to be retrieved
            List<FaceAttributeType?> features = new List<FaceAttributeType?>
            {
                FaceAttributeType.Age,
                FaceAttributeType.Emotion,
                FaceAttributeType.Glasses
            };


            // Get faces
            using (var imageData = File.OpenRead(imageFile))
            {    
                var detected_faces = await faceClient.Face.DetectWithStreamAsync(imageData, returnFaceAttributes: features);

                if (detected_faces.Count > 0)
                {
                    Console.WriteLine($"{detected_faces.Count} faces detected.");

                    // Prepare image for drawing
                    Image image = Image.FromFile(imageFile);
                    Graphics graphics = Graphics.FromImage(image);
                    Pen pen = new Pen(Color.LightGreen, 3);
                    Font font = new Font("Arial", 4);
                    SolidBrush brush = new SolidBrush(Color.Black);

                    // Draw and annotate each face
                    foreach (var face in detected_faces)
                    {
                        // Get face properties
                        Console.WriteLine($"\nFace ID: {face.FaceId}");
                        Console.WriteLine($" - Age: {face.FaceAttributes.Age}");
                        Console.WriteLine($" - Emotions:");
                        foreach (var emotion in face.FaceAttributes.Emotion.ToRankedList())
                        {
                            Console.WriteLine($"   - {emotion}");
                        }

                        Console.WriteLine($" - Glasses: {face.FaceAttributes.Glasses}");

                        // Draw and annotate face
                        var r = face.FaceRectangle;
                        Rectangle rect = new Rectangle(r.Left, r.Top, r.Width, r.Height);
                        graphics.DrawRectangle(pen, rect);
                        string annotation = $"Face ID: {face.FaceId}";
                        graphics.DrawString(annotation,font,brush,r.Left, r.Top);
                    }

                    // Save annotated image
                    String output_file = "detected_faces.jpg";
                    image.Save(output_file);
                    Console.WriteLine(" Results saved in " + output_file);   
                }
            }
 
 
        }

        static async Task CompareFaces(string image1, string image2)
        {
            Console.WriteLine($"Comparing faces in {image1} and {image2}");
            // Determine if the face in image 1 is also in image 2
            DetectedFace image_i_face;
            using (var image1Data = File.OpenRead(image1))
            {    
                // Get the first face in image 1
                var image1_faces = await faceClient.Face.DetectWithStreamAsync(image1Data);
                if (image1_faces.Count > 0)
                {
                    image_i_face = image1_faces[0];
                    Image img1 = Image.FromFile(image1);
                    Graphics graphics = Graphics.FromImage(img1);
                    Pen pen = new Pen(Color.LightGreen, 3);
                    var r = image_i_face.FaceRectangle;
                    Rectangle rect = new Rectangle(r.Left, r.Top, r.Width, r.Height);
                    graphics.DrawRectangle(pen, rect);
                    String output_file = "face_to_match.jpg";
                    img1.Save(output_file);
                    Console.WriteLine(" Results saved in " + output_file); 

                    //Get all the faces in image 2
                    using (var image2Data = File.OpenRead(image2))
                    {    
                        var image2Faces = await faceClient.Face.DetectWithStreamAsync(image2Data);

                        // Get faces
                        if (image2Faces.Count > 0)
                        {

                            var image2FaceIds = image2Faces.Select(f => f.FaceId).ToList<Guid?>();
                            var similarFaces = await faceClient.Face.FindSimilarAsync((Guid)image_i_face.FaceId,faceIds:image2FaceIds);
                            var similarFaceIds = similarFaces.Select(f => f.FaceId).ToList<Guid?>();

                            // Prepare image for drawing
                            Image img2 = Image.FromFile(image2);
                            Graphics graphics2 = Graphics.FromImage(img2);
                            Pen pen2 = new Pen(Color.LightGreen, 3);
                            Font font2 = new Font("Arial", 4);
                            SolidBrush brush2 = new SolidBrush(Color.Black);

                            // Draw and annotate each face
                            foreach (var face in image2Faces)
                            {
                                if (similarFaceIds.Contains(face.FaceId))
                                {
                                    // Draw and annotate face
                                    var r2 = face.FaceRectangle;
                                    Rectangle rect2 = new Rectangle(r2.Left, r2.Top, r2.Width, r2.Height);
                                    graphics2.DrawRectangle(pen2, rect2);
                                    string annotation = "Match!";
                                    graphics2.DrawString(annotation,font2,brush2,r2.Left, r2.Top);
                                }
                            }

                            // Save annotated image
                            String output_file2 = "matched_faces.jpg";
                            img2.Save(output_file2);
                            Console.WriteLine(" Results saved in " + output_file2);   
                        }
                    }

                }
            }

        }

        static async Task TrainModel(string groupId, string groupName, List<string> imageFolders)
        {
            Console.WriteLine($"Creating model for {groupId}");



        }

        static async Task RecognizeFaces(string imageFile, string groupId)
        {
            Console.WriteLine($"Recognizing faces in {imageFile}");

        
        }

        static async Task VerifyFace(string personImage, string personName, string groupId)
        {
            Console.WriteLine($"Verifying the person in {personImage} is {personName}");

            string result = "Not verified";

            // Get the ID of the person from the people group


            // print the result
            Console.WriteLine(result);
        }
    }
}
