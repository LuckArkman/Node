using System.Diagnostics;
using System.Net.Mime;
using Brain;
using Interfaces;

namespace Core;

public class ChatBot : IChatBot
    {
        private readonly NeuralNetwork neuralNetwork;
        private readonly List<string> vocabulary;
        private readonly int inputSize;
        private readonly Dictionary<int, List<string>> intentResponses;
        private Random _random = new Random();
        private readonly string[] intents;

        public ChatBot(int inputSize, int hiddenSize, int outputSize)
        {
            this.inputSize = inputSize;
            neuralNetwork = new NeuralNetwork(inputSize, hiddenSize, outputSize);
            vocabulary = new List<string>();
            intentResponses = new Dictionary<int, List<string>>();
            intents = new string[]
            {
                "saudação", "despedida", "agradecimento", "pedido de ajuda", "pergunta sobre o bot",
                "pergunta sobre capital", "pedido de fato interessante", "pergunta sobre ciência",
                "pedido de piada", "conversa casual", "falar sobre país", "exclamação positiva"
            };
            for (int i = 0; i < intents.Length; i++)
            {
                intentResponses[i] = new List<string>();
            }
        }

        public static List<(string input, string output)> GetTrainingData()
        {
            return new List<(string input, string output)>
            {
                ("Hello", "Hi! How can I help you today?"),
                ("Hello", "Hello! What is on your mind?"),
                ("Hello", "Hey there! Nice to see you!"),
                ("Hi", "Hello! What is on your mind?"),
                ("Hi", "Hi! How can I assist you?"),
                ("Hey", "Hey! What’s up?"),
                ("Good morning", "Good morning to you too!"),
                ("Good afternoon", "Good afternoon! Hope you're doing well."),
                ("Good evening", "Good evening! How was your day?"),
                ("Hey there", "Hey! Nice to hear from you."),
                ("Greetings", "Greetings! How can I assist you?"),
                ("Bye", "Goodbye! Have a nice day."),
                ("See you later", "See you!"),
                ("Good night", "Good night! Sleep well."),
                ("Farewell", "Farewell! Until next time."),
                ("Talk to you later", "Sure! I'll be here."),
                ("Thank you", "You're welcome!"),
                ("Thanks", "No problem! Happy to help."),
                ("Thanks a lot", "Anytime!"),
                ("Much appreciated", "Glad to be of help."),
                ("I appreciate it", "You're very welcome!"),
                ("Help", "I am here to help! Try asking about a capital city or a general knowledge question."),
                ("I need help", "Sure! What would you like to know?"),
                ("Can you help me?", "Absolutely! What’s your question?"),
                ("What can I ask?", "You can ask about countries, science facts, jokes, or general knowledge!"),
                ("Who are you?", "I am a chatbot created to answer your questions!"),
                ("What is your name?", "I am Ninfa, nice to meet you!"),
                ("What can you do?",
                    "I can answer questions about capitals, general knowledge, and chat about various topics!"),
                ("What is your purpose?", "I am here to chat and answer your questions."),
                ("How old are you?", "I don't have an age, I am a computer program."),
                ("Where do you live?", "I live in the cloud, on servers!"),
                ("The capital of France is", "Paris."),
                ("The capital of Brazil is", "Brasília."),
                ("The capital of Japan is", "Tokyo."),
                ("The capital of Australia is", "Canberra."),
                ("The capital of Canada is", "Ottawa."),
                ("The capital of Germany is", "Berlin."),
                ("The capital of Italy is", "Rome."),
                ("The capital of Argentina is", "Buenos Aires."),
                ("The capital of Spain is", "Madrid."),
                ("The capital of the United States is", "Washington, D.C."),
                ("The capital of Russia is", "Moscow."),
                ("The capital of China is", "Beijing."),
                ("The capital of India is", "New Delhi."),
                ("The capital of South Africa is", "Pretoria."),
                ("The capital of Egypt is", "Cairo."),
                ("The capital of Mexico is", "Mexico City."),
                ("The capital of South Korea is", "Seoul."),
                ("The capital of the UK is", "London."),
                ("The capital of Portugal is", "Lisbon."),
                ("The capital of Peru is", "Lima."),
                ("The capital of Chile is", "Santiago."),
                ("The capital of Colombia is", "Bogotá."),
                ("The capital of Thailand is", "Bangkok."),
                ("The capital of Turkey is", "Ankara."),
                ("The capital of Indonesia is", "Jakarta."),
                ("Tell me something interesting", "Did you know that octopuses have three hearts?"),
                ("Tell me a fun fact", "Bananas are berries, but strawberries aren't!"),
                ("Did you know?", "Honey never spoils. Archaeologists have found edible honey in ancient tombs!"),
                ("What is the largest planet?", "Jupiter is the largest planet in our solar system."),
                ("What is the smallest planet?", "Mercury is the smallest planet in our solar system."),
                ("What is the hottest planet?", "Venus is the hottest due to its thick atmosphere."),
                ("What is the coldest planet?", "Neptune is the coldest in our solar system."),
                ("What is the boiling point of water?",
                    "The boiling point of water is 100 degrees Celsius at standard pressure."),
                ("What is the freezing point of water?", "0 degrees Celsius."),
                ("What is the tallest mountain?", "Mount Everest is the tallest mountain in the world."),
                ("What is the deepest ocean?", "The Pacific Ocean is the deepest."),
                ("What is the longest river?", "The Nile River is often considered the longest."),
                ("How many continents are there?", "There are seven continents."),
                ("How many oceans are there?", "There are five oceans on Earth."),
                ("Who wrote Romeo and Juliet?", "William Shakespeare wrote Romeo and Juliet."),
                ("Who painted the Mona Lisa?", "Leonardo da Vinci."),
                ("Who discovered gravity?", "Isaac Newton."),
                ("What is the speed of light?", "About 299,792 kilometers per second."),
                ("Can you tell me a joke?", "Why don't scientists trust atoms? Because they make up everything!"),
                ("Tell me another joke", "Why did the bicycle fall over? It was two-tired."),
                ("Make me laugh", "I would tell you a UDP joke, but you might not get it."),
                ("Do you know a joke?", "Why did the scarecrow win an award? Because he was outstanding in his field!"),
                ("What is up?", "Not much, just hanging out in the digital world. What is up with you?"),
                ("How are you?", "I’m just a bunch of code, but I’m functioning perfectly!"),
                ("What are you doing?", "Chatting with you!"),
                ("I do not know what to ask",
                    "No worries! How about asking me about a country's capital or a fun fact?"),
                ("I'm bored", "Want to hear a joke or learn a fun fact?"),
                ("Do you have emotions?", "Not really, but I can pretend!"),
                ("Are you real?", "As real as a line of code can be!"),
                ("Do you dream?", "Only in binary."),
                ("Tell me about France", "France is known for its culture, wine, and the Eiffel Tower in Paris."),
                ("More about France", "France has a rich history with landmarks like the Louvre and Versailles."),
                ("Tell me about Brazil", "Brazil is famous for Carnival, the Amazon rainforest, and soccer."),
                ("More about Brazil", "Brazil's capital is Brasília, and it has vibrant cities like Rio de Janeiro."),
                ("Tell me about Japan", "Japan is known for its technology, culture, and Mount Fuji."),
                ("Tell me about Canada",
                    "Canada is famous for maple syrup, beautiful landscapes, and friendly people."),
                ("Wow", "Amazing, right?"),
                ("Cool", "Glad you liked it!"),
                ("Nice", "Thanks!"),
                ("Okay", "Got it!"),
                ("Alright", "Let’s keep going!"),
                ("Really?", "Yup! Absolutely."),
                ("Are you sure?", "I'm pretty confident!"),
                ("That's funny", "I try my best!"),
                ("You're smart", "Thank you! You’re not so bad yourself."),
                ("You're funny", "You're making me blush... if I could."),
                ("I like you", "I like chatting with you too!")
            };
        }

        public async Task<string> Train()
        {
            string modelPath = Path.Combine(Environment.CurrentDirectory, "chatbot_model.csv");
            Console.WriteLine(modelPath);
            if (neuralNetwork.LoadModel(modelPath))
            {
                LoadVocabularyAndResponses();
                return "Modelo carregado com sucesso. Pulando treinamento.";
                // Carregar vocabulário e respostas, já que o modelo foi carregado
            }

            var trainingData = GetTrainingData();
            var intentMap = new Dictionary<string, int>
            {
                { "saudação", 0 }, { "despedida", 1 }, { "agradecimento", 2 }, { "pedido de ajuda", 3 },
                { "pergunta sobre o bot", 4 }, { "pergunta sobre capital", 5 }, { "pedido de fato interessante", 6 },
                { "pergunta sobre ciência", 7 }, { "pedido de piada", 8 }, { "conversa casual", 9 },
                { "falar sobre país", 10 }, { "exclamação positiva", 11 }
            };

            // Mapear entradas para intenções
            var trainingDataWithIntents = new List<(string input, int intent)>();
            foreach (var data in trainingData)
            {
                string input = data.input.ToLower();
                string output = data.output;
                int intent;

                if (input.Contains("hello") || input.Contains("hi") || input.Contains("hey") ||
                    input.Contains("good morning") ||
                    input.Contains("good afternoon") || input.Contains("good evening") || input.Contains("greetings"))
                    intent = intentMap["saudação"];
                else if (input.Contains("bye") || input.Contains("see you") || input.Contains("good night") ||
                         input.Contains("farewell") ||
                         input.Contains("talk to you later"))
                    intent = intentMap["despedida"];
                else if (input.Contains("thank") || input.Contains("appreciated"))
                    intent = intentMap["agradecimento"];
                else if (input.Contains("help") || input.Contains("what can i ask"))
                    intent = intentMap["pedido de ajuda"];
                else if (input.Contains("who are you") || input.Contains("what is your name") ||
                         input.Contains("what can you do") ||
                         input.Contains("what is your purpose") || input.Contains("how old are you") ||
                         input.Contains("where do you live"))
                    intent = intentMap["pergunta sobre o bot"];
                else if (input.Contains("capital of"))
                    intent = intentMap["pergunta sobre capital"];
                else if (input.Contains("tell me something interesting") || input.Contains("fun fact") ||
                         input.Contains("did you know"))
                    intent = intentMap["pedido de fato interessante"];
                else if (input.Contains("planet") || input.Contains("boiling point") ||
                         input.Contains("freezing point") ||
                         input.Contains("tallest mountain") || input.Contains("deepest ocean") ||
                         input.Contains("longest river") ||
                         input.Contains("continents") || input.Contains("oceans") || input.Contains("who wrote") ||
                         input.Contains("who painted") ||
                         input.Contains("who discovered") || input.Contains("speed of light"))
                    intent = intentMap["pergunta sobre ciência"];
                else if (input.Contains("joke") || input.Contains("make me laugh"))
                    intent = intentMap["pedido de piada"];
                else if (input.Contains("what is up") || input.Contains("how are you") ||
                         input.Contains("what are you doing") ||
                         input.Contains("i do not know what to ask") || input.Contains("i'm bored") ||
                         input.Contains("emotions") ||
                         input.Contains("are you real") || input.Contains("do you dream"))
                    intent = intentMap["conversa casual"];
                else if (input.Contains("tell me about") || input.Contains("more about"))
                    intent = intentMap["falar sobre país"];
                else if (input.Contains("wow") || input.Contains("cool") || input.Contains("nice") ||
                         input.Contains("okay") ||
                         input.Contains("alright") || input.Contains("really") || input.Contains("are you sure") ||
                         input.Contains("that's funny") || input.Contains("you're smart") ||
                         input.Contains("you're funny") ||
                         input.Contains("i like you"))
                    intent = intentMap["exclamação positiva"];
                else
                    continue; // Ignora entradas não classificadas

                trainingDataWithIntents.Add((input, intent));
                intentResponses[intent].Add(output);
            }

            // Construir vocabulário
            foreach (var data in trainingDataWithIntents)
            {
                string[] words = data.input.ToLower().Split(' ');
                foreach (string word in words)
                {
                    if (!string.IsNullOrEmpty(word) && !vocabulary.Contains(word))
                    {
                        vocabulary.Add(word);
                    }
                }
            }

            if (vocabulary.Count > inputSize)
            {
                vocabulary.RemoveRange(inputSize, vocabulary.Count - inputSize);
            }

            Tensor[] inputs = new Tensor[trainingDataWithIntents.Count];
            Tensor[] targets = new Tensor[trainingDataWithIntents.Count];

            for (int i = 0; i < trainingDataWithIntents.Count; i++)
            {
                inputs[i] = TextToTensor(trainingDataWithIntents[i].input);
                double[] targetData = new double[intents.Length];
                targetData[trainingDataWithIntents[i].intent] = 1.0; // One-hot encoding
                targets[i] = new Tensor(targetData, new int[] { intents.Length });
            }

            neuralNetwork.Train(inputs, targets, epochs: 100, learningRate: 0.005);
            neuralNetwork.SaveModel(modelPath);
            return "";
        }

        private void LoadVocabularyAndResponses()
        {
            var trainingData = GetTrainingData();
            var intentMap = new Dictionary<string, int>
            {
                { "saudação", 0 }, { "despedida", 1 }, { "agradecimento", 2 }, { "pedido de ajuda", 3 },
                { "pergunta sobre o bot", 4 }, { "pergunta sobre capital", 5 }, { "pedido de fato interessante", 6 },
                { "pergunta sobre ciência", 7 }, { "pedido de piada", 8 }, { "conversa casual", 9 },
                { "falar sobre país", 10 }, { "exclamação positiva", 11 }
            };

            // Mapear entradas para intenções e construir respostas
            foreach (var data in trainingData)
            {
                string input = data.input.ToLower();
                string output = data.output;
                int intent;

                if (input.Contains("hello") || input.Contains("hi") || input.Contains("hey") ||
                    input.Contains("good morning") ||
                    input.Contains("good afternoon") || input.Contains("good evening") || input.Contains("greetings"))
                    intent = intentMap["saudação"];
                else if (input.Contains("bye") || input.Contains("see you") || input.Contains("good night") ||
                         input.Contains("farewell") ||
                         input.Contains("talk to you later"))
                    intent = intentMap["despedida"];
                else if (input.Contains("thank") || input.Contains("appreciated"))
                    intent = intentMap["agradecimento"];
                else if (input.Contains("help") || input.Contains("what can i ask"))
                    intent = intentMap["pedido de ajuda"];
                else if (input.Contains("who are you") || input.Contains("what is your name") ||
                         input.Contains("what can you do") ||
                         input.Contains("what is your purpose") || input.Contains("how old are you") ||
                         input.Contains("where do you live"))
                    intent = intentMap["pergunta sobre o bot"];
                else if (input.Contains("capital of"))
                    intent = intentMap["pergunta sobre capital"];
                else if (input.Contains("tell me something interesting") || input.Contains("fun fact") ||
                         input.Contains("did you know"))
                    intent = intentMap["pedido de fato interessante"];
                else if (input.Contains("planet") || input.Contains("boiling point") ||
                         input.Contains("freezing point") ||
                         input.Contains("tallest mountain") || input.Contains("deepest ocean") ||
                         input.Contains("longest river") ||
                         input.Contains("continents") || input.Contains("oceans") || input.Contains("who wrote") ||
                         input.Contains("who painted") ||
                         input.Contains("who discovered") || input.Contains("speed of light"))
                    intent = intentMap["pergunta sobre ciência"];
                else if (input.Contains("joke") || input.Contains("make me laugh"))
                    intent = intentMap["pedido de piada"];
                else if (input.Contains("what is up") || input.Contains("how are you") ||
                         input.Contains("what are you doing") ||
                         input.Contains("i do not know what to ask") || input.Contains("i'm bored") ||
                         input.Contains("emotions") ||
                         input.Contains("are you real") || input.Contains("do you dream"))
                    intent = intentMap["conversa casual"];
                else if (input.Contains("tell me about") || input.Contains("more about"))
                    intent = intentMap["falar sobre país"];
                else if (input.Contains("wow") || input.Contains("cool") || input.Contains("nice") ||
                         input.Contains("okay") ||
                         input.Contains("alright") || input.Contains("really") || input.Contains("are you sure") ||
                         input.Contains("that's funny") || input.Contains("you're smart") ||
                         input.Contains("you're funny") ||
                         input.Contains("i like you"))
                    intent = intentMap["exclamação positiva"];
                else
                    continue; // Ignora entradas não classificadas

                intentResponses[intent].Add(output);
            }

            // Construir vocabulário
            foreach (var data in trainingData)
            {
                string[] words = data.input.ToLower().Split(' ');
                foreach (string word in words)
                {
                    if (!string.IsNullOrEmpty(word) && !vocabulary.Contains(word))
                    {
                        vocabulary.Add(word);
                    }
                }
            }

            if (vocabulary.Count > inputSize)
            {
                vocabulary.RemoveRange(inputSize, vocabulary.Count - inputSize);
            }
        }

        public async Task<string> Respond(string inputText)
        {
            Tensor input = TextToTensor(inputText);
            Tensor output = neuralNetwork.Forward(input);

            int maxIndex = 0;
            double maxValue = output.Infer(new int[] { 0 });
            for (int i = 1; i < output.shape[0]; i++)
            {
                double value = output.Infer(new int[] { i });
                if (value > maxValue)
                {
                    maxValue = value;
                    maxIndex = i;
                }
            }

            // Escolher uma resposta aleatória da intenção
            var responses = intentResponses[maxIndex];
            if (responses.Count == 0)
                return "Desculpe, não sei como responder a isso.";

            string baseResponse = responses[_random.Next(0, responses.Count)];

            // Construir resposta dinâmica com base na entrada
            string dynamicResponse = baseResponse;
            string inputLower = inputText.ToLower();

            if (maxIndex == intents.Length - 1) // Exclamação positiva
            {
                if (inputLower.Contains("funny"))
                    dynamicResponse = $"Obrigado! Tento ser engraçado às vezes. {baseResponse}";
                else if (inputLower.Contains("smart"))
                    dynamicResponse = $"Valeu pelo elogio! {baseResponse}";
            }
            else if (maxIndex == 5) // Pergunta sobre capital
            {
                string[] countries =
                {
                    "france", "brazil", "japan", "australia", "canada", "germany", "italy", "argentina",
                    "spain", "united states", "russia", "china", "india", "south africa", "egypt",
                    "mexico", "south korea", "uk", "portugal", "peru", "chile", "colombia", "thailand",
                    "turkey", "indonesia"
                };
                string[] capitals =
                {
                    "Paris", "Brasília", "Tokyo", "Canberra", "Ottawa", "Berlin", "Rome", "Buenos Aires",
                    "Madrid", "Washington, D.C.", "Moscow", "Beijing", "New Delhi", "Pretoria", "Cairo",
                    "Mexico City", "Seoul", "London", "Lisbon", "Lima", "Santiago", "Bogotá", "Bangkok",
                    "Ankara", "Jakarta"
                };
                for (int i = 0; i < countries.Length; i++)
                {
                    if (inputLower.Contains(countries[i]))
                    {
                        dynamicResponse = $"{capitals[i]}. Quer saber mais sobre {countries[i].Capitalize()}?";
                        break;
                    }
                }
            }
            else if (maxIndex == 10) // Falar sobre país
            {
                if (inputLower.Contains("france"))
                    dynamicResponse = $"{baseResponse} Quer saber sobre a cultura ou pontos turísticos?";
                else if (inputLower.Contains("brazil"))
                    dynamicResponse = $"{baseResponse} Quer ouvir sobre o Carnaval ou a Amazônia?";
                else if (inputLower.Contains("japan"))
                    dynamicResponse = $"{baseResponse} Interessado em tecnologia ou cultura tradicional?";
                else if (inputLower.Contains("canada"))
                    dynamicResponse = $"{baseResponse} Quer saber sobre a natureza ou as cidades?";
            }
            else if (maxIndex == 8) // Pedido de piada
            {
                dynamicResponse =
                    $"{baseResponse} Aqui vai outra: Por que o astronauta terminou com a namorada? Porque ele precisava de espaço!";
            }

            return dynamicResponse;
        }

        private Tensor TextToTensor(string text)
        {
            double[] inputData = new double[inputSize];
            string[] words = text.ToLower().Split(' ');
            foreach (string word in words)
            {
                int index = vocabulary.IndexOf(word);
                if (index >= 0 && index < inputSize)
                {
                    inputData[index] += 1.0; // Conta a frequência da palavra
                }
            }

            // Normaliza o vetor
            double sum = 0;
            for (int i = 0; i < inputSize; i++)
            {
                sum += inputData[i];
            }

            if (sum > 0)
            {
                for (int i = 0; i < inputSize; i++)
                {
                    inputData[i] /= sum;
                }
            }

            return new Tensor(inputData, new int[] { inputSize });
        }
    }