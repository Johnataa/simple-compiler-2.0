namespace SimpleCompilerService.Suporte
{
    public class Simbolo
    {        
        public string SimboloId { get; set; }
        public string Cadeia { get; set; }
        public Token Token { get; set; }
        public string Categoria { get; set; }
        public string Tipo { get; set; }
        public object Valor { get; set; }

        public Simbolo(string cadeia, string categoria)
        {
            Cadeia = cadeia;
            Categoria = categoria;
            SimboloId = cadeia + categoria;
        }

        public Simbolo(Token token, string categoria, object valor)
        {
            SimboloId = token.Lexema.ToString() + categoria;
            Cadeia = token.Lexema.ToString();
            Categoria = categoria;
            Token = token;
            Valor = valor;
        }
    }
}
