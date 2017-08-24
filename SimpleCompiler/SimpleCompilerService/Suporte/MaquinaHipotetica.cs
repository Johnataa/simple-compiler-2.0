using System.Collections.Generic;

namespace SimpleCompilerService.Suporte
{
    public class MaquinaHipotetica
    {
        public List<string> C { get; set; }
        public List<string> D { get; set; }

        public MaquinaHipotetica()
        {
            C = new List<string>();
            D = new List<string>();
        }
    }
}
