using System.Collections.Generic;
using System.Linq;

namespace SimpleCompilerService.Suporte
{
    public class TabelaDeSimbolos
    {
        public Dictionary<string, Simbolo> TabelaPrograma { get; set; }
        public Dictionary<string, Simbolo> TabelaProcedimento { get; set; }

        public TabelaDeSimbolos()
        {
            TabelaPrograma = new Dictionary<string, Simbolo>();
            TabelaProcedimento = new Dictionary<string, Simbolo>();
        }

        public Simbolo Busca(string SimboloId, bool isProcedure)
        {
            if (isProcedure)
            {
                if (TabelaProcedimento.ContainsKey(SimboloId))
                {
                    return TabelaProcedimento[SimboloId];
                }
                return null;
            }
            else
            {
                if (TabelaPrograma.ContainsKey(SimboloId))
                {
                    return TabelaPrograma[SimboloId];
                }
                return null;
            }
        }

        public Simbolo Insere(Simbolo simbolo, bool isProcedure)
        {
            var aux = Busca(simbolo.SimboloId, isProcedure);
            if (isProcedure)
            {
                if (aux == null)
                {
                    TabelaProcedimento[simbolo.SimboloId] = simbolo;
                    return null;
                }
            }
            else
            {
                if (aux == null)
                {
                    TabelaPrograma[simbolo.SimboloId] = simbolo;
                    return null;
                }
            }
            return aux;
            
        }

        public List<Simbolo> Insere(ref Queue<Simbolo> fila, string tipo, bool isProcedure)
        {
            var erros = new List<Simbolo>();
            Simbolo aux;
            while (fila.Any())
            {
                var simbolo = fila.Dequeue();
                simbolo.Tipo = tipo;
                aux = Insere(simbolo, isProcedure);
                if (aux != null)
                {
                    erros.Add(aux);
                }
            }
            if (erros.Any())
            {
                return erros;
            }
            return null;
        }
    }
}
