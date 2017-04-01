using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRLib.Patterns.Observer;
namespace MyLibTests.Patterns.Observer
{
    class Sender
    {
        Messenger m;
        public Sender(Messenger m)
        {
            this.m = m;
        }
        public void Send<TMessage>(TMessage message)
        {
            m.Send(this, message);
        }
    }
}
