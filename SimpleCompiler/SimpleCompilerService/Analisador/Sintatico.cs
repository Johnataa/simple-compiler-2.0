using SimpleCompilerService.Suporte;
using System.Collections.Generic;

namespace SimpleCompilerService.Analisador
{
    public class Sintatico
    {
        #region 1. Propriedades
        private static Token CurrentToken;
        public static Dictionary<Token, object> Erros { get; set; }
        #endregion

        #region 2. Métodos Públicos
        public static void Analyze()
        {
            Erros = new Dictionary<Token, object>();
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
                    Corpo();
                    CurrentTokenIs('.');
                }
            }
        }

        private static void Corpo()
        {
            Dc();
            if (CurrentTokenIs("begin"))
            {
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
                    Variaveis();
                    CurrentTokenIs(')');
                }
            }
            else if(CurrentToken.Tag == Tag.IDENTIFICADOR)
            {
                RestoIdent();
            }
            else
            {
                Error("if, while, read, write, Identificador");
            }
        }

        private static void RestoIdent()
        {
            if (Lexico.NextTokenIs(":="))
            {
                CurrentToken = Lexico.NextToken();
                Expressao();
            }
            Lista_arg();
        }

        private static void Lista_arg()
        {
            if (Lexico.NextTokenIs('('))
            {
                CurrentToken = Lexico.NextToken();
                Argumentos();
                CurrentTokenIs(')');
            }
        }

        private static void Argumentos()
        {
            if (CurrentTokenIs(Tag.IDENTIFICADOR))
            {
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

        private static void Expressao()
        {
            Termo();
            Outros_termos();
        }

        private static void Termo()
        {
            Op_un();
            Fator();
            Mais_fatores();
        }

        private static void Mais_fatores()
        {
            if (Lexico.NextTokenIs('*') || Lexico.NextTokenIs('/'))
            {
                Op_mul();
                Fator();
                Mais_fatores();
            }            
        }

        private static void Op_mul()
        {
            CurrentTokenIs('*', '/');
        }

        private static void Fator()
        {
            CurrentToken = Lexico.NextToken();
            if (CurrentToken.Equals('('))
            {
                Expressao();
                CurrentTokenIs(')');
            }
            else if (CurrentToken.Tag != Tag.NUMERO_INTEIRO && CurrentToken.Tag != Tag.NUMERO_REAL && CurrentToken.Tag != Tag.IDENTIFICADOR)
            {
                Error("N° inteiro, N° real ou identificador");
            }
        }

        private static void Op_un()
        {
            if (Lexico.NextTokenIs('+') || Lexico.NextTokenIs('-'))
            {
                CurrentToken = Lexico.NextToken();
            }
        }

        private static void Outros_termos()
        {
            if (Lexico.NextTokenIs('+') || Lexico.NextTokenIs('-'))
            {
                Op_ad();
                Termo();
                Outros_termos();
            }
        }

        private static void Op_ad()
        {
            CurrentTokenIs('+','-');
        }

        private static void Pfalsa()
        {
            if (Lexico.NextTokenIs("else"))
            {
                Comandos();
            }
        }

        private static void Condicao()
        {
            Expressao();
            Relacao();
            Expressao();
        }

        private static void Relacao()
        {
            CurrentTokenIs('<', "<=", "<>", '=', ">=", '>');
        }

        private static void Dc()
        {
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
                if (CurrentTokenIs(Tag.IDENTIFICADOR))
                {
                    Parametros();
                    Corpo_p();
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
        }

        private static void Variaveis()
        {
            if (CurrentTokenIs(Tag.IDENTIFICADOR))
            {
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
            Erros.Add(CurrentToken, expected);
        }
        #endregion
    }
}
