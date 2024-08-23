using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class OpenAIClient
{
    private readonly HttpClient _httpClient;                    // HttpClient instance for making HTTP requests
    private readonly string _apiKey;                            // API key for authenticating with my OpenAI client
    private static int requestCounter = 0;                      // Counter to track the number of requests made    

    // Constructor to initialize the OpenAIClient with the provided API key
    public OpenAIClient(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")                 // Base address for the OpenAI API
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey); // Set the authorization header with the API key
    }

    // Create the request body with the model and user prompt 
    public async Task<string> GetChatCompletionAsync(string prompt)            
    {
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        int maxRetries = 1;                                                     // Maximum number of retry attempts
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"))
            {
                try
                {
                    requestCounter++;                                           // Increment the request counter
                    Console.WriteLine($"Attempt {attempt + 1}, Request count: {requestCounter}");
                                                                                // Send the POST request to the OpenAI API
                    HttpResponseMessage response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                    if (response.IsSuccessStatusCode)                           // Read and deserialize the response content
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<OpenAIResponse>(responseString);
                        return responseObject.choices[0].message.content;       // Return the generated content
                    }
                    else if ((int)response.StatusCode == 429)
                    {
                        Console.WriteLine("Rate limit exceeded, retrying...");  // Handle rate limit exceeded
                        await HandleRateLimit(response.Headers);
                        await DelayWithExponentialBackoff(attempt);
                    }
                    else
                    {   
                        // Handle other HTTP errors
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"HTTP error {(int)response.StatusCode}: {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        }
        //
        // If max retry attempts are exceeded, throw an exception
        Console.WriteLine("Max retry attempts exceeded. Please try again later.");
        throw new Exception("Max retry attempts exceeded. Failed to get response from OpenAI.");
    }

    public class OpenAIResponse
    {
        public List<Choice> choices { get; set; }               // List of choices returned by the OpenAI API
    }

    public class Choice
    {
        public Message message { get; set; }                    // List of choices returned by the OpenAI API
    }

    public class Message
    {
        public string role { get; set; }                        // Role of the message sender (e.g., "user" or "assistant")
        public string content { get; set; }                     // Content of the message
    }

    //
    // Method to introduce a delay with exponential backoff
    private async Task DelayWithExponentialBackoff(int attempt)
    {
        int delay = (int)Math.Pow(2, attempt) * 1000;           // Calculate delay based on the attempt number
        Console.WriteLine($"Waiting for {delay} milliseconds before retrying...");
        await Task.Delay(delay);                                // Wait for the calculated delay
    }
    //
    // Method to handle rate limit responses from the API
    private async Task HandleRateLimit(HttpResponseHeaders headers)
    {
        // Check if rate limit headers are present
        if (headers.TryGetValues("X-Ratelimit-Limit", out var limitValues) &&
            headers.TryGetValues("X-Ratelimit-Remaining", out var remainingValues) &&
            headers.TryGetValues("X-Ratelimit-Reset", out var resetValues))
        {
            int limit = int.Parse(limitValues.First());             // Parse the rate limit value
            int remaining = int.Parse(remainingValues.First());     // Parse the remaining requests value
            int reset = int.Parse(resetValues.First());             // Parse the reset time value

            Console.WriteLine($"Rate limit: {limit}, Remaining: {remaining}, Reset in: {reset} seconds");

            if (remaining == 0)
            {
                //
                // If no remaining requests, wait until the rate limit resets
                Console.WriteLine($"Rate limit exceeded. Waiting for {reset} seconds.");
                await Task.Delay(reset * 1000);                     // Wait for the reset time in milliseconds
            }
        }
    }
}

    
    




