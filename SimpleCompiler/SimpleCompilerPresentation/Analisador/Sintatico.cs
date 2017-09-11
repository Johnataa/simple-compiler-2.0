using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCompiler
{
    public class Sintatico
    {
        #region 1. Propriedades
        private static Token CurrentToken;

        private static TabelaDeSimbolos TabelaDeSimbolos;
        private static Queue<Simbolo> FilaSimbolos;
        private static string Categoria;
        private static string Escopo;

        private static int EnderecoRelativo;
        private static MaquinaHipotetica MaqHip;
        #endregion

        #region 2. Métodos Públicos
        public static void Analyze()
        {
            TabelaDeSimbolos = new TabelaDeSimbolos();
            FilaSimbolos = new Queue<Simbolo>();
            MaqHip = MaquinaHipotetica.GetInstance(true);
            EnderecoRelativo = 0;
            Programa();
        }

        #endregion

        #region 3. Métodos Privados
        private static void Programa()
        {
            if (CurrentTokenIs("program"))
            {
                if (CurrentTokenIs(Tag.IDENTIFICADOR))
                {
                    MaqHip.C.Add("INPP");
                    Corpo();
                    if (CurrentTokenIs('.'))
                    {
                        MaqHip.C.Add("PARA");
                    }
                }
            }
        }

        private static void Corpo()
        {
            Dc();
            if (CurrentTokenIs("begin"))
            {
                Escopo = "";
                Comandos();
                CurrentTokenIs("end");
            }
        }

        private static void Comandos()
        {
            Comando();
            Mais_comandos();
        }

        private static void Mais_comandos()
        {
            if (Lexico.NextTokenIs(';'))
            {
                CurrentToken = Lexico.NextToken();
                Comandos();
            }
        }

        private static void Comando()
        {
            CurrentToken = Lexico.NextToken();
            if (CurrentToken.Equals("if"))
            {
                Condicao();
                int pos = MaqHip.C.Count();
                MaqHip.C.Add("DSVF");
                if (CurrentTokenIs("then"))
                {
                    Comandos();
                    MaqHip.C[pos] = "DSVF " + (MaqHip.C.Count()+1);
                    pos = MaqHip.C.Count();
                    MaqHip.C.Add("DSVI");
                    Pfalsa();
                    MaqHip.C[pos] = "DSVI " + MaqHip.C.Count();
                    CurrentTokenIs('$');
                }
            }
            else if (CurrentToken.Equals("while"))
            {
                int inicio = MaqHip.C.Count();
                Condicao();
                if (CurrentTokenIs("do"))
                {
                    int loop = MaqHip.C.Count();
                    MaqHip.C.Add("DSVF");
                    Comandos();
                    MaqHip.C.Add("DSVI " + inicio);
                    MaqHip.C[loop] = "DSVF " + MaqHip.C.Count();
                    CurrentTokenIs('$');
                }
            }
            else if(CurrentToken.Equals("read") || CurrentToken.Equals("write"))
            {
                var isRead = CurrentToken.Equals("read");
                if (CurrentTokenIs('('))
                {

                    Variaveis(false);
                    var tipo = "";
                    while (FilaSimbolos.Any())
                    {
                        var simbolo = FilaSimbolos.Dequeue();
                        var s = TabelaDeSimbolos.Busca(simbolo);
                        if (s == null)
                        {
                            simbolo.SetMsgErro(MsgErrosSemanticos.NAO_DECLARADO);
                            Error(simbolo);
                        }
                        if (tipo == "")
                        {
                            tipo = s.Tipo;
                        }
                        else if (tipo != s.Tipo)
                        {
                            simbolo.SetMsgErro(MsgErrosSemanticos.TIPOS_DIFERENTES, tipo);
                            Error(simbolo);
                        }
                        if (isRead)
                        {
                            MaqHip.C.Add("LEIT");
                            MaqHip.C.Add("ARMZ " + s.EnderecoRelativo);
                        }
                        else
                        {
                            MaqHip.C.Add("CRVL " + s.EnderecoRelativo);
                            MaqHip.C.Add("IMPR");
                        }
                        
                    }
                    CurrentTokenIs(')');
                }
            }
            else if(CurrentToken.Tag == Tag.IDENTIFICADOR)
            {
                var simbolo = TabelaDeSimbolos.Busca(CurrentToken.Lexema, Escopo);
                if (simbolo == null)
                {
                    Error(new Simbolo(CurrentToken, MsgErrosSemanticos.NAO_DECLARADO));
                }
                simbolo.Token.Linha = CurrentToken.Linha;
                RestoIdent(simbolo);

            }
            else
            {
                Error("if, while, read, write, Identificador");
            }
        }

        private static void RestoIdent(Simbolo pEsq)
        {
            if (Lexico.NextTokenIs(":="))
            {
                CurrentToken = Lexico.NextToken();
                Expressao(pEsq);
                MaqHip.C.Add("ARMZ " + pEsq.EnderecoRelativo);
            }
            else
            {
                int retorno = MaqHip.C.Count();
                MaqHip.C.Add("PUSHER");
                Lista_arg(pEsq);
                MaqHip.C.Add("CHPR " + pEsq.PrimeiraInstrucao);
                MaqHip.C[retorno] = "PUSHER " + MaqHip.C.Count();
            }            
        }

        private static void Lista_arg(Simbolo pEsq)
        {
            var parametros = TabelaDeSimbolos.BuscaParametros(pEsq);
            if (Lexico.NextTokenIs('('))
            {
                CurrentToken = Lexico.NextToken();
                Argumentos();
                
                if (parametros.Count() != FilaSimbolos.Count())
                {
                    pEsq.SetMsgErro(MsgErrosSemanticos.PARAMETROS_INCORRETOS, parametros.Count(), FilaSimbolos.Count());
                    Error(pEsq);
                }
                while (FilaSimbolos.Any())
                {
                    var sim = FilaSimbolos.Dequeue();
                    var param = parametros.Dequeue();
                    if (sim.Tipo != param.Tipo)
                    {
                        pEsq.SetMsgErro(MsgErrosSemanticos.PARAMETRO_ERRADO, sim, param);
                        Error(pEsq);
                    }
                    MaqHip.C.Add("PARAM " + sim.EnderecoRelativo);
                }
                CurrentTokenIs(')');
            }
            if (parametros.Count() != 0)
            {
                pEsq.SetMsgErro(MsgErrosSemanticos.PARAMETROS_INCORRETOS, parametros.Count(), 0);
                Error(pEsq);
            }
        }

        private static void Argumentos()
        {
            if (CurrentTokenIs(Tag.IDENTIFICADOR))
            {
                var simbolo = TabelaDeSimbolos.Busca(CurrentToken.Lexema, Escopo);
                if (simbolo == null)
                {
                    Error(new Simbolo(CurrentToken, MsgErrosSemanticos.NAO_DECLARADO));
                }
                FilaSimbolos.Enqueue(simbolo);
                Mais_ident();
            }
        }

        private static void Mais_ident()
        {
            if (Lexico.NextTokenIs(';'))
            {
                CurrentToken = Lexico.NextToken();
                Argumentos();
            }
        }

        private static Simbolo Expressao(Simbolo pEsq)
        {
            var tDir = Termo(pEsq);
            var oDir = Outros_termos(tDir);
            return oDir;
        }

        private static Simbolo Termo(Simbolo pEsq)
        {
            var sinal = Op_un();
            var fDir = Fator(pEsq);
            if (sinal != null && sinal == '-')
            {
                MaqHip.C.Add("INVE");
            }
            var mDir = Mais_fatores(fDir);
            return mDir;
        }

        private static Simbolo Mais_fatores(Simbolo pEsq)
        {
            if (Lexico.NextTokenIs('*') || Lexico.NextTokenIs('/'))
            {
                var inst = Op_mul();
                var fDir = Fator(pEsq);
                MaqHip.C.Add(inst);
                var mDir = Mais_fatores(fDir);
                return mDir;
            }
            return pEsq;
        }

        private static string Op_mul()
        {
            CurrentTokenIs('*', '/');
            return CurrentToken.Equals('*') ? "MULT" : "DIVI";
        }

        private static Simbolo Fator(Simbolo pEsq)
        {
            CurrentToken = Lexico.NextToken();
            if (CurrentToken.Equals('('))
            {
                Expressao(pEsq);
                CurrentTokenIs(')');
            }
            else if (CurrentToken.Tag == Tag.IDENTIFICADOR)
            {
                var simbolo = TabelaDeSimbolos.Busca(CurrentToken.Lexema, Escopo);
                if (simbolo == null)
                {
                    Error(new Simbolo(CurrentToken, MsgErrosSemanticos.NAO_DECLARADO));
                }
                if(simbolo.Categoria == "procedure")
                {
                    Error(new Simbolo(CurrentToken, MsgErrosSemanticos.NAO_DECLARADO));
                }
                simbolo.Token.Linha = CurrentToken.Linha;
                if (pEsq != null)
                {
                    if (simbolo.Tipo != pEsq.Tipo)
                    {
                        pEsq.SetMsgErro(MsgErrosSemanticos.ATRIBUICAO_ERRADA, simbolo);
                        Error(pEsq);
                    }
                    pEsq.Token.Linha = CurrentToken.Linha;
                    MaqHip.C.Add("CRVL " + simbolo.EnderecoRelativo);
                    return pEsq;
                }
                MaqHip.C.Add("CRVL " + simbolo.EnderecoRelativo);
                return simbolo;
            }
            else if (CurrentToken.Tag == Tag.NUMERO_INTEIRO)
            {
                var s = new Simbolo(CurrentToken, Escopo, "", CurrentToken.Lexema, -1);
                MaqHip.C.Add("CRCT " + s.Valor);
                s.Tipo = CurrentToken.GetTagDescription();
                if (pEsq != null)
                {
                    if (pEsq.Tipo != "integer")
                    {
                        pEsq.SetMsgErro(MsgErrosSemanticos.ATRIBUICAO_ERRADA, s);
                        Error(pEsq);
                    }
                    pEsq.Token.Linha = CurrentToken.Linha;
                    return pEsq;
                }
                return s;
            }
            else if (CurrentToken.Tag == Tag.NUMERO_REAL)
            {
                var s = new Simbolo(CurrentToken, Escopo, "", CurrentToken.Lexema, -1);
                MaqHip.C.Add("CRCT " + s.Valor);
                s.Tipo = CurrentToken.GetTagDescription();
                if (pEsq != null)
                {
                    if (pEsq.Tipo != "real")
                    {
                        pEsq.SetMsgErro(MsgErrosSemanticos.ATRIBUICAO_ERRADA, s);
                        Error(pEsq);
                    }
                    pEsq.Token.Linha = CurrentToken.Linha;
                    return pEsq;
                }
                return s;
            }
            else
            {
                Error("N° inteiro, N° real ou identificador");
            }
            return pEsq;
        }

        private static char? Op_un()
        {
            if (Lexico.NextTokenIs('+') || Lexico.NextTokenIs('-'))
            {
                CurrentToken = Lexico.NextToken();
                return (char)CurrentToken.Lexema;
            }
            return null;
        }

        private static Simbolo Outros_termos(Simbolo pEsq)
        {
            if (Lexico.NextTokenIs('+') || Lexico.NextTokenIs('-'))
            {
                var inst = Op_ad();
                var tDir = Termo(pEsq);
                MaqHip.C.Add(inst);
                var oDir = Outros_termos(tDir);
                return oDir;
            }
            return pEsq;
        }

        private static string Op_ad()
        {
            CurrentTokenIs('+','-');
            return CurrentToken.Equals('+') ? "SOMA" : "SUBT";
        }

        private static void Pfalsa()
        {
            if (Lexico.NextTokenIs("else"))
            {
                CurrentToken = Lexico.NextToken();
                Comandos();
            }
        }

        private static void Condicao()
        {
            var eDir = Expressao(null);
            var inst = Relacao();
            Expressao(eDir);
            MaqHip.C.Add(inst);
        }

        private static string Relacao()
        {
            CurrentTokenIs('<', "<=", "<>", '=', ">=", '>');
            if (CurrentToken.Equals('<'))
            {
                return "CPME";
            }
            else if (CurrentToken.Equals("<="))
            {
                return "CPMI";
            }
            else if (CurrentToken.Equals("<>"))
            {
                return "CDES";
            }
            else if (CurrentToken.Equals('='))
            {
                return "CPIG";
            }
            else if (CurrentToken.Equals(">="))
            {
                return "CMAI";
            }
            else
            {
                return "CPMA";
            }
        }

        private static void Dc()
        {
            Escopo = "";
            if (Lexico.NextTokenIs("var"))
            {
                Dc_v();
                Mais_dc();
            }
            else if (Lexico.NextTokenIs("procedure"))
            {
                Dc_p();
                Mais_dc();
            }
        }

        private static void Dc_p()
        {
            if (CurrentTokenIs("procedure"))
            {
                Categoria = "procedure";
                if (CurrentTokenIs(Tag.IDENTIFICADOR))
                {
                    var i = MaqHip.C.Count();
                    MaqHip.C.Add("DSVI");
                    var simbolo = new Simbolo(CurrentToken, Escopo, Categoria, null, EnderecoRelativo++, i+1);
                    var adicionou = TabelaDeSimbolos.Insere(simbolo) == null;
                    if (!adicionou)
                    {
                        simbolo.SetMsgErro(MsgErrosSemanticos.JA_DECLARADO);
                        Error(simbolo);
                    }
                    Escopo = simbolo.Cadeia;
                    Parametros();
                    Corpo_p();
                    var j = TabelaDeSimbolos.CountParametros(simbolo) + TabelaDeSimbolos.CountVariaveis(simbolo);
                    MaqHip.C.Add("DESM " + j);
                    MaqHip.C.Add("RTPR");
                    MaqHip.C[i] = "DSVI " + MaqHip.C.Count();
                }
            }
        }

        private static void Corpo_p()
        {
            Dc_loc();
            if (CurrentTokenIs("begin"))
            {
                Comandos();
                CurrentTokenIs("end");
            }
        }

        private static void Dc_loc()
        {
            if (Lexico.NextTokenIs("var"))
            {
                Dc_v();
                Mais_dcloc();
            }
        }

        private static void Mais_dcloc()
        {
            if (Lexico.NextTokenIs(';'))
            {
                CurrentToken = Lexico.NextToken();
                Dc_loc();
            }
        }

        private static void Parametros()
        {
            if (Lexico.NextTokenIs('('))
            {
                CurrentToken = Lexico.NextToken();
                Categoria = "param";
                Lista_par();
                CurrentTokenIs(')');
            }
        }

        private static void Lista_par()
        {
            Variaveis(false);
            if (CurrentTokenIs(':'))
            {
                Tipo_var();
                Mais_par();
            }
        }

        private static void Mais_par()
        {
            if (Lexico.NextTokenIs(';'))
            {
                CurrentToken = Lexico.NextToken();
                Lista_par();
            }
        }

        private static void Mais_dc()
        {
            if (Lexico.NextTokenIs(';'))
            {
                CurrentToken = Lexico.NextToken();
                Dc();
            }
        }

        private static void Dc_v()
        {
            if (CurrentTokenIs("var"))
            {
                Categoria = "var";
                Variaveis();
                if (CurrentTokenIs(':'))
                {
                    Tipo_var();
                }
            }
        }

        private static void Tipo_var()
        {
            CurrentTokenIs("real", "integer");
            var result = TabelaDeSimbolos.Insere(ref FilaSimbolos, CurrentToken.Lexema.ToString());
            if (result != null)
            {
                Error(result);
            }
        }

        private static void Variaveis(bool novo = true)
        {
            if (CurrentTokenIs(Tag.IDENTIFICADOR))
            {
                var simbolo = new Simbolo(CurrentToken, Escopo, Categoria, null, -1);
                simbolo.EnderecoRelativo = EnderecoRelativo++;
                if (novo)
                {
                    MaqHip.C.Add("ALME 1");
                }
                FilaSimbolos.Enqueue(simbolo);
                Mais_var(novo);
            }
        }

        private static void Mais_var(bool novo = true)
        {
            if (Lexico.NextTokenIs(','))
            {
                CurrentToken = Lexico.NextToken();
                Variaveis(novo);
            }
        }

        private static bool CurrentTokenIs(params object[] objs)
        {
            CurrentToken = Lexico.NextToken();
            string erros = "";
            foreach (var item in objs)
            {
                if (item is Tag)
                {
                    if (CurrentToken.Tag == (Tag)item)
                    {
                        return true;
                    }
                    erros += item + ", ";
                }
                else if (CurrentToken.Equals(item))
                {
                    return true;
                }
                else
                {
                    erros += item + ", ";
                }
            }
            erros += "a";
            erros = erros.Replace(", a", string.Empty);
            Error(erros);
            return false;
        }

        private static void Error(object expected)
        {
            string msg = "#sintatico#Esperado: " + expected.ToString() + "\r\nEncontrado: " + CurrentToken.Lexema + "\r\nLinha:" + CurrentToken.Linha;
            throw new Exception(msg);
        }

        private static void Error(Simbolo simbolo)
        {
            throw new Exception(simbolo.MsgErro);
        }
        #endregion
    }
}
