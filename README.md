# AI Chat Client Example

This project demonstrates how to build a simple AI-powered chat client using `Microsoft.Extensions.AI`. It connects to a locally hosted AI model (e.g., Llama 3) and streams responses in real time.

## Features

- Uses `Microsoft.Extensions.AI` for AI client integration.
- Streams AI responses for real-time output.
- Maintains chat history between the user and the assistant.

## Prerequisites

- An AI model server (e.g., Llama 3) running locally at `http://localhost:11434`.

## Getting Started

- Pull the Ollama container.
- docker run --gpus all -d -v ollama_data:/root/.ollama -p 11434:11434 --name ollama ollama/ollama
- Pull the llama3 model.
- docker exec -it ollama ollama pull llama3

### Clone the Repository

```bash
git clone https://github.com/your-username/your-repo-name.git
cd your-repo-name
