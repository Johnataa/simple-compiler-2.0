﻿namespace SimpleCompiler
{
    public class Simbolo
    {
        #region 1. Propriedades
        public string SimboloId { get; set; }
        public string Cadeia { get; set; }
        public Token Token { get; set; }
        public string Categoria { get; set; }
        public string Escopo { get; set; }
        public string Tipo { get; set; }
        public object Valor { get; set; }
        public string MsgErro { get; set; }
        public int EnderecoRelativo { get; set; }
        public int PrimeiraInstrucao { get; set; }
        #endregion

        #region 2. Métodos Construtores
        public Simbolo(Token token, string escopo, string categoria, object valor, int enderecoRelativo, int primeiraInstrucao = 0)
        {
            SimboloId = token.Lexema.ToString();
            if (escopo != "")
            {
                SimboloId += "#" + escopo;
            }
            Cadeia = token.Lexema.ToString();
            Categoria = categoria;
            Escopo = escopo;
            Token = token;
            Valor = valor;
            Tipo = categoria == "procedure" ? "void" : "";
            EnderecoRelativo = enderecoRelativo;
            PrimeiraInstrucao = primeiraInstrucao;
        }

        public Simbolo(Token token, string msgErro)
        {
            Token = token;
            Cadeia = token.Lexema.ToString();
            MsgErro = msgErro.Replace("{0}", Cadeia).Replace("{1}", Token.Linha.ToString());
        }
        #endregion

        #region 3. Métodos Públicos
        public void SetMsgErro(string msgErro)
        {
            MsgErro = msgErro.Replace("{0}", Cadeia).Replace("{1}", Token.Linha.ToString());
        }

        public void SetMsgErro(string msgErro, Simbolo sim, Simbolo param)
        {
            MsgErro = msgErro.Replace("{0}", sim.Cadeia).Replace("{1}", Cadeia).Replace("{2}", param.Tipo).Replace("{3}", sim.Tipo).Replace("{4}", sim.Token.Linha.ToString());
        }

        public void SetMsgErro(string msgErro, int esperado, int encontrado)
        {
            MsgErro = msgErro.Replace("{0}", Cadeia).Replace("{1}", esperado.ToString()).Replace("{2}", encontrado.ToString()).Replace("{3}", Token.Linha.ToString());
        }

        public void SetMsgErro(string msgErro, Simbolo s)
        {
            MsgErro = msgErro.Replace("{0}", Cadeia).Replace("{1}", Tipo).Replace("{2}", s.Tipo).Replace("{3}", s.Token.Linha.ToString());
        }

        public void SetMsgErro(string msgErro, string tipo)
        {
            MsgErro = msgErro.Replace("{0}", Cadeia).Replace("{1}", tipo).Replace("{2}", Token.Linha.ToString());
        }
        #endregion
    }
}
