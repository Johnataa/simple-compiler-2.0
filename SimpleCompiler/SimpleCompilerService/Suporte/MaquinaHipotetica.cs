using System.Collections.Generic;

namespace SimpleCompilerService.Suporte
{
    public class MaquinaHipotetica
    {
        private static MaquinaHipotetica _maqHip;
        public List<string> C { get; set; }
        public List<string> D { get; set; }

        private MaquinaHipotetica()
        {
            C = new List<string>();
            D = new List<string>();
        }

        public static MaquinaHipotetica GetInstance(bool novasInstrucoes = false)
        {
            if (_maqHip == null)
            {
                _maqHip = new MaquinaHipotetica();
            }
            if (novasInstrucoes)
            {
                _maqHip.C = new List<string>();
            }
            return _maqHip;
        }
    }
}
