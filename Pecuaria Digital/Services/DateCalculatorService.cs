using System;
using Pecuaria_Digital.Constants;

namespace Pecuaria_Digital.Services
{
    /// <summary>
    /// Calcula as datas das etapas do ProtocoloConstants a partir do D0.
    /// Regra: D8 = D0 + 8 dias | IATF = D8 + 2 dias | DG = IATF + 30 dias
    /// </summary>
    public class DateCalculatorService
    {
        public DateTime CalcularD8(DateTime d0) => d0.AddDays(ProtocoloConstants.DiasD0ParaD8);
        public DateTime CalcularIATF(DateTime d8) => d8.AddDays(ProtocoloConstants.DiasD8ParaIATF);
        public DateTime CalcularDG(DateTime iatf) => iatf.AddDays(ProtocoloConstants.DiasIATFParaDG);

        /// <summary>
        /// Recalcula todas as datas a partir do D0.
        /// </summary>
        public (DateTime d8, DateTime iatf, DateTime dg) CalcularCadeia(DateTime d0)
        {
            var d8 = CalcularD8(d0);
            var iatf = CalcularIATF(d8);
            var dg = CalcularDG(iatf);
            return (d8, iatf, dg);
        }
    }
}
