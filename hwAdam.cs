using Microsoft.Data.SqlClient;
using System.IO.Ports;
using System.Text;
using static Scada.ScadaClasses;

namespace Scada
{
    public class Adam
    {
        static string myPortName = "COM4";
        static int baudRate = 9600;
        static SerialPort sp = new SerialPort(myPortName, baudRate);
    
        public Adam(string portName, string baud)
        {
            myPortName = portName;
            if (int.TryParse(baud, out int iBaud))
            {
                ConnectionString = "Data Source=PC-5CG5125C24; Initial Catalog=SCADA; Integrated Security=true; TrustServerCertificate=true"
            };

            sp.Parity = Parity.None;
            sp.DataBits = 8;
            sp.StopBits = StopBits.One;
            sp.Handshake = Handshake.None;
            sp.ReadTimeout = 500;
            sp.WriteTimeout = 500;

            try
            {
              if (!sp.IsOpen)
                sp.Open();
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
            string DigOutput;
            int Channel;
            double SV;

            DigOutput = "-------";
            try
            {
                var myConnection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = "Data Source=PC-5CG5125C24; Initial Catalog=SCADA; Integrated Security=true; TrustServerCertificate=true"
                };

                //string sqlGetData = "SELECT Channel,SetValue FROM Adam where adress = @Adress ORDER BY Adress ASC,Channel ASC";
                string sqlGetData = "SELECT Channel, Tags.Value FROM AdamChannels " +
                                     "JOIN Tags on Tags.TagID = AdamChannels.TagID " +
                                     "Where ( Tags.TagID = AdamChannels.TagID ) AND ( adress = @Adress) " +
                                     "ORDER BY Adress ASC, Channel ASC";


                myConnection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, myConnection);
                cmdGetData.Parameters.AddWithValue("@Adress", Adress);
                SqlDataReader reader = cmdGetData.ExecuteReader();
                StringBuilder sDigital = new StringBuilder("-------");

                while (reader.Read())
                {
                    Channel = reader.GetInt32(0);
                    SV = reader.GetDouble(1);
                    if (SV > 0.2)
                        sDigital[Channel] = '1';
                    else
                        sDigital[Channel] = '0';

                }
                DigOutput = sDigital.ToString();
                reader.Close();
                cmdGetData.Dispose();
                myConnection.Close();
                myConnection.Dispose();
                myConnection = null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
            return (DigOutput);
        }


        public void ReadADAM()
        {
            Double Value = 9999999;
            var myConnection = new Microsoft.Data.SqlClient.SqlConnection
            {
                ConnectionString = "Data Source=PC-5CG5125C24; Initial Catalog=SCADA; Integrated Security=true; TrustServerCertificate=true"
            };

            string sBinary, s, DigitalOutput;
            short Channel, Adress;
            string message;
            double k = 1;
            double m = 0;

            Adress = 1;
            if (sp.IsOpen == true)
            {
                //Write Digital outputs
                Adress = 2;
                DigitalOutput = MyDataAccessLayer.GetDigitalOutputs(Adress);
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
                    string[] portNames = SerialPort.GetPortNames();
                    sp.DiscardNull = false;
                    sp.Encoding = Encoding.UTF8;
                    sp.ReadTimeout = 100;
                    sp.WriteTimeout = 50;
                    sp.NewLine = "\r";
                    sp.DtrEnable = true;
                    sp.Open();

                }
                catch
                { }
            }
        }
    }
}
