using System.Collections.Generic;

namespace Pecuaria_Digital.Models
{
    /// <summary>
    /// Representa o estado completo de um lote pronto para ser salvo ou carregado.
    /// </summary>
    public class LoteData
    {
        public string NomeFazenda { get; set; } = string.Empty;
        public string DataInicioInseminacao { get; set; } = string.Empty;

        // Estágio e modo de visualização
        public string MaiorEstagioAlcancado { get; set; } = "D0_Inicio";
        public bool ModoTabelaCompleta { get; set; }

        // Os animais
        public List<Animal> Animais { get; set; } = new List<Animal>();
    }
}
