using SimpleCompilerService.Suporte;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompilerService.Analisador
{
    public class Lexico
    {
        #region 1. Propriedades e Cosntrutores
        public static Queue<char> Texto;
        public static Queue<Token> Tokens;
        private static Char? Peek;
        private static int Linha;

        private Lexico() { }
        #endregion

        #region 2. Métodos Públicos 
        public static void ScanearTexto(string str)
        {
            #region 2.1 Inicialização de Variáveis
            Texto = ParseToCharQueue(str);
            Tokens = new Queue<Token>();
            Linha = 1;
            Token t;
            NextChar();
            #endregion
            while (Peek != null)
            {
                #region 2.2 Tratamento de quebra de linhas, comentários, espaços e tabulações
                if (Peek == '\n')
                {
                    Linha++;
                    NextChar();
                    continue;
                }
                else if (Peek == '{')
                {
                    while (Peek != null)
                    {
                        NextChar();
                        if (Peek == '}')
                        {
                            break;
                        }
                    }
                    NextChar();
                    continue;
                }
                else if (Peek == '/' && NextCharIs('*'))
                {
                    NextChar(); NextChar();
                    while (Peek != null)
                    {
                        NextChar();
                        if (Peek == '*' && NextCharIs('/'))
                        {
                            NextChar();
                            break;
                        }
                    }
                    NextChar();
                    continue;
                }
                else if (Peek == ' ' || Peek == '\t')
                {
                    NextChar();
                    continue;
                }
                #endregion

                #region 2.3 Constrói fila de Tokens
                t = ConstroiToken();
                Tokens.Enqueue(t);
                if (t.Tag == Tag.OPERADOR || t.Tag == Tag.SIMBOLO_DUPLO || t.Tag == Tag.SIMBOLO_SIMPLES)
                {
                    NextChar();
                }
                #endregion
            }
        }
        #endregion

        #region 3. Métodos Privados
        private static Token ConstroiToken()
        {
            #region 3.1 Constrói Tokens de Simbolos Simples e Duplos
            switch (Peek)
            {
                case '.':
                    return new Token('.', Tag.SIMBOLO_SIMPLES, Linha);
                case ',':
                    return new Token(',', Tag.SIMBOLO_SIMPLES, Linha);
                case '(':
                    return new Token('(', Tag.SIMBOLO_SIMPLES, Linha);
                case ')':
                    return new Token(')', Tag.SIMBOLO_SIMPLES, Linha);
                case ';':
                    return new Token(';', Tag.SIMBOLO_SIMPLES, Linha);
                case '=':
                    return new Token('=', Tag.SIMBOLO_SIMPLES, Linha);
                case '+':
                    return new Token('+', Tag.OPERADOR, Linha);
                case '-':
                    return new Token('-', Tag.OPERADOR, Linha);
                case '*':
                    return new Token('*', Tag.OPERADOR, Linha);
                case '/':
                    return new Token('/', Tag.OPERADOR, Linha);
                case ':':
                    if (NextCharIs('='))
                    {
                        NextChar();
                        return new Token(":=", Tag.SIMBOLO_DUPLO, Linha);
                    }
                    else
                    {
                        return new Token(':', Tag.SIMBOLO_SIMPLES, Linha);
                    }
                case '>':
                    if (NextCharIs('='))
                    {
                        NextChar();
                        return new Token(">=", Tag.SIMBOLO_DUPLO, Linha);
                    }
                    else
                    {
                        return new Token('>', Tag.SIMBOLO_SIMPLES, Linha);
                    }
                case '<':
                    if (NextCharIs('='))
                    {
                        NextChar();
                        return new Token("<=", Tag.SIMBOLO_DUPLO, Linha);
                            
                    }
                    else if (NextCharIs('>'))
                    {
                        NextChar();
                        return new Token("<>", Tag.SIMBOLO_DUPLO, Linha);
                    }
                    else
                    {
                        return new Token('<', Tag.SIMBOLO_SIMPLES, Linha);
                    }
            }
            #endregion

            #region 3.2 Constrói Tokens Numéricos
            if (Char.IsDigit((char)Peek))
            {
                int v = 0;
                do
                {
                    v = 10 * v + int.Parse(Peek.ToString());
                    NextChar();
                } while (Peek != null && Char.IsDigit((char)Peek));
                if (Peek != '.')
                {
                    return new Token(v, Tag.NUMERO_INTEIRO, Linha);
                }
                float x = v;
                float d = 10;
                do
                {
                    x = x + float.Parse(Peek.ToString()) / d;
                    d = d * 10;
                    NextChar();
                } while (Peek != null && Char.IsDigit((char) Peek));
                return new Token(x, Tag.NUMERO_REAL, Linha);
            }
            #endregion

            #region 3.3 Constói Tokens de Palavras Reservadas e Identificadores
            if (Char.IsLetter((char)Peek))
            {
                StringBuilder sb = new StringBuilder();
                do
                {
                    sb.Append(Peek);
                    NextChar();
                } while (Peek != null && Char.IsLetterOrDigit((char)Peek));
                string lexema = sb.ToString();
                if (EhPalavraReservada(lexema))
                {
                    return new Token(lexema, Tag.PALAVRA_RESERVADA, Linha);
                }
                return new Token(lexema, Tag.IDENTIFICADOR, Linha);
            }
            #endregion

            return null;
        }

        private static void NextChar()
        {
            if (Texto.Any())
            {
                Peek = Texto.Dequeue();
            }
            else
            {
                Peek = null;
            }
        }

        private static bool NextCharIs(char c)
        {
            char b = Texto.Peek();
            if (b != c)
            {
                return false;
            }
            Peek = ' ';
            return true;
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

        private static bool EhPalavraReservada(string lexema)
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
