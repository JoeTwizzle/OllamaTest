using OllamaSharp.Models;
using System.Data;
using System.Runtime.InteropServices;

namespace Backend;

internal record Document(string Text, List<float[]> Embedding);
partial class OllamaChatSession
{
    private const float SimilarityThreshold = 0.2f; // Minimum relevance threshold
    private Dictionary<string, List<Document>> _documents = [];

    public void ClearDocuments()
    {
        _documents.Clear();
    }

    public void RemoveDocuments(string key)
    {
        _documents.Remove(key);
    }

    public async Task AddDocument(string key, string text)
    {
        if (_ollama == null)
        {
            LogError("Could not add document! Ollama not loaded.");
            return;
        }

        if (_embeddingModel == null)
        {
            LogError("Could not add document! No embedding model specified.");
            return;
        }

        var request = new EmbedRequest() { Input = [text], Model = _embeddingModel };
        var embedding = await _ollama.EmbedAsync(request);
        ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(_documents, key, out var exists);
        if (!exists)
        {
            list = new();
        }
        list!.Add(new Document(text, embedding.Embeddings));
    }

    public async Task<string> GetFinalPromptAsync(string key, string userPrompt)
    {
        if (_ollama == null)
        {
            throw new InvalidOperationException("Could not query documents! Ollama not loaded.");
        }

        if (_embeddingModel == null)
        {
            throw new InvalidOperationException("Could not query documents! No embedding model specified.");
        }

        if (_activeCharacter == null)
        {
            throw new InvalidOperationException("Could not query documents! No active character set.");
        }
        var request = new EmbedRequest() { Input = [$"The player says to {_activeCharacter.Name}: {Environment.NewLine} {userPrompt}"], Model = _embeddingModel };
        var questionEmbedding = await _ollama.EmbedAsync(request);

        // Find best document with similarity score
        //TODO: Maybe allow more than one doc to be returned?
        if (_documents.TryGetValue(key, out var val))
        {
            var bestMatch = val.Select(doc => new
            {
                Document = doc,
                Similarity = CosineSimilarity(questionEmbedding.Embeddings, doc.Embedding)
            })
            .MaxBy(x => x.Similarity);

            if (bestMatch == null || bestMatch.Similarity < SimilarityThreshold)
                return userPrompt;

            var prompt = $"""
            You may use the following context to aid your answer to a question. 
            If you don't know the answer, just say so OR ask the user to specify what they mean.
            
            Context:
            {bestMatch.Document.Text}
            
            Question: 
            {userPrompt}
            """;

            return prompt;
        }
        return userPrompt;
    }

    private static float CosineSimilarity(List<float[]> vec1, List<float[]> vec2)
    {
        if (vec1.Count == 0 || vec2.Count == 0)
            return 0;

        float[] promptVector = vec1[0];
        float maxSimilarity = float.MinValue;

        foreach (float[] docVector in vec2)
        {
            float dot = 0;
            float mag1 = 0;
            float mag2 = 0;

            for (int i = 0; i < promptVector.Length; i++)
            {
                dot += promptVector[i] * docVector[i];
                mag1 += promptVector[i] * promptVector[i];
                mag2 += docVector[i] * docVector[i];
            }

            mag1 = float.Sqrt(mag1);
            mag2 = float.Sqrt(mag2);

            if (mag1 < float.Epsilon || mag2 < float.Epsilon)
                continue;

            float similarity = dot / (mag1 * mag2);
            if (similarity > maxSimilarity)
                maxSimilarity = similarity;
        }

        return maxSimilarity == float.MinValue ? 0 : maxSimilarity;
    }
}
