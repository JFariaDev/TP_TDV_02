// BratalianDB.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Bratalian
{
    /// <summary>
    /// Carrega e expõe os dados de bratalian_db.json
    /// </summary>
    public static class BratalianDB
    {
        public static Dictionary<int, BratalianData> Data { get; private set; }

        public static void Load()
        {
            // Aqui o JSON deve ter sido copiado para bin/.../Content/bratalian_db.json
            string file = Path.Combine(AppContext.BaseDirectory, "Content", "bratalian_db.json");
            if (!File.Exists(file))
                throw new FileNotFoundException($"Nao encontrei o JSON em: {file}");

            string json = File.ReadAllText(file);
            var list = JsonSerializer.Deserialize<List<BratalianData>>(json);
            Data = list.ToDictionary(b => b.id);
        }

        public static BratalianData Get(int id)
            => Data != null && Data.TryGetValue(id, out var d) ? d : null;
    }
}
