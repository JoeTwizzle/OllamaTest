using LiteNetLib.Utils;

namespace Backend.Messages
{
    sealed class InitBackendMessage : INetSerializable
    {
        public string OllamaUri { get; set; }
        public string LanguageModel { get; set; }
        public string EmbeddingModel { get; set; }
        public string UserCode { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public InitBackendMessage() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public InitBackendMessage(string ollamaUri, string languageModel, string embeddingModel, string userCode)
        {
            OllamaUri = ollamaUri;
            LanguageModel = languageModel;
            EmbeddingModel = embeddingModel;
            UserCode = userCode;
        }

        public void Deserialize(NetDataReader reader)
        {
            OllamaUri = reader.GetString();
            LanguageModel = reader.GetString();
            EmbeddingModel = reader.GetString();
            UserCode = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OllamaUri);
            writer.Put(LanguageModel);
            writer.Put(EmbeddingModel);
            writer.Put(UserCode);
        }
    }
}
