using Microsoft.Data.SqlClient;
using System.IO.Ports;
using System.Text;
using static Scada.ScadaClasses;

namespace Scada
{
    public class Adam
    {   /*
        public static string myPortName = "COM4";
        public static int baudRate = 9600;
        static SerialPort sp = new SerialPort(myPortName, baudRate);
        */

        static SerialPort sp = new SerialPort();
        public Adam(string portName, string baud)
        {
            
            int baudRate = 9600;
            if (int.TryParse(baud, out int iBaud))
            {
                baudRate = iBaud;
            }

            /*
            try
            {
               if (!sp.IsOpen)
                  sp.Close();
            }
            catch (UnauthorizedAccessException ex)
            {
            }
            */
         
            
            try
            {
               if (!sp.IsOpen)
               {      
                  sp.PortName = portName;
                  sp.BaudRate = baudRate;
                  sp.Parity = Parity.None;
                  sp.DataBits = 8;
                  sp.StopBits = StopBits.One;
                  sp.Handshake = Handshake.None;
                  sp.ReadTimeout = 500;
                  sp.WriteTimeout = 500;
                  sp.Open();
               }
            }
            catch (UnauthorizedAccessException ex)
            {         
            }
            catch (IOException ex)
            {               
            }            
        }


        ~Adam()
        {
           if (sp != null && sp.IsOpen)
             sp.Close();
        }


        public void ReadADAM()
        {
            Double Value = 99999;    
            DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

            string sBinary, s, DigitalOutput;
            short Channel, Adress;
            string message;
            double k = 1;
            double m = 0;

            Adress = 1;
            if (sp.IsOpen == true)
            {
                var ListOfDigitalOutputModules = MyDataAccessLayer.GetAdamDigitalOutputs();

                //Write Digital outputs
                //Adress = 2;
                foreach (var Module in ListOfDigitalOutputModules)
                {
                    DigitalOutput = MyDataAccessLayer.GetDigitalOutputs(Module.Adress);
                    for (Channel = 0; Channel < 7; Channel++)
                    {
                        if (DigitalOutput[Channel] != '-')
                        {
                            s = String.Concat("#0", Adress.ToString(), "1", Channel.ToString(), "0", DigitalOutput[Channel]);
                            try
                            {
                                sp.WriteLine(s);
                                message = sp.ReadLine();
                            }
                            catch (TimeoutException)
                            {
                                break;
                            }
                        }
                    }
                }



                try
                {
                    Adress = 2;
                    //Synchronized sampling start
                    s = String.Concat("#**");
                    sp.WriteLine(s);
                    //Synchronized sampling read command
                    s = String.Concat("$0", Adress.ToString(), "4");
                    sp.WriteLine(s);

                    message = sp.ReadLine();
                    message = message.Substring(4, 2);
                    try
                    {
                        sBinary = Convert.ToString(Convert.ToInt32(message, 16), 2);
                        MyDataAccessLayer.StoreDigital(sBinary, Adress);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString(), ex);
                    }

                }
                catch (TimeoutException)
                {
                }

                Adress = 1;// Adress of the Analog 4017 
                for (Channel = 0; Channel < 3; Channel++)
                {
                    Value = 9999999;
                    s = String.Concat("#0", Adress.ToString(), Channel.ToString());
                    message = "";
                    try
                    {
                        sp.WriteLine(s);
                        message = sp.ReadLine();
                        message = message.Replace("+", "");
                        message = message.Replace(".", ",");
                        message = message.Replace(">", "");
                    }
                    catch (TimeoutException)
                    {
                        message = "--.-";
                    }

                    if (Double.TryParse(message, out Value))
                    {
                        k = MyDataAccessLayer.GetAnalogChannel_k(Adress, Channel);
                        m = MyDataAccessLayer.GetAnalogChannel_m(Adress, Channel);
                        Value = Value * k + m;

                        double MaxRange = 10000F;
                        double MinRange = -10000F;

                        if ((Value < MaxRange) && (Value > MinRange))
                        {
                            MyDataAccessLayer.StoreAnalog(Channel,Adress,Value);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    //string[] portNames = SerialPort.GetPortNames();
                    sp.DiscardNull = false;
                    sp.Encoding = Encoding.UTF8;
                    sp.ReadTimeout = 100;
                    sp.WriteTimeout = 50;
                    sp.NewLine = "\r";
                    sp.DtrEnable = true;
                    sp.Open();
                }
                catch
                {
                }
            }
        }
    }
}
