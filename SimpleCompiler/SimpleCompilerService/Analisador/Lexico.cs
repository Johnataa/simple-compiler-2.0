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
        private static Queue<char> Texto;
        private static Queue<Token> Tokens;
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

            do
            {
                #region 2.2 Tratamento de quebra de linhas, comentários, espaços e tabulações
                while (Peek != null)
                {
                    if (Peek == '\n')
                    {
                        Linha++;
                    }
                    else if (Peek == '{')
                    {
                        while (Peek != null)
                        {
                            if (Peek == '}')
                            {
                                break;
                            }
                            NextChar();
                        }
                    } 
                    else if (Peek == '/' && NextCharIs('*'))
                    {
                        NextChar(); NextChar();
                        while(Peek != null)
                        {
                            if(Peek == '*' && NextCharIs('/'))
                            {
                                break;
                            }
                            NextChar();
                        }
                    }
                    else if (Peek != ' ' && Peek != '\t')
                    {
                        break;
                    }
                    NextChar();
                }
                #endregion

                #region 2.3 Criação de Tokens de Simbolos Simples e Duplos
                switch (Peek)
                {
                    case '.':
                        t = new Token('.', Tag.SIMBOLO_SIMPLES, Linha);
                        Tokens.Enqueue(t);
                        break;
                    case ',':
                        t = new Token(',', Tag.SIMBOLO_SIMPLES, Linha);
                        Tokens.Enqueue(t);
                        break;
                    case '(':
                        t = new Token('(', Tag.SIMBOLO_SIMPLES, Linha);
                        Tokens.Enqueue(t);
                        break;
                    case ')':
                        t = new Token(')', Tag.SIMBOLO_SIMPLES, Linha);
                        Tokens.Enqueue(t);
                        break;
                    case ';':
                        t = new Token(';', Tag.SIMBOLO_SIMPLES, Linha);
                        Tokens.Enqueue(t);
                        break;
                    case '=':
                        t = new Token('=', Tag.SIMBOLO_SIMPLES, Linha);
                        Tokens.Enqueue(t);
                        break;
                    case '+':
                        t = new Token('+', Tag.OPERADOR, Linha);
                        Tokens.Enqueue(t);
                        break;
                    case '-':
                        t = new Token('-', Tag.OPERADOR, Linha);
                        Tokens.Enqueue(t);
                        break;
                    case '*':
                        t = new Token('*', Tag.OPERADOR, Linha);
                        Tokens.Enqueue(t);
                        break;
                    case '/':
                        t = new Token('/', Tag.OPERADOR, Linha);
                        Tokens.Enqueue(t);
                        break;
                    case ':':
                        if (NextCharIs('='))
                        {
                            t = new Token(":=", Tag.SIMBOLO_DUPLO, Linha);
                            NextChar();
                        }
                        else
                        {
                            t = new Token(':', Tag.SIMBOLO_SIMPLES, Linha);
                        }
                        Tokens.Enqueue(t);
                        break;
                    case '>':
                        if (NextCharIs('='))
                        {
                            t = new Token(">=", Tag.SIMBOLO_DUPLO, Linha);
                            NextChar();
                        }
                        else
                        {
                            t = new Token('>', Tag.SIMBOLO_SIMPLES, Linha);
                        }
                        Tokens.Enqueue(t);
                        break;
                    case '<':
                        if (NextCharIs('='))
                        {
                            t = new Token("<=", Tag.SIMBOLO_DUPLO, Linha);
                            NextChar();
                        }
                        else if (NextCharIs('>'))
                        {
                            t = new Token("<>", Tag.SIMBOLO_DUPLO, Linha);
                            NextChar();
                        }
                        else
                        {
                            t = new Token('<', Tag.SIMBOLO_SIMPLES, Linha);
                        }
                        Tokens.Enqueue(t);
                        break;
                }
                #endregion

            } while (Peek != null);

        }
        #endregion

        #region 3. Métodos Privados
        private static void NextChar()
        {
            if (!Texto.Any())
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

        private static Token ObterPalavraReservada(string lexema)
        {
            Token t = null;
            string[] lexemas = "program procedure if then while do write read else begin end integer real".Split(' ');
            foreach (var lex in lexemas)
            {
                if(lex == lexema)
                {
                    t = new Token(lex, Tag.PALAVRA_RESERVADA, Linha);
                    break;
                }
            }
            return t;
        }
        #endregion
    }
}
