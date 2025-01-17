
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Xml.Linq;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddChatClient(new OllamaChatClient(new Uri("http://localhost:11434"), "llama3"));

var app = builder.Build();

var chatClient = app.Services.GetRequiredService<IChatClient>();

var chatHistory = new List<ChatMessage>();



var posts = Directory.GetFiles("posts").Take(5).ToArray();



foreach (var post in posts)
{
    var xmlContent = File.ReadAllText(post);
    XDocument doc = XDocument.Parse(xmlContent);
    XElement contentElement = doc.Root.Element("content");



    if (contentElement != null)
    {
        string contentValue = contentElement.Value;

        string prompt = $$"""
         Summarize the article in no more than 300 words

         # Article content:

         {{contentValue}}
         """;

        chatHistory.Add(new ChatMessage(ChatRole.User, prompt));

        string chatResponse = "";

        await foreach (var item in chatClient.CompleteStreamingAsync(chatHistory))
        {
            // We're streaming the response, so we get each message as it arrives
            Console.Write(item.Text);
            chatResponse += item.Text;
        }

        chatHistory.Add(new ChatMessage(ChatRole.Assistant, chatResponse));

        string prompt2 = $$""" Możesz przetłumaczyć swoją poprzednią odpowiedź na jezyk polski?""";

        chatHistory.Add(new ChatMessage(ChatRole.User, prompt2));
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine(Environment.NewLine);

        await foreach (var item in chatClient.CompleteStreamingAsync(chatHistory))
        {
            // We're streaming the response, so we get each message as it arrives
            Console.Write(item.Text);
            chatResponse += item.Text;
        }

        Console.WriteLine(chatResponse);
        chatHistory.Add(new ChatMessage(ChatRole.Assistant, chatResponse));

        string prompt3 = $$"""
         Format your previous answer to JSON
         Only provide a RFC8259 compliant JSON response following this format without deviation.
         Don't forget '}' at the end of the JSON object.
         {
            "summary": "Summarize the article in no more than 100 words"
         }
         """;

        chatHistory.Add(new ChatMessage(ChatRole.User, prompt3));
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine(Environment.NewLine);

        await foreach (var item in chatClient.CompleteStreamingAsync(chatHistory))
        {
            // We're streaming the response, so we get each message as it arrives
            Console.Write(item.Text);
            chatResponse += item.Text;
        }

        chatHistory.Add(new ChatMessage(ChatRole.Assistant, chatResponse));

        string prompt4 = $$"""
             Możesz przetłumaczyć swoją poprzednią odpowiedź na jezyk polski. 
            Zachowując format JSON. Nie dodawaj zbędnych komentarzy. Nie zapomnij o „}” na końcu obiektu JSON.
            """;

        chatHistory.Add(new ChatMessage(ChatRole.User, prompt4));
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine(Environment.NewLine);

        await foreach (var item in chatClient.CompleteStreamingAsync(chatHistory))
        {
            // We're streaming the response, so we get each message as it arrives
            Console.Write(item.Text);
            chatResponse += item.Text;
        }

        Console.WriteLine(Environment.NewLine);
        Console.WriteLine("====================================================");
        Console.WriteLine(Environment.NewLine);
    }




}