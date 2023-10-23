using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;

namespace ComputerVisionQuickstart
{
    class Program
    {
        // Add your Computer Vision key and endpoint
        static string key = Environment.GetEnvironmentVariable("VISION_KEY");
        static string endpoint = Environment.GetEnvironmentVariable("VISION_ENDPOINT");

        private const string READ_TEXT_URL_IMAGE = "https://i0.wp.com/cavessharing.cavesbooks.com.tw/wp-content/uploads/2023/07/%E8%92%BC%E8%A0%85%E6%95%88%E6%87%89.jpg?resize=770%2C450&ssl=1";

        static void Main(string[] args)
        {
            Console.WriteLine("Azure Cognitive Services Computer Vision - .NET quickstart example");
            Console.WriteLine();

            ComputerVisionClient client = Authenticate(endpoint, key);

            // Extract text (OCR) from a URL image using the Read API
            Console.WriteLine("----------------------------------------------------------");
            Console.Write("輸入圖片位置:");
            string userInput = Console.ReadLine(); //C:\Users\Public-GranDen\Pictures\test.jpg

            ReadFileUrl(client, userInput, IsLocalFile(userInput)).Wait();
        }

        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };
            return client;
        }

        private static bool IsLocalFile(string text)
        {
            return !Regex.IsMatch(text, "http");
        }

        public static async Task ReadFileUrl(ComputerVisionClient client, string urlFile, bool isLocalFile)
        {
            // Read text from URL
            //var textHeaders = await client.ReadAsync(urlFile);
            //var textHeaders;

            string operationLocation = "";
            if (isLocalFile)
            {
                using (FileStream imageStream = new FileStream(urlFile, FileMode.Open, FileAccess.Read))
                {
                    Console.WriteLine($"讀取了長度為 {imageStream.Length} 位元組的圖片資料流。");
                    ReadInStreamHeaders textHeaders = await client.ReadInStreamAsync(imageStream);
                    operationLocation = textHeaders.OperationLocation;
                }
            }
            else
            {
                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine("READ FILE FROM URL");

                ReadHeaders textHeaders = await client.ReadAsync(urlFile);
                operationLocation = textHeaders.OperationLocation;
            }
            

            // After the request, get the operation location (operation ID)
            //string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);

            // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            ReadOperationResult results;
            Console.WriteLine($"Extracting text from URL file {Path.GetFileName(urlFile)}...");
            Console.WriteLine();
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));

            // Display the found text.
            Console.WriteLine();
            var textUrlFileResults = results.AnalyzeResult.ReadResults;
            foreach (ReadResult page in textUrlFileResults)
            {
                foreach (Line line in page.Lines)
                {
                    Console.WriteLine(line.Text);
                }
            }
            Console.WriteLine();
        }

    }
}