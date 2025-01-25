using Microsoft.Extensions.Hosting;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Big.Data.DataProcessor.Services;

public class PredictionRequest
{
    public string Text { get; set; }
}

public class PredictionResponse
{
    public int PredictedClass { get; set; }
    public float[] Probabilities { get; set; }
}

public class PreprocessingParams
{
    [JsonProperty("max_length")]
    public int MaxLength { get; set; }

    [JsonProperty("do_lower_case")]
    public bool DoLowerCase { get; set; }
}

public class Token
{
    public string Content { get; set; }
    public bool Lstrip { get; set; }
    public bool Normalized { get; set; }
    public bool Rstrip { get; set; }
    public bool SingleWord { get; set; }
    public bool Special { get; set; }
}

public class TokenizerConfig
{
    [JsonProperty("added_tokens_decoder")]
    public Dictionary<int, Token> AddedTokensDecoder { get; set; }

    [JsonProperty("clean_up_tokenization_spaces")]
    public bool CleanUpTokenizationSpaces { get; set; }

    [JsonProperty("cls_token")]
    public string ClsToken { get; set; }

    [JsonProperty("do_basic_tokenize")]
    public bool DoBasicTokenize { get; set; }

    [JsonProperty("do_lower_case")]
    public bool DoLowerCase { get; set; }

    [JsonProperty("mask_token")]
    public string MaskToken { get; set; }

    [JsonProperty("model_max_length")]
    public int ModelMaxLength { get; set; }

    [JsonProperty("never_split")]
    public List<string> NeverSplit { get; set; }

    [JsonProperty("pad_token")]
    public string PadToken { get; set; }

    [JsonProperty("sep_token")]
    public string SepToken { get; set; }

    [JsonProperty("strip_accents")]
    public bool? StripAccents { get; set; }

    [JsonProperty("tokenize_chinese_chars")]
    public bool TokenizeChineseChars { get; set; }

    [JsonProperty("tokenizer_class")]
    public string TokenizerClass { get; set; }

    [JsonProperty("unk_token")]
    public string UnkToken { get; set; }
}


public class BertTokenizer
{
    private readonly Dictionary<string, int> _vocab;
    private readonly TokenizerConfig _config;

    public int VocabSize => _vocab.Count;

    public BertTokenizer(string tokenizerPath)
    {
        var vocabPath = Path.Combine(tokenizerPath, "vocab.txt");
        _vocab = File.ReadAllLines(vocabPath)
            .Select((word, index) => new { word, index })
            .ToDictionary(x => x.word, x => x.index);

        var configPath = Path.Combine(tokenizerPath, "tokenizer_config.json");
        var configJson = File.ReadAllText(configPath);
        _config = JsonConvert.DeserializeObject<TokenizerConfig>(configJson);
    }

    public (long[] InputIds, long[] AttentionMask) Tokenize(string text, int maxLength)
    {
        text = CleanText(text);
        var tokens = text.Split(' ');

        var tokenIds = new List<long> { _vocab[_config.ClsToken] };
        var attentionMask = new List<long> { 1 };

        foreach (var token in tokens)
        {
            if (_vocab.ContainsKey(token))
            {
                tokenIds.Add(_vocab[token]);
            }
            else
            {
                tokenIds.Add(_vocab[_config.UnkToken]);
            }
            attentionMask.Add(1);
        }

        tokenIds.Add(_vocab[_config.SepToken]);
        attentionMask.Add(1);

        while (tokenIds.Count < maxLength)
        {
            tokenIds.Add(_vocab[_config.PadToken]);
            attentionMask.Add(0);
        }

        return (tokenIds.Take(maxLength).ToArray(), attentionMask.Take(maxLength).ToArray());
    }

    public string CleanText(string text)
    {
        text = text.ToLower();
        text = Regex.Replace(text, @"http\S+", "");
        text = Regex.Replace(text, "<.*?>", "");
        text = Regex.Replace(text, @"[\uD800-\uDBFF][\uDC00-\uDFFF]", "");
        text = Regex.Replace(text, "[^a-zA-Z0-9 ]+", "");

        return text;
    }
}


public class PredictionService : IPredictionService
{
    private readonly InferenceSession _session;
    private readonly PreprocessingParams _params;
    private readonly BertTokenizer _tokenizer;

    public PredictionService(IHostEnvironment env)
    {
        var exePath = AppContext.BaseDirectory;
        var basePath = Path.GetFullPath(Path.Combine(exePath, "..", "..", ".."));
        var modelPath = Path.Combine(basePath, "MLModel", "bert_model.onnx");
        _session = new InferenceSession(modelPath);

        var paramsPath = Path.Combine(basePath, "MLModel", "Tokenizer", "preprocessing_params.json");
        var paramsJson = File.ReadAllText(paramsPath);
        _params = JsonConvert.DeserializeObject<PreprocessingParams>(paramsJson);

        var tokenizerPath = Path.Combine(basePath, "MLModel", "Tokenizer");
        _tokenizer = new BertTokenizer(tokenizerPath);
    }

    public PredictionResponse Predict(PredictionRequest request)
    {
        var cleanedText = _tokenizer.CleanText(request.Text);
        var tokenized = _tokenizer.Tokenize(cleanedText, _params.MaxLength);
        var inputIds = new DenseTensor<long>(new[] { 1, _params.MaxLength });
        var tokenTypeIds = new DenseTensor<long>(new[] { 1, _params.MaxLength });

        for (int i = 0; i < _params.MaxLength; i++)
        {
            inputIds[0, i] = tokenized.InputIds[i];
            tokenTypeIds[0, i] = 0;
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)
        };

        using var results = _session.Run(inputs);
        var outputTensor = results.First().AsTensor<float>();
        var logits = outputTensor.ToArray();
        var probabilities = Softmax(logits);

        int predictedClass = probabilities[1] > 0.95 ? 1 : 0;

        return new PredictionResponse
        {
            PredictedClass = predictedClass,
            Probabilities = probabilities
        };
    }

    private float[] Softmax(float[] logits)
    {
        var maxLogit = logits.Max();
        var exp = logits.Select(l => Math.Exp(l - maxLogit)).ToArray();
        var sumExp = exp.Sum();
        return exp.Select(e => (float)(e / sumExp)).ToArray();
    }
}
