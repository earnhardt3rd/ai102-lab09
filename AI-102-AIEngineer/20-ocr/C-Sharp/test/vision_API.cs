using HandwrittenTextApp.Business_Layer.Interface;
using HandwrittenTextApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
  
namespace HandwrittenTextApp.Business_Layer
{
    public class VisionApiService : IVisionApiService
    {
        // Replace <Subscription Key> with your valid subscription key.
        const string subscriptionKey = "b53fd172-9547-495e-995d-b7bbb72b12c9";
  
        // You must use the same region in your REST call as you used to
        // get your subscription keys. The paid subscription keys you will get
        // it from microsoft azure portal.
        // Free trial subscription keys are generated in the westcentralus region.
        // If you use a free trial subscription key, you shouldn't need to change
        // this region.
        const string endPoint ="https://eastus.api.cognitive.microsoft.com/vision/v1.0/recognizeText Jump ";
  
        ///
<summary>
        /// Gets the handwritten text from the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
  
        /// <param name="imageFilePath">The image file with handwritten text.</param>
        public async Task<string> ReadHandwrittenText()
        {
            string imageFilePath = @"C:\lab\ai102-lab09\AI-102-AIEngineer\20-ocr\C-Sharp\read-text\images\roster.jpg";
            var errors = new List<string>();
            ImageInfoViewModel responeData = new ImageInfoViewModel();
            string extractedResult = "";
            try
            {
                HttpClient client = new HttpClient();
  
                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);
  
                // Request parameter.
                // Note: The request parameter changed for APIv2.
                // For APIv1, it is "handwriting=true".
                string requestParameters = "mode=Handwritten";
  
                // Assemble the URI for the REST API Call.
                string uri = endPoint + "?" + requestParameters;
  
                HttpResponseMessage response;
  
                // Two REST API calls are required to extract handwritten text.
                // One call to submit the image for processing, the other call
                // to retrieve the text found in the image.
                // operationLocation stores the REST API location to call to
                // retrieve the text.
                string operationLocation;
  
                // Request body.
                // Posts a locally stored JPEG image.
                byte[] byteData = GetImageAsByteArray(imageFilePath);
  
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");
  
                    // The first REST call starts the async process to analyze the
                    // written text in the image.
                    response = await client.PostAsync(uri, content);
                }
  
                // The response contains the URI to retrieve the result of the process.
                if (response.IsSuccessStatusCode)
                    operationLocation =
                        response.Headers.GetValues("Operation-Location").FirstOrDefault();
                else
                {
                    // Display the JSON error data.
                    string errorString = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine("\n\nResponse:\n{0}\n",
                    //    JToken.Parse(errorString).ToString());
                    return errorString;
                }
  
                // The second REST call retrieves the text written in the image.
                //
                // Note: The response may not be immediately available. Handwriting
                // recognition is an async operation that can take a variable amount
                // of time depending on the length of the handwritten text. You may
                // need to wait or retry this operation.
                //
                // This example checks once per second for ten seconds.
                string result;
                int i = 0;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    response = await client.GetAsync(operationLocation);
                    result = await response.Content.ReadAsStringAsync();
                    ++i;
                }
                while (i < 10 && result.IndexOf("\"status\":\"Succeeded\"") == -1);
  
                if (i == 10 && result.IndexOf("\"status\":\"Succeeded\"") == -1)
                {
                    Console.WriteLine("\nTimeout error.\n");
                    return "Error";
                }
  
                //If it is success it will execute further process.
                if (response.IsSuccessStatusCode)
                {
                    // The JSON response mapped into respective view model.
                    responeData = JsonConvert.DeserializeObject<ImageInfoViewModel>(result,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Include,
                            Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs earg)
                            {
                                errors.Add(earg.ErrorContext.Member.ToString());
                                earg.ErrorContext.Handled = true;
                            }
                        }
                    );
  
                    var linesCount = responeData.recognitionResult.lines.Count;
                    for (int j = 0; j < linesCount; j++)
                    {
                        var imageText = responeData.recognitionResult.lines[j].text;
                         
                        extractedResult += imageText + Environment.NewLine;
                    }
  
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
            return extractedResult;
        }
  
  
        ///
<summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
  
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        public byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}