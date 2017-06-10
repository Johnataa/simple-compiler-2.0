using SimpleCompilerService.Suporte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCompilerService.Analisador
{
    public class Lexico
    {
        #region 1. Propriedades
        private static Queue<char> Texto;
        public static Queue<Token> Tokens { get; set; }
        public static bool ContemErroLexico { get; set; }
        private static char Peek;

        private static int Linha;
        #endregion

        #region 2. Métodos Públicos 
        public static void ScanText(string str)
        {
            #region 2.1 Inicialização de Variáveis
            Texto = ParseToCharQueue(str);
            Tokens = new Queue<Token>();
            ContemErroLexico = false;
            Linha = 1;
            Token t = null;
            Peek = NextChar();
            #endregion

            while (Texto.Any())
            {
                #region 2.2 Simbolos simples, duplos, comentários, quebra de linhas e tabulações
                if (Peek == '\n')
                {
                    Linha++;
                    Peek = NextChar();
                    continue;
                }
                else if (Peek == ' ' || Peek == '\t' || Peek == '\r')
                {
                    Peek = NextChar();
                    continue;
                }
                else if (Peek == '.' || Peek == ',' || Peek == '(' || Peek == ')' || Peek == ';' || Peek == '=')
                {
                    t = new Token(Peek, Tag.SIMBOLO_SIMPLES, Linha);
                    Peek = NextChar();
                }
                else if (Peek == '+' || Peek == '-' || Peek == '*')
                {
                    t = new Token(Peek, Tag.OPERADOR, Linha);
                    Peek = NextChar();
                }
                else if (Peek == '/')
                {
                    Peek = NextChar();
                    if (Peek == '*')
                    {
                        while (Texto.Any())
                        {
                            var linha = 0;
                            Peek = NextChar();
                            if (Peek == '\n')
                            {
                                linha++;
                                Peek = NextChar();
                                continue;
                            }
                            if (Peek == '*')
                            {
                                Peek = NextChar();
                                if (Peek == '/')
                                {
                                    Peek = NextChar();
                                    Linha += linha;
                                    break;
                                }
                            }
                        }
                        if (!Texto.Any())
                        {
                            ContemErroLexico = true;
                            t = new Token("Comentário mal formado!", Tag.ERRO_LEXICO, Linha);
                        }
                    }
                    else
                    {
                        t = new Token('/', Tag.OPERADOR, Linha);
                    }
                }
                else if (Peek == '{')
                {
                    while (Texto.Any())
                    {
                        var linha = 0;
                        Peek = NextChar();
                        if (Peek == '\n')
                        {
                            linha++;
                            Peek = NextChar();
                            continue;
                        }
                        if (Peek == '}')
                        {
                            Peek = NextChar();
                            Linha += linha;
                            break;
                        }
                    }
                    if (!Texto.Any())
                    {
                        ContemErroLexico = true;
                        t = new Token("Comentário mal formado!", Tag.ERRO_LEXICO, Linha);
                    }
                }
                else if (Peek == ':')
                {
                    Peek = NextChar();
                    if (Peek == '=')
                    {
                        Peek = NextChar();
                        t = new Token(":=", Tag.SIMBOLO_DUPLO, Linha);
                    }
                    else
                    {
                        t = new Token(":", Tag.SIMBOLO_SIMPLES, Linha);
                    }
                }
                else if (Peek == '>')
                {
                    Peek = NextChar();
                    if (Peek == '=')
                    {
                        NextChar();
                        t = new Token(">=", Tag.SIMBOLO_DUPLO, Linha);
                    }
                    else
                    {
                        t = new Token('>', Tag.SIMBOLO_SIMPLES, Linha);
                    }
                }
                else if (Peek == '<')
                {
                    Peek = NextChar();
                    if (Peek == '=')
                    {
                        NextChar();
                        t = new Token("<=", Tag.SIMBOLO_DUPLO, Linha);

                    }
                    else if (Peek == '>')
                    {
                        NextChar();
                        t = new Token("<>", Tag.SIMBOLO_DUPLO, Linha);
                    }
                    else
                    {
                        t= new Token('<', Tag.SIMBOLO_SIMPLES, Linha);
                    }
                }
                else if(!Char.IsLetterOrDigit(Peek))
                {
                    ContemErroLexico = true;
                    t = new Token(Peek, Tag.ERRO_LEXICO, Linha);
                    Peek = NextChar();
                }
                if(t != null)
                {
                    Tokens.Enqueue(t);
                    t = null;
                }
                #endregion

                #region 2.3 Tokens Numéricos
                if (Char.IsDigit(Peek))
                {
                    int v = 0;
                    do
                    {
                        v = 10 * v + int.Parse(Peek.ToString());
                        Peek = NextChar();
                        if (!Char.IsDigit(Peek))
                        {
                            break;
                        }
                    } while (Tokens.Any());

                    if (Peek != '.')
                    {
                        t = new Token(v, Tag.NUMERO_INTEIRO, Linha);
                    }
                    else
                    {
                        Peek = NextChar();
                        if (Char.IsDigit(Peek))
                        {
                            float x = v;
                            float d = 10;
                            do
                            {
                                x = x + float.Parse(Peek.ToString()) / d;
                                d = d * 10;
                                Peek = NextChar();
                                if (!Char.IsDigit(Peek))
                                {
                                    break;
                                }
                            } while (Tokens.Any());
                            t = new Token(x, Tag.NUMERO_REAL, Linha);
                        }
                        else
                        {
                            ContemErroLexico = true;
                            t = new Token(v.ToString() + '.', Tag.ERRO_LEXICO, Linha);
                        }
                    }
                }
                if (t != null)
                {
                    Tokens.Enqueue(t);
                    t = null;
                }
                #endregion

                #region 2.3 Tokens de Palavras Reservadas e Identificadores
                if (Char.IsLetter(Peek))
                {
                    string lexema = BuildStringLexeme();
                    if (IsReservedWord(lexema))
                    {
                        t = new Token(lexema, Tag.PALAVRA_RESERVADA, Linha);
                    }
                    else
                    {
                        t = new Token(lexema, Tag.IDENTIFICADOR, Linha);
                    }
                }
                if (t != null)
                {
                    Tokens.Enqueue(t);
                    t = null;
                }
                #endregion
            }
        }

        public static Token NextToken()
        {
            if (Tokens != null && Tokens.Any())
            {
                return Tokens.Dequeue();
            }
            return null;
        }

        public static bool NextTokenIs(object obj)
        {
            if (Tokens != null && Tokens.Any())
            {
                return Tokens.Peek().Equals(obj);
            }
            return false;
        }
        #endregion

        #region 3. Métodos Privados
        private static string BuildStringLexeme(string start = "")
        {
            StringBuilder sb = new StringBuilder();
            do
            {
                sb.Append(Peek);
                Peek = NextChar();
                if (!Char.IsLetterOrDigit(Peek))
                {
                    break;
                }
            } while (Texto.Any());
            return start + sb.ToString();
        }

        private static char NextChar()
        {
            return Texto.Dequeue();
        }

        private static Queue<char> ParseToCharQueue(string str)
        {
            Queue<char> fc = new Queue<char>();
            foreach (var c in str)
            {
                fc.Enqueue(c);
            }
            return fc;
        }

        private static bool IsReservedWord(string lexema)
        {
            string[] lexemas = "var program procedure if then while do write read else begin end integer real".Split(' ');
            foreach (var lex in lexemas)
            {
                if(lex == lexema)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
