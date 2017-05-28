﻿using System.ComponentModel;

namespace SimpleCompilerService.Suporte
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

        [Description("Número Inteiro")]
        NUMERO_INTEIRO,

        [Description("Número Real")]
        NUMERO_REAL,

        [Description("Identificador")]
        IDENTIFICADOR,

        [Description("Erro Léxico")]
        ERRO_LEXICO
    }
}
