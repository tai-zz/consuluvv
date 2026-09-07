using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ConsultasUVV.Validation;

public class CpfAttribute : ValidationAttribute
{
    public CpfAttribute()
        : base("Informe um CPF válido.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        var digitos = new string((value.ToString() ?? string.Empty).Where(char.IsDigit).ToArray());

        return CpfValido(digitos);
    }

    public static string Normalizar(string? cpf) =>
        new((cpf ?? string.Empty).Where(char.IsDigit).ToArray());

    public static bool CpfValido(string digitos)
    {
        if (digitos.Length != 11)
            return false;

        if (digitos.All(c => c == digitos[0]))
            return false;

        var numeros = digitos.Select(c => c - '0').ToArray();

        var primeiroDigito = CalcularDigito(numeros, 9, 10);
        if (primeiroDigito != numeros[9])
            return false;

        var segundoDigito = CalcularDigito(numeros, 10, 11);
        return segundoDigito == numeros[10];
    }

    private static int CalcularDigito(int[] numeros, int quantidade, int pesoInicial)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += numeros[i] * (pesoInicial - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
