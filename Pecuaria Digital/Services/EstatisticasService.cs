using Pecuaria_Digital.Models;
using System.Linq;
using System.Collections.Generic;
using Pecuaria_Digital.Constants;

namespace Pecuaria_Digital.Services
{
    /// <summary>
    /// Calcula as métricas zootécnicas a partir de uma lista de animais.
    /// Testável de forma independente — sem nenhuma referência à UI.
    /// </summary>
    public class EstatisticasService
    {
        public double TaxaConcepcao { get; private set; }
        public double TaxaServico { get; private set; }
        public double TaxaPrenhez { get; private set; }
        public double IndiceInseminacao { get; private set; }
        public int TotalAnimais { get; private set; }
        public int TotalInseminadas { get; private set; }
        public int TotalPrenhas { get; private set; }

        public void Calcular(IEnumerable<Animal> animais)
        {
            var lista = animais.ToList();

            TotalAnimais = lista.Count;
            TotalInseminadas = lista.Count(a => a.FoiInseminada);
            TotalPrenhas = lista.Count(a => a.EstaPrenha);

            TaxaConcepcao = TotalInseminadas > 0
                ? (double)TotalPrenhas / TotalInseminadas * 100 : 0;

            TaxaServico = TotalAnimais > 0
                ? (double)TotalInseminadas / TotalAnimais * 100 : 0;

            TaxaPrenhez = TotalAnimais > 0
                ? (double)TotalPrenhas / TotalAnimais * 100 : 0;

            IndiceInseminacao = TotalPrenhas > 0
                ? (double)TotalInseminadas / TotalPrenhas : 0;
        }
    }
}

