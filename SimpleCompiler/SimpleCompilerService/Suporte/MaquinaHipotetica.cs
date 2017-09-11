﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleCompilerPresentation
{
    public class MaquinaHipotetica
    {
        private static MaquinaHipotetica _maqHip;
        public List<string> C { get; set; }
        public List<object> D { get; set; }
        private int _i;

        private MaquinaHipotetica()
        {
            C = new List<string>();
        }

        public static MaquinaHipotetica GetInstance(bool novasInstrucoes = false)
        {
            if (_maqHip == null || novasInstrucoes)
            {
                _maqHip = new MaquinaHipotetica();
            }
            return _maqHip;
        }

        public void ExecutarPrograma()
        {
            _i = 0;
            while (_i < C.Count())
            {
                var aux = C[_i].Split(' ');
                var func = aux[0];
                var param = aux.Count() > 1 ? new object[] { aux[1] } : null;
                var function = GetType().GetMethod(func);
                function.Invoke(this, param);
                Debug.WriteLine(C[_i]);
                foreach (var item in D)
                {
                    Debug.WriteLine(item);
                }
                Debug.WriteLine("-----------------------");
                _i++;
            }
        }

        public void INPP()
        {
            D = new List<object>();
        }

        public void ARMZ(string param)
        {
            var n = int.Parse(param);
            D[n] = DPop();
        }

        public void CRCT(string param)
        {
            var k = double.Parse(param);
            D.Add(k);
        }

        public void CRVL(string param)
        {
            var n = int.Parse(param);
            D.Add(D[n]);
        }

        public void SOMA()
        {
            var a = double.Parse(DPop());
            var b = double.Parse(DPop());
            D.Add(a + b);
        }

        public void SUBT()
        {
            var a = double.Parse(DPop());
            var b = double.Parse(DPop());
            D.Add(b-a);
        }

        public void MULT()
        {
            var a = double.Parse(DPop());
            var b = double.Parse(DPop());
            D.Add(b * a);
        }
        public void DIVI()
        {
            var a = double.Parse(DPop());
            var b = double.Parse(DPop());
            D.Add(b * a);
        }

        public void INVI()
        {
            var a = double.Parse(DPop());
            D.Add(-a);
        }

        public void CPME()
        {
            var a = double.Parse(DPop());
            var b = double.Parse(DPop());
            D.Add(b < a);
        }

        public void CPMA()
        {
            var a = double.Parse(DPop());
            var b = double.Parse(DPop());
            D.Add(b > a);
        }

        public void CPIG()
        {
            var a = double.Parse(DPop());
            var b = double.Parse(DPop());
            D.Add(b == a);
        }

        public void CDES()
        {
            var a = double.Parse(DPop());
            var b = double.Parse(DPop());
            D.Add(b != a);
        }

        public void CPMI()
        {
            var a = double.Parse(DPop());
            var b = double.Parse(DPop());
            D.Add(b <= a);
        }

        public void CMAI()
        {
            var a = double.Parse(DPop());
            var b = double.Parse(DPop());
            D.Add(b > a);
        }
        public void DSVI(string param)
        {
            var i = int.Parse(param);
            _i = i-1;
        }

        public void DSVF(string param)
        {
            var i = int.Parse(param);
            var a = bool.Parse(DPop());
            if (!a)
            {
                _i = i-1;
            }
        }

        public void LEIT()
        {
            //TODO:  e agora josé?
            var inputDialog = new InputDialogSample("Informe a 1º entrada", "");
            if (inputDialog.ShowDialog() == true)
            {
                Debug.WriteLine(inputDialog.Answer);
            }
            else
            {
                Debug.WriteLine(inputDialog.Answer);
            }
            var input = 2;
            D.Add(input);
        }
        public void IMPR()
        {
            var output = DPop();
            Debug.WriteLine(output);
        }

        public void PARA()
        {
        
        }

        public void ALME(string param)
        {
            D.Add(0);
        }

        public void PARAM(string param)
        {
            var i = int.Parse(param);
            var value = D[i];
            D.Add(value);
        }
        public void PUSHER(string param)
        {
            var e = int.Parse(param);
            D.Add(e);
        }

        public void CHPR(string param)
        {
            DSVI(param);
        }

        public void DESM(string param)
        {
            var m = int.Parse(param);
            while (m > 0)
            {
                DPop();
                m--;
            }
        }

        public void RTPR()
        {
            _i = int.Parse(DPop()) - 1;
        }

        private string DPop()
        {
            var r = D.Last();
            D.RemoveAt(D.Count()-1);
            return r.ToString();
        }

    }
}
