using System.ComponentModel;

namespace SimpleCompiler
{
    public enum Tag
    {
        [Description("Operador")]
        OPERADOR,

        [Description("Símbolo Simples")]
        SIMBOLO_SIMPLES,

        [Description("Símbolo Duplo")]
        SIMBOLO_DUPLO,

        [Description("Palavra Reservada")]
        PALAVRA_RESERVADA,

        [Description("integer")]
        NUMERO_INTEIRO,

        [Description("real")]
        NUMERO_REAL,

        [Description("Identificador")]
        IDENTIFICADOR,

        [Description("Erro Léxico")]
        ERRO_LEXICO
    }
}
