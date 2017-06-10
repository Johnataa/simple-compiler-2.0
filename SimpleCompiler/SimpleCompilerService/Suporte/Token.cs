using System;
using System.ComponentModel;
using System.Reflection;

namespace SimpleCompilerService.Suporte
{
    public class Token
    {
        #region 1. Propriedades e Construtores
        public object Lexema { get; set; }
        public Tag Tag { get; set; }
        public int Linha { get; set; }

        public Token(object lexema, Tag tag, int linha)
        {
            Lexema = lexema;
            Tag = tag;
            Linha = linha;
        }

        public Token()
        {

        }
        #endregion

        #region 2. Métodos Privados
        private string GetTagDescription()
        {
            FieldInfo fi = Tag.GetType().GetField(Tag.ToString());
            DescriptionAttribute attr = Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) as DescriptionAttribute;

            if (attr != null)
            {
                return attr.Description;
            }
            return null;
        }
        #endregion

        #region 3. Sobrecarga de Métodos
        public override string ToString()
        {
            return Lexema.ToString() + " [" + GetTagDescription() + "] Linha " + Linha;
        }

        public override bool Equals(object obj)
        {
            return Lexema.GetType() == obj.GetType() && Lexema.ToString().Equals(obj.ToString());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
