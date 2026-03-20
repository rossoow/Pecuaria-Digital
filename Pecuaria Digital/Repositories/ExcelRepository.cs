using System;
using System.IO;
using ClosedXML.Excel;
using Pecuaria_Digital.Constants;
using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Models;

namespace Pecuaria_Digital.Repositories
{
    public class ExcelRepository : IArquivoRepository
    {
        public bool SuportaExtensao(string ext) =>
            ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase);

        public void Salvar(LoteData lote, string caminho)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Dados Pecuaria");

                // Linha 1 — metadados
                ws.Cell(1, 1).Value =
                    $"{ProtocoloConstants.PrefixoEstagio}{lote.MaiorEstagioAlcancado}" +
                    $"{ProtocoloConstants.SeparadorCsv}" +
                    (lote.ModoTabelaCompleta ? ProtocoloConstants.ModoCompleto : ProtocoloConstants.ModoEtapas) +
                    $"{ProtocoloConstants.SeparadorCsv}FAZENDA:{lote.NomeFazenda}" +
                    $"{ProtocoloConstants.SeparadorCsv}DATA:{lote.DataInicioInseminacao}";

                // Linha 2 — cabeçalho
                string[] cabecalho = ObterCabecalho();
                for (int i = 0; i < cabecalho.Length; i++)
                {
                    ws.Cell(2, i + 1).Value = cabecalho[i];
                    ws.Cell(2, i + 1).Style.Font.Bold = true;
                    ws.Cell(2, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // Linha 3 em diante — dados
                int linhaExcel = 3;
                foreach (var animal in lote.Animais)
                {
                    var valores = AnimalParaArray(animal);
                    for (int i = 0; i < valores.Length; i++)
                        ws.Cell(linhaExcel, i + 1).Value = valores[i];
                    linhaExcel++;
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(caminho);
            }

            AppLogger.Acao("Excel Salvo", caminho);
        }

        public LoteData Carregar(string caminho)
        {
            var lote = new LoteData();

            using (var workbook = new XLWorkbook(caminho))
            {
                var ws = workbook.Worksheet(1);
                if (ws.LastRowUsed() == null) return lote;

                // Linha 1 — metadados
                string meta = ws.Cell(1, 1).GetValue<string>().Trim();
                int linhaCabecalho = 1;

                if (meta.ToUpper().StartsWith(ProtocoloConstants.PrefixoEstagio.ToUpper()))
                {
                    var partes = meta.Split(ProtocoloConstants.SeparadorCsv);
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

                    linhaCabecalho = 2; // metadados na linha 1, cabeçalho na linha 2
                }

                // Mapeamento de colunas pelo cabeçalho
                var mapa = new System.Collections.Generic.Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);
                var linhaCab = ws.Row(linhaCabecalho);
                foreach (var cell in linhaCab.CellsUsed())
                    mapa[cell.GetString().Trim()] = cell.Address.ColumnNumber;

                // Leitura dos dados
                int ultimaLinha = ws.LastRowUsed().RowNumber();
                for (int i = linhaCabecalho + 1; i <= ultimaLinha; i++)
                {
                    var row = ws.Row(i);
                    if (row.IsEmpty()) continue;

                    string G(string col) => mapa.ContainsKey(col)
                        ? row.Cell(mapa[col]).GetValue<string>() : string.Empty;

                    var animal = new Animal
                    {
                        Id = G("Nº Brinco"),
                        Categoria = G("Categoria"),
                        Raca = G("Raça"),
                        Ecc = G("ECC"),
                        Lote = G("Lote Trat."),
                        Observacoes = G("Observações"),
                        PerdeuImplante = G("Perdeu Implante?").ToLower() == "true",
                        UsouEcg = G("Usou eCG?").ToLower() == "true",
                        EscoreCio = G("Escore Cio"),
                        UsouGnrh = G("Indutor (GnRH)?").ToLower() == "true",
                        Touro = G("Touro"),
                        Inseminador = G("Inseminador"),
                        ResultadoDG = G("Resultado DG"),
                        Ovario = G("Est. Ovariana"),
                        Destino = G("Destino Final"),
                    };

                    if (DateTime.TryParse(G("Data D0"), out var d0)) animal.DataD0 = d0;
                    if (DateTime.TryParse(G("Data D8"), out var d8)) animal.DataD8 = d8;
                    if (DateTime.TryParse(G("Data IATF"), out var iatf)) animal.DataIATF = iatf;
                    if (DateTime.TryParse(G("Data DG"), out var dg)) animal.DataDG = dg;

                    if (!string.IsNullOrWhiteSpace(animal.Id))
                        lote.Animais.Add(animal);
                }
            }

            AppLogger.Acao("Excel Carregado",
                $"{lote.Animais.Count} animais de {caminho}");
            return lote;
        }

        private string[] ObterCabecalho() => new[]
        {
            "Nº Brinco", "Categoria", "Raça", "ECC", "Lote Trat.", "Data D0",
            "Perdeu Implante?", "Usou eCG?", "Data D8",
            "Escore Cio", "Indutor (GnRH)?", "Touro", "Inseminador", "Data IATF",
            "Resultado DG", "Est. Ovariana", "Destino Final", "Data DG", "Observações"
        };

        private string[] AnimalParaArray(Animal a) => new[]
        {
            a.Id, a.Categoria, a.Raca, a.Ecc, a.Lote,
            a.DataD0?.ToString(ProtocoloConstants.FormatoDataHora)   ?? "",
            a.PerdeuImplante.ToString(), a.UsouEcg.ToString(),
            a.DataD8?.ToString(ProtocoloConstants.FormatoDataHora)   ?? "",
            a.EscoreCio, a.UsouGnrh.ToString(), a.Touro, a.Inseminador,
            a.DataIATF?.ToString(ProtocoloConstants.FormatoDataHora) ?? "",
            a.ResultadoDG, a.Ovario, a.Destino,
            a.DataDG?.ToString(ProtocoloConstants.FormatoDataSimples) ?? "",
            a.Observacoes
        };
    }
}