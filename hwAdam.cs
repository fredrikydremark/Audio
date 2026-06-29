using System.IO.Ports;
using System.Text;
using static Scada.ScadaClasses;

namespace Scada
{
    public class Adam
    {  
        static SerialPort sp = new SerialPort();
        public Adam(string portName, string baud)
        {           
            int baudRate = 9600;
            if (int.TryParse(baud, out int iBaud))
            {
                baudRate = iBaud;
            }
 
            try
            {
               if (!sp.IsOpen)
               {      
                  sp.PortName = portName;
                  sp.BaudRate = baudRate; 
                  sp.DiscardNull = false;
                  sp.Encoding = Encoding.UTF8;
                  sp.NewLine = "\r";
                  sp.DtrEnable = true;
                  sp.Parity = Parity.None;
                  sp.DataBits = 8;
                  sp.StopBits = StopBits.One;
                  sp.Handshake = Handshake.None;
                  sp.ReadTimeout = 100;
                  sp.WriteTimeout = 50;
                  sp.Open();
               }
            }
            catch (UnauthorizedAccessException)
            {         
            }
            catch (IOException )
            {
            }
            catch (Exception)
            {
                return;
            }
        }


        ~Adam()
        {      
            try
            {
               if (sp != null && sp.IsOpen)
                  sp.Close();
            }
            catch (UnauthorizedAccessException )
            {
            }
            catch (IOException )
            {
            }
            catch (Exception)
            {
                return;
            }

        }


        public void ReadADAM()
        {
            Double Value = 99999;    


            string sBinary, s, DigitalOutput;
            short Channel=0;
            string message="";
            double k = 1;
            double m = 0;

            DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
            var ListOfDigitalOutputModules = MyDataAccessLayer.GetAdamModules(chDigitalOutput);

            foreach (var Module in ListOfDigitalOutputModules)
            {
                DigitalOutput = MyDataAccessLayer.GetDigitalOutputs(Module.Adress);
                for (Channel = 0; Channel < 7; Channel++)
                {
                    if (DigitalOutput[Channel] != '-')
                    {
                        s = String.Concat("#0", Module.Adress.ToString(), "1", Channel.ToString(), "0", DigitalOutput[Channel]);
                        try
                        {
                            sp.WriteLine(s);
                            message = sp.ReadLine();
                        }
                        catch (TimeoutException)
                        {
                            return;
                        }
                        catch (UnauthorizedAccessException )
                        {
                            return;
                        }
                        catch (IOException )
                        {
                            return;
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                }
            }

            /*
            try
            {   // Digital inputs
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
                    new Exception(ex.ToString(), ex);
                }

            }
            catch (TimeoutException)
            {
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                return;
            }
            catch (IOException ex)
            {
                return;
            }
            */



            //Adress = 1;
            var ListOfAnalogModules = MyDataAccessLayer.GetAdamModules(chAnalogInput);
            // Adress of the Analog 4017 
            foreach (var Module in ListOfAnalogModules)
            {
                for (Channel = 0; Channel < 3; Channel++)
                {
                    Value = 9999999;
                    s = String.Concat("#0", Module.Adress.ToString(), Channel.ToString());
                    message = "";
                    try
                    {
                        sp.WriteLine(s);
                        message = sp.ReadLine();
                        message = message.Replace("+", "");
                        message = message.Replace(".", ",");
                        message = message.Replace(">", "");

                        if (Double.TryParse(message, out Value))
                        {
                            k = MyDataAccessLayer.GetAnalogChannel_k( Module.Adress, Channel);
                            m = MyDataAccessLayer.GetAnalogChannel_m( Module.Adress, Channel);
                            Value = Value * k + m;

                            double MaxRange = 10000F;
                            double MinRange = -10000F;

                            if ((Value < MaxRange) && (Value > MinRange))
                            {
                                MyDataAccessLayer.StoreAnalog(Channel, Module.Adress, Value);
                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        message = "--.-";
                    }
                    catch (UnauthorizedAccessException )
                    {
                        return;
                    }
                    catch (IOException )
                    {
                        return;
                    }
                    catch (Exception)
                    {
                        return;
                    }

                }
            }
        }
    }
}
