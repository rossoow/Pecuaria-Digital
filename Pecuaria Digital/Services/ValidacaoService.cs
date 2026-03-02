using System.Globalization;

namespace Pecuaria_Digital.Services
{
    /// <summary>
    /// Centraliza todas as regras de validação de dados do sistema.
    /// O Form chama esses métodos em vez de validar diretamente.
    /// </summary>
    public static class ValidacaoService
    {
        /// <summary>
        /// Valida os campos obrigatórios antes de inserir um animal.
        /// </summary>
        public static (bool Valido, string Mensagem) ValidarInsercaoAnimal(
            string numero, string categoria, string raca)
        {
            if (string.IsNullOrWhiteSpace(numero))
                return (false, "Número do brinco é obrigatório.");

            if (!int.TryParse(numero.Trim(), out _))
                return (false, "Número do brinco deve conter apenas dígitos.");

            return (true, string.Empty);
        }

        /// <summary>
        /// Valida o Escore de Condição Corporal (escala 1 a 5).
        /// </summary>
        public static (bool Valido, string Mensagem) ValidarEcc(string ecc)
        {
            if (string.IsNullOrWhiteSpace(ecc))
                return (true, string.Empty); // ECC é opcional

            string normalizado = ecc.Replace(",", ".");
            if (!double.TryParse(normalizado,
                NumberStyles.Any, CultureInfo.InvariantCulture, out double valor))
                return (false, "ECC deve ser um número (ex: 3.5).");

            if (valor < 1 || valor > 5)
                return (false, "ECC deve estar entre 1 e 5.");

            return (true, string.Empty);
        }

        /// <summary>
        /// Valida o nome da fazenda ao criar um novo lote.
        /// </summary>
        public static (bool Valido, string Mensagem) ValidarNomeFazenda(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return (false, "Nome da fazenda é obrigatório.");

            if (nome.Trim().Length < 3)
                return (false, "Nome da fazenda deve ter pelo menos 3 caracteres.");

            return (true, string.Empty);
        }
    }
}