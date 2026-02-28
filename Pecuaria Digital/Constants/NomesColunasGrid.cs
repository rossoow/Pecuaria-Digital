using System;
using System.Collections.Generic;
using System.Text;

namespace Pecuaria_Digital.Constants
{
    /// <summary>
    /// Centraliza os nomes das colunas do DataGridView.
    /// Use sempre estas constantes no lugar de strings literais.
    /// </summary>
    public static class Colunas
    {
        // Grupo 1 — Identidade
        public const string Id = "colId";
        public const string Categoria = "colCategoria";
        public const string Raca = "colRaca";

        // Grupo 2 — D0
        public const string Ecc = "colEcc";
        public const string Lote = "colLote";
        public const string DataD0 = "colDataD0";

        // Grupo 3 — D8
        public const string PerdeuImplante = "colPerdeuImplante";
        public const string Ecg = "colEcg";
        public const string DataD8 = "colDataD8";

        // Grupo 4 — IATF
        public const string EscoreCio = "colEscoreCio";
        public const string Gnrh = "colGnrh";
        public const string Touro = "colTouro";
        public const string Inseminador = "colInseminador";
        public const string DataIATF = "colDataIATF";

        // Grupo 5 — DG
        public const string ResultadoDG = "colResultadoDG";
        public const string Ovario = "colOvario";
        public const string Destino = "colDestino";
        public const string DataDG = "colDataDG";

        // Observações
        public const string Obs = "colObs";
    }

}
