using Microsoft.Extensions.AI;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Backend;

partial class OllamaChatSession
{
    private const float SimilarityThreshold = 0.2f; // Minimum relevance threshold
    private readonly List<Document> _documents = [];
    private record Document(string Text, List<float[]> Embedding);
    public async Task AddDocument(string text)
    {
        if (_ollama == null)
        {
            Console.WriteLine("Could not add document! Ollama not loaded.");
            return;
        }

        if (_embeddingModel == null)
        {
            Console.WriteLine("Could not add document! No embedding model specified.");
            return;
        }

        var request = new EmbedRequest() { Input = [text], Model = _embeddingModel };
        var embedding = await _ollama.EmbedAsync(request);
        _documents.Add(new Document(text, embedding.Embeddings));
    }

    public async Task<string> GetFinalPromptAsync(string userPrompt)
    {
        if (_ollama == null)
        {
            throw new InvalidOperationException("Could not query document! Ollama not loaded.");
        }

        if (_embeddingModel == null)
        {
            throw new InvalidOperationException("Could not query document! No embedding model specified.");
        }

        var request = new EmbedRequest() { Input = [userPrompt], Model = _embeddingModel };
        var questionEmbedding = await _ollama.EmbedAsync(request);

        // Find best document with similarity score
        var bestMatch = _documents
            .Select(doc => new
            {
                Document = doc,
                Similarity = CosineSimilarity(questionEmbedding.Embeddings, doc.Embedding)
            })
            .MaxBy(x => x.Similarity);

        // Apply similarity threshold
        if (bestMatch == null || bestMatch.Similarity < SimilarityThreshold)
            return userPrompt;

        // Build RAG prompt
        var prompt = $"""
            You may use the following context to aid your answer to a question. 
            If you don't know the answer, just say so and ask the user to specify what they mean.
            
            Context:
            {bestMatch.Document.Text}
            
            Question: 
            {userPrompt}
            """;

        return prompt;
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
