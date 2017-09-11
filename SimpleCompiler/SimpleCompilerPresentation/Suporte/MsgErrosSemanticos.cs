namespace SimpleCompiler
{
    public class MsgErrosSemanticos
    {
        public const string
            NAO_DECLARADO = "Variável ou procedimento '{0}' na linha {1} não declarado.",
            JA_DECLARADO = "Variável ou procedimento '{0}' já declarado na linha {1}.",
            ATRIBUICAO_ERRADA = "Variável '{0}' espera um tipo '{1}' e foi encontrado um tipo '{2}' na linha {3}.",
            PARAMETRO_ERRADO = "Parâmetro '{0}' não é do Tipo esperado pelo procedimento '{1}'.\r\nEsperado: {2}\r\nEncontrado: {3}\r\nLinha: {4}.",
            PARAMETROS_INCORRETOS = "Procedimento '{0}' exige {1} parâmetros e foram recebidos {2}.\r\nLinha: {3}.";
    }
}
