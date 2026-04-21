using Microsoft.Data.SqlClient;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace Scada
{
    public class Adam
    {     
        static string myPortName = "COM4";
        static int baudRate = 9600;
        static SerialPort sp = new SerialPort(myPortName, baudRate);
    
        /*
        int StoreDigital(string sBinaryValue, int Adress)
        {
            var myConnection = new Microsoft.Data.SqlClient.SqlConnection
            {
                ConnectionString = "Data Source=PC-5CG5125C24; Initial Catalog=SCADA; Integrated Security=true; TrustServerCertificate=true"
            };
            for (int Channel = 0; Channel < 6; Channel++)
            {
                int Value = Convert.ToInt16(sBinaryValue.Substring((sBinaryValue.Length - 1) - Channel, 1));

                string sqlUpdate = "UPDATE Tags SET Value = @Value FROM Tags " +
                                   "JOIN AdamChannels on Tags.TagID = AdamChannels.TagID " +
                                   "WHERE(Tags.TagID = AdamChannels.TagID) AND ( Channel = @Channel AND Adress = @Adress )";

                try
                {
                    myConnection.Open();
                    SqlCommand cmdIns = new SqlCommand(sqlUpdate, myConnection);
                    cmdIns.Parameters.AddWithValue("@Channel", Channel);
                    cmdIns.Parameters.AddWithValue("@Adress", Adress);
                    cmdIns.Parameters.AddWithValue("@Value", Value);
                    cmdIns.ExecuteNonQuery();
                    cmdIns.Dispose();
                }
                catch (Exception ex)
                {
                    return (-1);
                    throw new Exception(ex.ToString(), ex);
                }
                finally
                {
                    myConnection.Close();
                }
            }
            return (0);
        }
        */

        /*
        private string GetDigitalOutputs(int Adress)
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
        */

        public void ReadADAM()
        {
            Double Value = 9999999;
    
            DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

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
                        switch (Channel)
                        {
                            case 0:
                                try
                                {
                                    k = 15;
                                    m = -50;
                                    Value = (Convert.ToDouble(message) * k) + m;

                                }
                                catch
                                {
                                }

                                break;

                            case 1:
                                try
                                {
                                    k = -1.8;
                                    m = 10;
                                    Value = (Convert.ToDouble(message) * k) + m;
                                }
                                catch
                                {
                                }

                                break;
                            case 2:
                                try
                                {
                                    k = 1;
                                    m = 0;
                                    Value = (Convert.ToDouble(message) * k) + m;
                                    Value = 0;
                                }
                                catch
                                {
                                }
                                break;

                            case 3:
                                try
                                {
                                    Value = 0;
                                }
                                catch
                                {
                                }
                                break;
                            case 4:
                                try
                                {

                                }
                                catch
                                {

                                }
                                break;
                            case 5:
                                try
                                {
                                    //Value = (Convert.ToDouble(message) * Convert.ToDouble(Value6_k)) + Convert.ToDouble(Value6_m);
                                }
                                catch
                                {

                                }

                                break;
                        }
                        message = "> " + message + "\r\n";

                        if ((Value < 99999) && (Value > -99999))
                        {
                            MyDataAccessLayer.StoreAnalog(Channel, Adress,Value);
                            /*
                            string sqlUpdate = "UPDATE Tags SET Value = @Value, ValueTime = Getdate(),StatusQuality=0 FROM Tags " +
                                               "JOIN AdamChannels on Tags.TagID = AdamChannels.TagID " +
                                               "WHERE(Tags.TagID = AdamChannels.TagID) AND ( Channel = @Channel AND Adress = @Adress)";

                            try
                            {
                                myConnection.Open();
                                SqlCommand cmdIns = new SqlCommand(sqlUpdate, myConnection);
                                cmdIns.Parameters.AddWithValue("@Channel", Channel);
                                cmdIns.Parameters.AddWithValue("@Adress", Adress);
                                cmdIns.Parameters.AddWithValue("@Value", Value);
                                cmdIns.ExecuteNonQuery();
                                cmdIns.Dispose();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.ToString(), ex);
                            }
                            finally
                            {
                                myConnection.Close();
                            }
                            */
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
                { }
            }
            //timer1.Enabled = true;
        }
    }
}
