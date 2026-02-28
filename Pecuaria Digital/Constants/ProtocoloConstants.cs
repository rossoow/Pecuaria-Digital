using System;
using System.Collections.Generic;
using System.Text;

namespace Pecuaria_Digital.Constants
{
    public static class Protocolo
    {
        // Resultados do Diagnóstico de Gestação
        public const string ResultadoPrenha = "PRENHA";
        public const string ResultadoVazia = "VAZIA";

        // Formato de data padrão do sistema
        public const string FormatoDataHora = "dd/MM/yyyy HH:mm";
        public const string FormatoDataSimples = "dd/MM/yyyy";

        // Metadados do arquivo CSV/Excel
        public const string PrefixoEstagio = "ESTAGIO:";
        public const string ModoCompleto = "MODO:COMPLETO";
        public const string ModoEtapas = "MODO:ETAPAS";

        // Separador CSV
        public const char SeparadorCsv = ';';

        // Cálculo de dias entre etapas
        public const int DiasD0ParaD8 = 8;
        public const int DiasD8ParaIATF = 2;
        public const int DiasIATFParaDG = 30;

        // Metas zootécnicas
        // Essas metas serão melhor estudadas para quando for adicionado a IA no sistema
        public const double MetaTaxaConcepcao = 50.0;
        public const double MetaTaxaServico = 60.0;
        public const double MetaTaxaPrenhez = 40.0;
        public const double MetaIndiceInsemBom = 1.5;
        public const double MetaIndiceInsemMedio = 2.0;
    }

}
