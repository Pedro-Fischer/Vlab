using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.ModelViews
{
    public class Home
    {
        public string Doc { get => "/swagger"; }
        public string Mensagem { get => "Bem-vindo à API"; }
    }
}