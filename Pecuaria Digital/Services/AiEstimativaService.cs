using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pecuaria_Digital.Services
{
    public class AiEstimativaService
    {
        private readonly string _apiKey;
        private static readonly HttpClient _http = new();
        private const string Endpoint = "https://api.anthropic.com/v1/messages";

        public AiEstimativaService(string apiKey) => _apiKey = apiKey;

        public async Task<double?> EstimarSucessoAsync(
            IEnumerable<Animal> animais, string estagio)
        {
            if (string.IsNullOrWhiteSpace(_apiKey)) return null;

            try
            {
                string prompt = MontarPrompt(animais, estagio);
                string resposta = await ChamarApiAsync(prompt);
                return ParsearPercentual(resposta);
            }
            catch (Exception ex)
            {
                AppLogger.Aviso($"IA indisponível: {ex.Message}");
                return null;
            }
        }

        private static string MontarPrompt(IEnumerable<Animal> animais, string estagio)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Você é especialista em reprodução bovina (IATF).");
            sb.AppendLine($"Etapa atual: {estagio}");
            sb.AppendLine("Estime a Taxa de Prenhez (%). Responda APENAS com um número (ex: 58.5).");
            sb.AppendLine();
            int n = 0;
            foreach (var a in animais)
            {
                sb.AppendLine($"ECC:{a.Ecc}|Raça:{a.Raca}|Categoria:{a.Categoria}" +
                              $"|EscoreCio:{a.EscoreCio}|Touro:{a.Touro}" +
                              $"|eCG:{a.UsouEcg}|GnRH:{a.UsouGnrh}|DG:{a.ResultadoDG}");
                if (++n >= 50) { sb.AppendLine("...(truncado)"); break; }
            }
            return sb.ToString();
        }

        private async Task<string> ChamarApiAsync(string prompt)
        {
            var payload = new
            {
                model = "claude-sonnet-4-5",
                max_tokens = 20,
                messages = new[] { new { role = "user", content = prompt } }
            };

            var req = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            req.Headers.Add("x-api-key", _apiKey);
            req.Headers.Add("anthropic-version", "2023-06-01");
            req.Content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("content")[0]
                       .GetProperty("text").GetString() ?? "";
        }

        private static double? ParsearPercentual(string texto)
        {
            string limpo = texto.Trim().Replace("%", "").Replace(",", ".");
            return double.TryParse(limpo,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double v)
                && v >= 0 && v <= 100 ? Math.Round(v, 1) : null;
        }
    }
}