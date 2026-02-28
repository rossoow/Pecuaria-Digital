using System;
using System.Collections.Generic;
using System.Text;

namespace Pecuaria_Digital.Models
{
    public class Animal
    {
        // --- Identidade ---
        public string Id { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Raca { get; set; } = string.Empty;
        public string Lote { get; set; } = string.Empty;
        public string Ecc { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;

        // --- D0 ---
        public DateTime? DataD0 { get; set; }

        // --- D8 ---
        public bool PerdeuImplante { get; set; }
        public bool UsouEcg { get; set; }
        public DateTime? DataD8 { get; set; }

        // --- IATF ---
        public string EscoreCio { get; set; } = string.Empty;
        public bool UsouGnrh { get; set; }
        public string Touro { get; set; } = string.Empty;
        public string Inseminador { get; set; } = string.Empty;
        public DateTime? DataIATF { get; set; }

        // --- DG ---
        public string ResultadoDG { get; set; } = string.Empty;
        public string Ovario { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public DateTime? DataDG { get; set; }

        // --- Propriedades calculadas (somente leitura) ---
        public bool FoiInseminada =>
            !string.IsNullOrWhiteSpace(Touro) || !string.IsNullOrWhiteSpace(Inseminador);

        public bool EstaPrenha =>
            ResultadoDG?.ToUpper().Contains("PRENHA") == true ||
            ResultadoDG?.ToUpper() == "TRUE";

    }
}
