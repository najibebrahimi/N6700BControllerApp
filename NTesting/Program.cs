using NEP.Instrument.N6700B;
using System.Runtime.CompilerServices;

namespace NTesting
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ERROR error;

            N6700B inst = new N6700B("TCPIP0::10.1.2.150::5025::SOCKET");

            inst.Initialize(out error);

            inst.SetVoltage(1, 2.86, out error);

            inst.SetOutputState(1, N6700B.State.ON, out error);

            double res = inst.MeasureVoltage(1, out error); 
            Console.WriteLine(res);

            Console.WriteLine(inst.NajibTest());

            double x = inst.MeasureCurrent(1, out error);
            Console.Write(x);

            inst.SetOutputState(1, N6700B.State.OFF, out error);

            inst.Close();
        }
    }
}
