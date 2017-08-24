using SimpleCompilerService.Suporte;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCompilerService.Analisador
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
            MaqHip = new MaquinaHipotetica();
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
                if (CurrentTokenIs("then"))
                {
                    Comandos();
                    Pfalsa();
                    CurrentTokenIs('$');
                }
            }
            else if (CurrentToken.Equals("while"))
            {
                Condicao();
                if (CurrentTokenIs("do"))
                {
                    Comandos();
                    CurrentTokenIs('$');
                }
            }
            else if(CurrentToken.Equals("read") || CurrentToken.Equals("write"))
            {
                if (CurrentTokenIs('('))
                {

                    Variaveis(false);
                    var instrucao = "CRVL ";
                    if (CurrentToken.Equals("read"))
                    {
                        MaqHip.C.Add("LEIT");
                        instrucao = "ARMZ ";
                    }
                    while (FilaSimbolos.Any())
                    {
                        var simbolo = FilaSimbolos.Dequeue();
                        var s = TabelaDeSimbolos.Busca(simbolo);
                        if (s == null)
                        {
                            simbolo.SetMsgErro(MsgErrosSemanticos.NAO_DECLARADO);
                            Error(simbolo);
                        }
                        MaqHip.C.Add(instrucao + s.EnderecoRelativo);
                    }
                    if (instrucao == "CRVL ")
                    {
                        MaqHip.C.Add("IMPR");
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
            }
            else
            {
                Lista_arg(pEsq);
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
            Op_un();
            var fDir = Fator(pEsq);
            var mDir = Mais_fatores(fDir);
            return mDir;
        }

        private static Simbolo Mais_fatores(Simbolo pEsq)
        {
            if (Lexico.NextTokenIs('*') || Lexico.NextTokenIs('/'))
            {
                Op_mul();
                var fDir = Fator(pEsq);
                var mDir = Mais_fatores(fDir);
                return mDir;
            }
            return pEsq;
        }

        private static void Op_mul()
        {
            CurrentTokenIs('*', '/');
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
                    return pEsq;
                }
                return simbolo;
            }
            else if (CurrentToken.Tag == Tag.NUMERO_INTEIRO)
            {
                var s = new Simbolo(CurrentToken, Escopo, "", CurrentToken.Lexema, -1);
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

        private static void Op_un()
        {
            if (Lexico.NextTokenIs('+') || Lexico.NextTokenIs('-'))
            {
                CurrentToken = Lexico.NextToken();
            }
        }

        private static Simbolo Outros_termos(Simbolo pEsq)
        {
            if (Lexico.NextTokenIs('+') || Lexico.NextTokenIs('-'))
            {
                Op_ad();
                var tDir = Termo(pEsq);
                var oDir = Outros_termos(tDir);
                return oDir;
            }
            return pEsq;
        }

        private static void Op_ad()
        {
            CurrentTokenIs('+','-');
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
            Relacao();
            Expressao(eDir);
        }

        private static void Relacao()
        {
            CurrentTokenIs('<', "<=", "<>", '=', ">=", '>');
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
                    MaqHip.C[i] = "DSVI " + MaqHip.C.Count();
                    i = TabelaDeSimbolos.BuscaParametros(simbolo).Count() + 1;
                    MaqHip.C.Add("DESM " + i);
                    MaqHip.C.Add("RTPR");
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
            Variaveis();
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
                if (novo)
                {
                    simbolo.EnderecoRelativo = EnderecoRelativo++;
                }
                FilaSimbolos.Enqueue(simbolo);
                Mais_var();
            }
        }

        private static void Mais_var()
        {
            if (Lexico.NextTokenIs(','))
            {
                CurrentToken = Lexico.NextToken();
                Variaveis();
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
