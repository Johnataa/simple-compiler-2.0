using System.Collections.Generic;
using System.Linq;

namespace SimpleCompilerService.Suporte
{
    public class TabelaDeSimbolos
    {
        public Dictionary<string, Simbolo> Tabela { get; set; }

        public TabelaDeSimbolos()
        {
            Tabela = new Dictionary<string, Simbolo>();
        }

        public Queue<Simbolo> BuscaParametros(Simbolo s)
        {

            var r = Tabela
                .Select(p => p.Value)
                .Where(p => p.Categoria == "param" && p.Escopo == s.Cadeia);
            return new Queue<Simbolo>(r);
        }
        
        public Simbolo Busca(object cadeia, string escopo = "")
        {
            var localId = cadeia.ToString() + "#" + escopo;
            var globalId = cadeia.ToString();
            if (escopo != "")
            {
                var simbolo = Tabela.ContainsKey(localId) ? Tabela[localId] : Tabela.ContainsKey(globalId) ? Tabela[globalId] : null;
                return simbolo;
            }
            else
            {
                var simbolo = Tabela.ContainsKey(globalId) ? Tabela[globalId] : null;
                return simbolo;
            }
        }

        public Simbolo Busca(Simbolo s)
        {
            return Busca(s.Cadeia, s.Escopo);
        }

        private Simbolo BuscaForAdd(Simbolo s)
        {
            var simbolo = Busca(s.Cadeia, s.Escopo);
            if (simbolo != null && simbolo.Escopo != s.Escopo)
            {
                return null;
            }
            return simbolo;
        }

        public Simbolo Insere(Simbolo simbolo)
        {
            var s = BuscaForAdd(simbolo);
            if (s == null)
            {
                Tabela[simbolo.SimboloId] = simbolo;
                if (simbolo.Categoria == "procedure")
                {
                    simbolo = new Simbolo(simbolo.Token, simbolo.Cadeia, simbolo.Categoria, simbolo.Valor, simbolo.EnderecoRelativo);
                    Tabela[simbolo.SimboloId] = simbolo;
                }
            }
            return s;
        }

        public Simbolo Insere(ref Queue<Simbolo> fila, string tipo)
        {
            Simbolo aux;
            while (fila.Any())
            {
                var simbolo = fila.Dequeue();
                simbolo.Tipo = tipo;
                aux = Insere(simbolo);
                if (aux != null)
                {
                    aux.SetMsgErro(MsgErrosSemanticos.JA_DECLARADO);
                    return aux;
                }
            }
            return null;
        }
    }
}
