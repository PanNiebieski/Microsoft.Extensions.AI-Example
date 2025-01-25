using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Xml.Linq;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddChatClient(new OllamaChatClient(new Uri("http://localhost:11434"), "mistral"));

var app = builder.Build();

var chatClient = app.Services.GetRequiredService<IChatClient>();

var postsDirectory = "posts";
var outputDirectory = "D:\\PanNiebieski\\Webinary\\CShaprOpenAI\\CShaprOpenAI-03-Json\\mistral";
var postFiles = Directory.GetFiles(postsDirectory).Take(11).ToArray();

// Ensure the output directory exists
Directory.CreateDirectory(outputDirectory);

foreach (var postFile in postFiles)
{
    var chatHistory = new List<ChatMessage>();
    await ProcessPost(postFile, chatClient, chatHistory, outputDirectory);
}

static async Task ProcessPost(string postFile, IChatClient chatClient, List<ChatMessage> chatHistory, string outputDirectory)
{
    var responses = new StringBuilder();
    var xmlContent = File.ReadAllText(postFile);
    var doc = XDocument.Parse(xmlContent);
    var contentElement = doc.Root?.Element("content");

    if (contentElement == null)
        return;

    string articleContent = contentElement.Value;

    // Step 1: Summarize the article
    string summaryPrompt = $$"""
        Summarize the article in no more than 300 words.
        Write a summary as if you were the author of the text

        # Article content:

        {{articleContent}}
        """;
    string summaryResponse = await ExecuteChatPromptAsync(chatClient, chatHistory, summaryPrompt);
    responses.AppendSection("Summary", summaryPrompt, summaryResponse);

    // Step 2: Translate the summary to Polish
    string translationPrompt = "Możesz przetłumaczyć swoją poprzednią odpowiedź na język polski?";
    string translationResponse = await ExecuteChatPromptAsync(chatClient, chatHistory, translationPrompt);
    responses.AppendSection("Translation to Polish", translationPrompt, translationResponse);

    // Step 3: Format summary as JSON
    string jsonPrompt = $$"""
        Format your previous answer to JSON.
        Only provide a RFC8259 compliant JSON response following this format without deviation:
        {
            "summary": "Summarize the article in no more than 100 words. Write a summary as if you were the author of the text"
        }
        """;
    string jsonResponse = await ExecuteChatPromptAsync(chatClient, chatHistory, jsonPrompt);
    responses.AppendSection("JSON Format", jsonPrompt, jsonResponse);

    // Step 4: Translate JSON to Polish
    string jsonTranslationPrompt = """
        Możesz przetłumaczyć swoją poprzednią odpowiedź na język polski, zachowując format JSON?
        Nie dodawaj zbędnych komentarzy. Nie zapomnij o „}” na końcu obiektu JSON.
        """;
    string jsonTranslationResponse = await ExecuteChatPromptAsync(chatClient, chatHistory, jsonTranslationPrompt);
    responses.AppendSection("JSON Translation to Polish", jsonTranslationPrompt, jsonTranslationResponse);

    // Write responses to a file
    string outputFileName = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(postFile) + ".txt");
    File.WriteAllText(outputFileName, responses.ToString());
}

static async Task<string> ExecuteChatPromptAsync(IChatClient chatClient, List<ChatMessage> chatHistory, string prompt)
{
    chatHistory.Add(new ChatMessage(ChatRole.User, prompt));
    var responseBuilder = new StringBuilder();

    // Stream responses from the chat client
    var chatResponse = chatClient.CompleteStreamingAsync(chatHistory);
    await foreach (var message in chatResponse)
    {
        Console.Write(message.Text);
        responseBuilder.Append(message.Text);
    }

    string responseText = responseBuilder.ToString();
    chatHistory.Add(new ChatMessage(ChatRole.Assistant, responseText));
    return responseText;
}

public static class Extension
{
    public static void AppendSection(this StringBuilder sb, string sectionTitle, string prompt, string response)
    {
        sb.AppendLine(new string('=', 50));
        sb.AppendLine($"# {sectionTitle}");
        sb.AppendLine($"Prompt:\n{prompt}");
        sb.AppendLine(new string('-', 50));
        sb.AppendLine($"Response:\n{response}");
        sb.AppendLine(new string('=', 50));
    }

}

