# Project Name: OpenAI Console App

## Description 
A simple application that interfaces with OpenAI, which can give responses to strings sent to it via the console and the chatbot outputs it back to the console.

### Preview of program output:

```console
Ask your question:
Hi there! 
Hello, how can I assist you today?
```



## Pseudocode
```
Main() {     
     apiKey = "INSERT_KEY_HERE"; 
     OpenAIClient client = new OpenAIClient(apiKey);

     Console.WriteLine("Ask your question: ");
     prompt = Console.ReadLine();

     string response = await client.GetChatCompletionAsync(prompt);
     Console.WriteLine("Response: " + response);
         
 }	
```
```
Class OpenAIClient {
	
	OpenAIClient(apiKey) {
    		_apiKey = apiKey;
    		_httpClient = new HttpClient {
        		BaseAddress = new Uri("https://api.openai.com/v1/")
    		};    		
	}

	GetChatCompletionAsync(string prompt) {
    		requestBody = new {
        		model = "gpt-4o-mini",
		        messages = new[] {
            			new { role = "user", content = prompt }
        	}
    	};
}
```


