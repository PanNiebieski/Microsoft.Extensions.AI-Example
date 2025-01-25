using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddChatClient(new OllamaChatClient
    (new Uri("http://localhost:11434"),
    "deepseek-r1:8b"));

var app = builder.Build();

var chatClient = app.Services
    .GetRequiredService<IChatClient>();

var chatCompletion = await chatClient.CompleteAsync
    ("Are you deepseek-r1:8b?");

Console.WriteLine(chatCompletion.Message.Text);

