using Pecuaria_Digital.Logging;
using Pecuaria_Digital.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Pecuaria_Digital.Repositories
{
    public class FazendaRepository
    {
        private const string NomeArquivoMeta = "fazenda.json";

        public List<FazendaData> ListarTodas(string pastaRaiz)
        {
            var lista = new List<FazendaData>();
            if (!Directory.Exists(pastaRaiz)) return lista;

            foreach (var subpasta in Directory.GetDirectories(pastaRaiz))
            {
                string meta = Path.Combine(subpasta, NomeArquivoMeta);
                if (!File.Exists(meta)) continue;
                try { lista.Add(Carregar(subpasta)); }
                catch (Exception ex)
                { AppLogger.Aviso($"Fazenda ignorada ({subpasta}): {ex.Message}"); }
            }
            return lista;
        }

        public FazendaData Carregar(string caminhoPasta)
        {
            string meta = Path.Combine(caminhoPasta, NomeArquivoMeta);
            if (!File.Exists(meta))
                return new FazendaData
                { Nome = Path.GetFileName(caminhoPasta), CaminhoPasta = caminhoPasta };

            string json = File.ReadAllText(meta);
            var fazenda = JsonSerializer.Deserialize<FazendaData>(json) ?? new FazendaData();
            fazenda.CaminhoPasta = caminhoPasta;
            return fazenda;
        }

        public void Salvar(FazendaData fazenda)
        {
            try
            {
                if (!Directory.Exists(fazenda.CaminhoPasta))
                    Directory.CreateDirectory(fazenda.CaminhoPasta);

                string meta = Path.Combine(fazenda.CaminhoPasta, NomeArquivoMeta);
                var opts = new JsonSerializerOptions { WriteIndented = true };

                var dados = new
                {
                    fazenda.Nome,
                    fazenda.Localizacao,
                    fazenda.CaminhoImagem
                };

                File.WriteAllText(meta, JsonSerializer.Serialize(dados, opts));
                AppLogger.Info($"Fazenda salva: {fazenda.Nome}");
            }
            catch (Exception ex)
            {
                AppLogger.Erro("FazendaRepository.Salvar", ex);
                throw;
            }
        }

        public void Excluir(FazendaData fazenda)
        {
            if (Directory.Exists(fazenda.CaminhoPasta))
                Directory.Delete(fazenda.CaminhoPasta, recursive: true);
            AppLogger.Info($"Fazenda excluída: {fazenda.Nome}");
        }

        public List<string> ListarArquivosProtocolo(string caminhoPasta)
        {
            var lista = new List<string>();
            if (!Directory.Exists(caminhoPasta)) return lista;
            foreach (var arq in Directory.GetFiles(caminhoPasta, "*.*"))
            {
                string ext = Path.GetExtension(arq).ToLower();
                if (ext == ".csv" || ext == ".xlsx") lista.Add(arq);
            }
            lista.Sort();
            return lista;
        }
    }
}