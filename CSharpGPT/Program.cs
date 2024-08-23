
using System;
using System.Threading.Tasks;

namespace OpenAIAPP
{

    public class Program
    {
        public static async Task Main(string[] args)
        {            
            string apiKey = "";
            OpenAIClient client = new OpenAIClient(apiKey);         // Initialize the OpenAI client with the API key

            Console.WriteLine("Ask your question: ");
            string prompt = Console.ReadLine();                     // Read the user's input from the console

            try
            {
                //
                // Get the chat completion from OpenAI and print the response
                string response = await client.GetChatCompletionAsync(prompt);
                Console.WriteLine("Response: " + response);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur and print the error message
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
