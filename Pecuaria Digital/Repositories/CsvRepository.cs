using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pecuaria_Digital.Constants;
using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Models;

namespace Pecuaria_Digital.Repositories
{
    public class CsvRepository : IArquivoRepository
    {
        public bool SuportaExtensao(string ext) =>
            ext.Equals(".csv", StringComparison.OrdinalIgnoreCase);

        public void Salvar(LoteData lote, string caminho)
        {
            using (var sw = new StreamWriter(caminho,
                append: false,
                encoding: System.Text.Encoding.UTF8))
            {
                // Linha 1 — metadados completos
                sw.WriteLine(
                    $"{ProtocoloConstants.PrefixoEstagio}{lote.MaiorEstagioAlcancado}" +
                    $"{ProtocoloConstants.SeparadorCsv}" +
                    (lote.ModoTabelaCompleta ? ProtocoloConstants.ModoCompleto : ProtocoloConstants.ModoEtapas) +
                    $"{ProtocoloConstants.SeparadorCsv}FAZENDA:{lote.NomeFazenda}" +
                    $"{ProtocoloConstants.SeparadorCsv}DATA:{lote.DataInicioInseminacao}");

                // Linha 2: cabeçalho
                sw.WriteLine(ObterCabecalho());

                // Linhas de dados
                foreach (var animal in lote.Animais)
                    sw.WriteLine(AnimalParaCsv(animal));
            }
            AppLogger.Acao("CSV Salvo", caminho);
        }

        public LoteData Carregar(string caminho)
        {
            var lote = new LoteData();
            string[] linhas;

            try { linhas = File.ReadAllLines(caminho, System.Text.Encoding.UTF8); }
            catch { linhas = File.ReadAllLines(caminho, System.Text.Encoding.Default); }

            if (linhas.Length < 2) return lote;

            // Lê metadados da linha 1
            LerMetadados(linhas[0], lote);

            // Lê dados a partir da linha 3 (0=metadados, 1=cabeçalho, 2+=dados)
            for (int i = 2; i < linhas.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(linhas[i])) continue;
                var animal = CsvParaAnimal(linhas[i]);
                if (!string.IsNullOrWhiteSpace(animal.Id))
                    lote.Animais.Add(animal);
            }

            AppLogger.Acao("CSV Carregado",
                $"{lote.Animais.Count} animais de {caminho}");
            return lote;
        }

        private void LerMetadados(string linha, LoteData lote)
        {
            if (!linha.ToUpper().StartsWith("ESTAGIO:")) return;
            var partes = linha.Split(ProtocoloConstants.SeparadorCsv);
            lote.MaiorEstagioAlcancado = partes[0].Split(':')[1].Trim();
            lote.ModoTabelaCompleta = partes.Length > 1 &&
                partes[1].ToUpper().Contains(ProtocoloConstants.ModoCompleto);

            foreach (var parte in partes)
            {
                if (parte.ToUpper().StartsWith("FAZENDA:"))
                    lote.NomeFazenda = parte.Split(':')[1].Trim();
                if (parte.ToUpper().StartsWith("DATA:"))
                    lote.DataInicioInseminacao = parte.Split(':')[1].Trim();
            }
        }

        private string ObterCabecalho() =>
            "Nº Brinco;Categoria;Raça;ECC;Lote;DataD0;" +
            "PerdeuImplante;eCG;DataD8;" +
            "EscoreCio;GnRH;Touro;Inseminador;DataIATF;" +
            "ResultadoDG;Ovario;Destino;DataDG;Observacoes";

        private string AnimalParaCsv(Animal a) =>
            string.Join(";", new[]
            {
                a.Id, a.Categoria, a.Raca, a.Ecc, a.Lote,
                a.DataD0?.ToString(ProtocoloConstants.FormatoDataHora) ?? "",
                a.PerdeuImplante.ToString(), a.UsouEcg.ToString(),
                a.DataD8?.ToString(ProtocoloConstants.FormatoDataHora) ?? "",
                a.EscoreCio, a.UsouGnrh.ToString(), a.Touro, a.Inseminador,
                a.DataIATF?.ToString(ProtocoloConstants.FormatoDataHora) ?? "",
                a.ResultadoDG, a.Ovario, a.Destino,
                a.DataDG?.ToString(ProtocoloConstants.FormatoDataSimples) ?? "",
                a.Observacoes.Replace(";", ",")
            });

        private Animal CsvParaAnimal(string linha)
        {
            var d = linha.Split(';');
            string G(int i) => i < d.Length ? d[i].Trim() : string.Empty;
            return new Animal
            {
                Id = G(0),
                Categoria = G(1),
                Raca = G(2),
                Ecc = G(3),
                Lote = G(4),
                DataD0 = TryParseDate(G(5)),
                PerdeuImplante = G(6).ToLower() == "true",
                UsouEcg = G(7).ToLower() == "true",
                DataD8 = TryParseDate(G(8)),
                EscoreCio = G(9),
                UsouGnrh = G(10).ToLower() == "true",
                Touro = G(11),
                Inseminador = G(12),
                DataIATF = TryParseDate(G(13)),
                ResultadoDG = G(14),
                Ovario = G(15),
                Destino = G(16),
                DataDG = TryParseDate(G(17)),
                Observacoes = G(18)
            };
        }

        private DateTime? TryParseDate(string s) =>
            DateTime.TryParse(s, out var d) ? (DateTime?)d : null;
    }
}
