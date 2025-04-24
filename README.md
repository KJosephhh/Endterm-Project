LLama Chat is a simple web app where you can talk to the LLama model using the Ollama API. You type a question, and it sends it to the model to generate a response. The response shows up in real time, right after you send your message. The foundation of this project comes from experimenting with the Ollama application, which allows users to install and run AI models locally on their personal computers. However Ollama doesn't offer any built in user interface. So the speed of the models responses may vary among hardware. 

Frontend: You type your question into a text box and click "Send".

Backend: The backend takes your question, sends it to the Ollama API, and gets a response back.

Display: The response is shown on the page right below your question(which may take some time).

REQUIREMENTS:
This program requires https://github.com/ollama/ollama and llama3.2 in order for the AI model to function properly along with the Ionide-fsharp extension.

