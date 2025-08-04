using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Persistance;

[JsonSerializable(typeof(SaveFile))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class SourceGenContext : JsonSerializerContext;

