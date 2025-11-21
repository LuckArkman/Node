namespace Brain;

public class NeuralNetwork
    {
        private Tensor weightsHidden;
        private Tensor biasHidden;
        private Tensor weightsOutput;
        private Tensor biasOutput;
        private readonly int inputSize;
        private readonly int hiddenSize;
        private readonly int outputSize;

        public NeuralNetwork(int inputSize, int hiddenSize, int outputSize)
        {
            this.inputSize = inputSize;
            this.hiddenSize = hiddenSize;
            this.outputSize = outputSize;

            Random rand = new Random();
            double[] weightsHiddenData = new double[inputSize * hiddenSize];
            double[] biasHiddenData = new double[hiddenSize];
            double[] weightsOutputData = new double[hiddenSize * outputSize];
            double[] biasOutputData = new double[outputSize];

            for (int i = 0; i < weightsHiddenData.Length; i++)
                weightsHiddenData[i] = rand.NextDouble() - 0.5;
            for (int i = 0; i < biasHiddenData.Length; i++)
                biasHiddenData[i] = rand.NextDouble() - 0.5;
            for (int i = 0; i < weightsOutputData.Length; i++)
                weightsOutputData[i] = rand.NextDouble() - 0.5;
            for (int i = 0; i < biasOutputData.Length; i++)
                biasOutputData[i] = rand.NextDouble() - 0.5;

            weightsHidden = new Tensor(weightsHiddenData, new int[] { inputSize, hiddenSize });
            biasHidden = new Tensor(biasHiddenData, new int[] { hiddenSize });
            weightsOutput = new Tensor(weightsOutputData, new int[] { hiddenSize, outputSize });
            biasOutput = new Tensor(biasOutputData, new int[] { outputSize });
        }

        public Tensor Forward(Tensor input)
        {
            if (input.shape.Length != 1 || input.shape[0] != inputSize)
            {
                throw new ArgumentException(
                    "O tensor de entrada deve ser unidimensional com tamanho igual a inputSize.");
            }

            double[] hiddenData = new double[hiddenSize];
            for (int h = 0; h < hiddenSize; h++)
            {
                double sum = 0;
                for (int i = 0; i < inputSize; i++)
                {
                    sum += input.Infer(new int[] { i }) * weightsHidden.Infer(new int[] { i, h });
                }

                sum += biasHidden.Infer(new int[] { h });
                hiddenData[h] = Math.Max(0, sum); // ReLU
            }

            Tensor hidden = new Tensor(hiddenData, new int[] { hiddenSize });

            double[] outputData = new double[outputSize];
            double sumExp = 0;
            for (int o = 0; o < outputSize; o++)
            {
                double sum = 0;
                for (int h = 0; h < hiddenSize; h++)
                {
                    sum += hidden.Infer(new int[] { h }) * weightsOutput.Infer(new int[] { h, o });
                }

                sum += biasOutput.Infer(new int[] { o });
                outputData[o] = Math.Exp(sum);
                sumExp += outputData[o];
            }

            // Aplica softmax
            for (int o = 0; o < outputSize; o++)
            {
                outputData[o] /= sumExp;
            }

            return new Tensor(outputData, new int[] { outputSize });
        }

        public void Train(Tensor[] inputs, Tensor[] targets, int epochs, double learningRate)
        {
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                for (int i = 0; i < inputs.Length; i++)
                {
                    Tensor hidden = ComputeHidden(inputs[i]);
                    Tensor output = Forward(inputs[i]);

                    double[] gradOutput = new double[outputSize];
                    for (int o = 0; o < outputSize; o++)
                    {
                        gradOutput[o] = output.Infer(new int[] { o }) - targets[i].Infer(new int[] { o });
                    }

                    double[] newWeightsOutputData = new double[hiddenSize * outputSize];
                    double[] newBiasOutputData = new double[outputSize];
                    for (int o = 0; o < outputSize; o++)
                    {
                        for (int h = 0; h < hiddenSize; h++)
                        {
                            int idx = h * outputSize + o;
                            newWeightsOutputData[idx] = weightsOutput.Infer(new int[] { h, o }) -
                                                        learningRate * gradOutput[o] * hidden.Infer(new int[] { h });
                        }

                        newBiasOutputData[o] = biasOutput.Infer(new int[] { o }) -
                                               learningRate * gradOutput[o];
                    }

                    weightsOutput = new Tensor(newWeightsOutputData, new int[] { hiddenSize, outputSize });
                    biasOutput = new Tensor(newBiasOutputData, new int[] { outputSize });

                    double[] gradHidden = new double[hiddenSize];
                    for (int h = 0; h < hiddenSize; h++)
                    {
                        double sum = 0;
                        for (int o = 0; o < outputSize; o++)
                        {
                            sum += gradOutput[o] * weightsOutput.Infer(new int[] { h, o });
                        }

                        gradHidden[h] = sum * (hidden.Infer(new int[] { h }) > 0 ? 1 : 0);
                    }

                    double[] newWeightsHiddenData = new double[inputSize * hiddenSize];
                    double[] newBiasHiddenData = new double[hiddenSize];
                    for (int h = 0; h < hiddenSize; h++)
                    {
                        for (int j = 0; j < inputSize; j++)
                        {
                            int idx = j * hiddenSize + h;
                            newWeightsHiddenData[idx] = weightsHidden.Infer(new int[] { j, h }) -
                                                        learningRate * gradHidden[h] * inputs[i].Infer(new int[] { j });
                        }

                        newBiasHiddenData[h] = biasHidden.Infer(new int[] { h }) -
                                               learningRate * gradHidden[h];
                    }

                    weightsHidden = new Tensor(newWeightsHiddenData, new int[] { inputSize, hiddenSize });
                    biasHidden = new Tensor(newBiasHiddenData, new int[] { hiddenSize });
                }
            }
        }

        private Tensor ComputeHidden(Tensor input)
        {
            double[] hiddenData = new double[hiddenSize];
            for (int h = 0; h < hiddenSize; h++)
            {
                double sum = 0;
                for (int i = 0; i < inputSize; i++)
                {
                    sum += input.Infer(new int[] { i }) * weightsHidden.Infer(new int[] { i, h });
                }

                sum += biasHidden.Infer(new int[] { h });
                hiddenData[h] = Math.Max(0, sum);
            }

            return new Tensor(hiddenData, new int[] { hiddenSize });
        }

        public void SaveModel(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("weightsHidden");
                writer.WriteLine(string.Join(",", weightsHidden.GetShape()));
                writer.WriteLine(string.Join(",", weightsHidden.GetData()));

                writer.WriteLine("biasHidden");
                writer.WriteLine(string.Join(",", biasHidden.GetShape()));
                writer.WriteLine(string.Join(",", biasHidden.GetData()));

                writer.WriteLine("weightsOutput");
                writer.WriteLine(string.Join(",", weightsOutput.GetShape()));
                writer.WriteLine(string.Join(",", weightsOutput.GetData()));

                writer.WriteLine("biasOutput");
                writer.WriteLine(string.Join(",", biasOutput.GetShape()));
                writer.WriteLine(string.Join(",", biasOutput.GetData()));
            }
        }

        public bool LoadModel(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                int lineIndex = 0;

                // Carregar weightsHidden
                if (lines[lineIndex++] != "weightsHidden")
                    throw new Exception("Formato inválido: esperado 'weightsHidden'.");
                int[] weightsHiddenShape = Array.ConvertAll(lines[lineIndex++].Split(','), int.Parse);
                if (weightsHiddenShape[0] != inputSize || weightsHiddenShape[1] != hiddenSize)
                    throw new Exception("Dimensões de weightsHidden não correspondem.");
                double[] weightsHiddenData = Array.ConvertAll(lines[lineIndex++].Split(','), double.Parse);
                weightsHidden = new Tensor(weightsHiddenData, weightsHiddenShape);

                // Carregar biasHidden
                if (lines[lineIndex++] != "biasHidden")
                    throw new Exception("Formato inválido: esperado 'biasHidden'.");
                int[] biasHiddenShape = Array.ConvertAll(lines[lineIndex++].Split(','), int.Parse);
                if (biasHiddenShape[0] != hiddenSize)
                    throw new Exception("Dimensões de biasHidden não correspondem.");
                double[] biasHiddenData = Array.ConvertAll(lines[lineIndex++].Split(','), double.Parse);
                biasHidden = new Tensor(biasHiddenData, biasHiddenShape);

                // Carregar weightsOutput
                if (lines[lineIndex++] != "weightsOutput")
                    throw new Exception("Formato inválido: esperado 'weightsOutput'.");
                int[] weightsOutputShape = Array.ConvertAll(lines[lineIndex++].Split(','), int.Parse);
                if (weightsOutputShape[0] != hiddenSize || weightsOutputShape[1] != outputSize)
                    throw new Exception("Dimensões de weightsOutput não correspondem.");
                double[] weightsOutputData = Array.ConvertAll(lines[lineIndex++].Split(','), double.Parse);
                weightsOutput = new Tensor(weightsOutputData, weightsOutputShape);

                // Carregar biasOutput
                if (lines[lineIndex++] != "biasOutput")
                    throw new Exception("Formato inválido: esperado 'biasOutput'.");
                int[] biasOutputShape = Array.ConvertAll(lines[lineIndex++].Split(','), int.Parse);
                if (biasOutputShape[0] != outputSize)
                    throw new Exception("Dimensões de biasOutput não correspondem.");
                double[] biasOutputData = Array.ConvertAll(lines[lineIndex++].Split(','), double.Parse);
                biasOutput = new Tensor(biasOutputData, biasOutputShape);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar o modelo: {ex.Message}");
                return false;
            }
        }
    }