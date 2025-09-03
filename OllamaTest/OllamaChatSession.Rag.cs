using Microsoft.Extensions.AI;
using OllamaSharp.Models;
using System.Data;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace Backend;

internal record Document(string Text, List<float[]> Embedding);
partial class OllamaChatSession
{
    private const float SimilarityThreshold = 0.2f; // Minimum relevance threshold


    public void RemoveDocuments(string npcName)
    {
        var state = GetNpcState(npcName);
        state.RagDocuments.Clear();
        LogWarning($"{npcName}'s documents cleared");
    }

    public async Task AddDocument(string npcName, string text)
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
        var state = GetNpcState(npcName);
        state.RagDocuments.Add(new Document(text, embedding.Embeddings));
    }

    public void AddDocument(string npcName, Document document)
    {
        var state = GetNpcState(npcName);
        state.RagDocuments.Add(document);
    }

    public void RemoveDocument(string npcName, string text)
    {
        var state = GetNpcState(npcName);
        var doc = state.RagDocuments.Where(x => x.Text == text).FirstOrDefault();
        if (doc == null)
        {
            return;
        }
        state.RagDocuments.Remove(doc);
        LogWarning($"{npcName}'s document with text: {Environment.NewLine}{Environment.NewLine}{text}{Environment.NewLine}{Environment.NewLine} Was removed.");
    }

    public async Task<string> GetFinalPromptAsync(string npcName, string userPrompt)
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
        var state = GetNpcState(npcName);

        if (state.RagDocuments.Count > 0)
        {
            var bestMatches = state.RagDocuments.Select(doc => new
            {
                Document = doc,
                Similarity = CosineSimilarity(questionEmbedding.Embeddings, doc.Embedding)
            })
            .OrderBy(x => x.Similarity)
            .Where(x => x.Similarity >= SimilarityThreshold)
            .Take(3)
            .ToArray();

            if (bestMatches.Length == 0)
                return userPrompt;

            var context = string.Join("\n\n---\n\n", bestMatches.Select(m => m.Document.Text));
            var prompt = $"""
            You may use the following context to aid your answer. 
            If you don't know the answer, just say so OR ask the user to specify what they mean.
            
            Context:
            {context}
            
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
