using System.Collections.Generic;
using Pecuaria_Digital.Models;

namespace Pecuaria_Digital.Repositories
{
    /// <summary>
    /// Contrato para salvar e carregar dados do protocolo.
    /// Implementado por CsvRepository e ExcelRepository.
    /// </summary>
    public interface IArquivoRepository
    {
        /// <summary>Salva a lista de animais e os metadados do lote.</summary>
        void Salvar(LoteData lote, string caminho);

        /// <summary>Carrega um arquivo e retorna o LoteData (animais + metadados).</summary>
        LoteData Carregar(string caminho);

        /// <summary>Retorna true se o arquivo pode ser lido por este repository.</summary>
        bool SuportaExtensao(string extensao);
    }
}
