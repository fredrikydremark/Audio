
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text;
using static Scada.ScadaClasses;


namespace Scada
{
    public class DataAccessLayer(string s)
    {
        private string sConnection = s; 

        public string LoadLibItem(string Name, int TypeInLib)
        {
            try
            {
                string sBase64 = "";
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select base64 from Library where Name = @Name";
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@Name", Name);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    sBase64 = reader.GetString(0);
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (sBase64);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int SaveLibItem(string Name, int TypeInLib, string Base64)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlInsertData = "insert into Library(Type, Name, Base64 ) values (@TypeInLib, @Name, @Base64 )";
                SqlCommand cmdGetData = new SqlCommand(sqlInsertData, Connection);
                cmdGetData.Parameters.AddWithValue("@Name", Name);
                cmdGetData.Parameters.AddWithValue("@TypeInLib", TypeInLib);
                cmdGetData.Parameters.AddWithValue("@Base64", Base64);

                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int DeleteLibItem(string Name)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlDeleteData = "delete from Library where Name = @Name";
                SqlCommand cmdGetData = new SqlCommand(sqlDeleteData, Connection);
                cmdGetData.Parameters.AddWithValue("@Name", Name);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (-1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int StoreDigital(string sBinaryValue, int Adress)
        {
            var myConnection = new Microsoft.Data.SqlClient.SqlConnection
            {
                ConnectionString = sConnection
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

        public string GetDigitalOutputs(int Adress)
        {
            string DigOutput = "-------";
            try
            {
                var myConnection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "SELECT Channel, Tags.Value FROM AdamChannels " +
                                     "JOIN Tags on Tags.TagID = AdamChannels.TagID " +
                                     "Where ( Tags.TagID = AdamChannels.TagID ) AND ( adress = @Adress) " +
                                     "ORDER BY Adress ASC, Channel ASC";

                myConnection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, myConnection);
                cmdGetData.Parameters.AddWithValue("@Adress", Adress);
                SqlDataReader reader = cmdGetData.ExecuteReader();                  
                StringBuilder sDigital = new StringBuilder(DigOutput);

                while (reader.Read())
                {
                    int Channel = reader.GetInt32(0);
                    double SV = reader.GetDouble(1);
                    if (SV > 0.2)
                        sDigital[Channel] = '1';
                    else
                        sDigital[Channel] = '0';

                }
                DigOutput = sDigital.ToString();
                reader.Close();
                cmdGetData.Dispose();
                myConnection.Close();
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

        public double GetAnalogChannel_k(int Adress,int Channel)
        {
            double k = 1;
            try
            {
                var myConnection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "select k FROM AdamChannels " +
                                     "Where ( Channel= @Channel ) AND ( adress = @Adress )";
                                    
                myConnection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, myConnection);
                cmdGetData.Parameters.AddWithValue("@Adress", Adress);
                SqlDataReader reader = cmdGetData.ExecuteReader();
               

                while (reader.Read())
                {          
                   k = reader.GetDouble(0);
                }
               
                reader.Close();
                cmdGetData.Dispose();
                myConnection.Close();
 
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
            return (k);
        }

        public double GetAnalogChannel_m(int Adress, int Channel)
        {
            double m = 1;
            try
            {
                var myConnection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "select m FROM AdamChannels " +
                                     "Where ( Channel= @Channel ) AND ( adress = @Adress )";

                myConnection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, myConnection);
                cmdGetData.Parameters.AddWithValue("@Adress", Adress);
                SqlDataReader reader = cmdGetData.ExecuteReader();


                while (reader.Read())
                {
                    m = reader.GetDouble(0);
                }

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
            return (m);
        }

        public List<ScadaClasses.Adam> GetAdams()
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select TagID,Adress,Channel,ChType from AdamChannels";
                                  

                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var myAdams = new List<ScadaClasses.Adam>();

                while (reader.Read())
                {
                    myAdams.Add(new ScadaClasses.Adam
                    {
                        TagID = reader.GetInt32(0),
                        Adress = reader.GetInt32(1),
                        Channel = reader.GetInt32(2),
                        ChType = reader.GetInt32(3)

                    });
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (myAdams);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public List<ScadaClasses.Adam> GetAdamDigitalOutputs()
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select TagID,Adress,Channel,ChType from AdamChannels Where ChType=4";


                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var myAdams = new List<ScadaClasses.Adam>();

                while (reader.Read())
                {
                    myAdams.Add(new ScadaClasses.Adam
                    {
                        TagID = reader.GetInt32(0),
                        Adress = reader.GetInt32(1),
                        Channel = reader.GetInt32(2),
                        ChType = reader.GetInt32(3)

                    });
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (myAdams);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }





        public bool StoreAnalog(int Channel, int Adress, double Value)
        {
            var myConnection = new Microsoft.Data.SqlClient.SqlConnection
            {
                ConnectionString = sConnection
            };
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
            return (true);
        }


        public List<gridRow> ReadPages(string Filter, int StartCatRange, int EndCatRange, int StartRow, int EndRow)
        {
            try
            {
                string sqlGetData = "";
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);

                sqlGetData = "SELECT ID, Page, Name, Category FROM( SELECT *, ROW_NUMBER() OVER(ORDER BY Name) AS row " +
                             "FROM( SELECT * FROM[Pages] WHERE Category >= @StartCatRange AND Category <= @EndCatRange ) filtered ) numbered WHERE row >= @StartRow AND row <= @EndRow";

                cmdGetData.Parameters.AddWithValue("@StartCatRange", StartCatRange);
                cmdGetData.Parameters.AddWithValue("@EndCatRange", EndCatRange);
                cmdGetData.Parameters.AddWithValue("@StartRow", StartRow);
                cmdGetData.Parameters.AddWithValue("@EndRow", EndRow);

                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var gridRows = new List<gridRow>();
                int row = 0;
                while (reader.Read())
                {
                    gridRows.Add(new gridRow
                    {
                        Id = reader.GetInt32(0),
                        col1text = reader.GetString(2),
                        col1width = 100F,
                        col2text = "",
                        col2width = 10F,
                        col3text = "",
                        col3width = 10F,
                        col4text = "",
                        col4width = 10F,
                        col5text = "",
                        col5width = 10F,
                        col6text = "",
                        col6width = 10F,
                        Status = -1,
                        DataType = reader.GetInt32(3),
                        Row = row
                    });
                    row++;

                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (gridRows);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int UpdatePageName(int ID, string sName)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Pages SET Name = @Name WHERE ( ID = @ID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@Name", sName);
                cmdGetData.Parameters.AddWithValue("@ID", ID);

                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public List<gridRow> ReadParameters(string Filter, int StartCatRange, int EndCatRange, int StartRow, int EndRow )
        {
            try
            {
                string sqlGetData = "";
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);

                sqlGetData = "SELECT ID, Name, Value, Type, Category FROM( SELECT *, ROW_NUMBER() OVER(ORDER BY Name) AS row " +
                             "FROM( SELECT * FROM[Parameters] WHERE Category >= @StartCatRange AND Category <= @EndCatRange ) filtered ) numbered WHERE row >= @StartRow AND row <= @EndRow";

                cmdGetData.Parameters.AddWithValue("@StartCatRange", StartCatRange);
                cmdGetData.Parameters.AddWithValue("@EndCatRange", EndCatRange);
                cmdGetData.Parameters.AddWithValue("@StartRow", StartRow);
                cmdGetData.Parameters.AddWithValue("@EndRow", EndRow);

                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var gridRows = new List<gridRow>();
                int row = 0;
                while (reader.Read())
                {           
                        gridRows.Add(new gridRow
                        {
                            Id = reader.GetInt32(0),
                            col1text = reader.GetString(1),
                            col1width = 100F,
                            col2text = "",
                            col2width = 10F,
                            col3text = "",
                            col3width = 10F,
                            col4text = "",
                            col4width = 10F,
                            col5text = "",
                            col5width = 10F,
                            col6text = "",
                            col6width = 10F,
                            Status = -1,
                            DataType= reader.GetInt32(3),
                            Row = row
                        });
                        row++;
                   
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (gridRows);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public string ReadParameter(int ID)
        {
            try
            {
                int ParameterType = -1;
                string sParam = "";
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select Value,Type from Parameters where ID = @ID";
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ID", ID);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                while (reader.Read())
                {
                    sParam = reader.GetString(0);
                    ParameterType = reader.GetInt32(1);
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (sParam);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }
        public string ReadParameterByName(string Name)
        {
            try
            {
                int ParameterType = -1;
                string sParam = "";
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select Value,Type from Parameters where Name = @Name";
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@Name", Name);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                while (reader.Read())
                {
                    sParam = reader.GetString(0);
                    ParameterType = reader.GetInt32(1);
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (sParam);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int NewParameter(string Name, int Type, string Value, int Category)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlInsertData = "insert into Parameters(Name,Value, Type, Category ) values (@Name, @Value, @Type ,@Category )";
                SqlCommand cmdGetData = new SqlCommand(sqlInsertData, Connection);
                cmdGetData.Parameters.AddWithValue("@Name", Name);
                cmdGetData.Parameters.AddWithValue("@Type", Type);
                cmdGetData.Parameters.AddWithValue("@Value", Value);
                cmdGetData.Parameters.AddWithValue("@Category", Category);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int UpdateParameterValue(int ID, string sValue )
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Parameters SET Value = @Value WHERE ( ID = @ID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@Value", sValue);
                cmdGetData.Parameters.AddWithValue("@ID", ID);
                
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }



        public int DeleteParameter(string Name)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlDeleteData = "delete from Parameters where Name = @ParameterName";
                SqlCommand cmdGetData = new SqlCommand(sqlDeleteData, Connection);
                cmdGetData.Parameters.AddWithValue("@Name", Name);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (-1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }




        public int InitTags()
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "update tags set Value = -999999";

                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }



        public List<ScadaClasses.Tag> GetAnalogTagsToStore()
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select tags.tagID,tags.Value from tags " +
                                     "where ( Driver<>0 ) And " +
                                     "( TypeOfTag<>0 ) And " +
                                     "( tags.StatusQuality = 0 ) And " +
                                     "( Getdate() > DateADD(ss, tags.StoreIntervalSec,(select COALESCE(( select top 1 Time  from ChannelData where tagID = tags.TagID order by time desc ) ,'2020-01-01 00:00'))))";


                /*
                where(Driver<> 0) And(Getdate() > DateADD(ss, 60, (select COALESCE((select top 1 Time  from ChannelData where tagID = tags.TagID order by time desc), '2025-01-01 00:00'))))
                */
                /*
                select tags.tagID,tags.Value from tags
                where(Driver<> 0) And(Getdate() > DateADD(ss, 60, (select top 1 Time  from ChannelData where tagID = tags.TagID)))
                */

                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var myTags = new List<ScadaClasses.Tag>();

                while (reader.Read())
                {
                    myTags.Add(new ScadaClasses.Tag
                    {
                        TagID = reader.GetInt32(0),
                        Value = reader.GetDouble(1)
                    });
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (myTags);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public List<ScadaClasses.Tag> GetDigitalTagsToStore(int iFilter)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "";
                if (iFilter > 0)
                {
                    sqlGetData = "select tags.tagID,tags.Value from tags " +
                                 "where (Driver<> 0) And (TypeOfTag= 1) And (tags.Value <> (select COALESCE ((select top 1 Value from ChannelData where tagID = tags.TagID order by time desc) ,-1)))";
                }
                else
                {
                    sqlGetData = "select tags.tagID,tags.Value from tags " +
                                 "where (Driver<> 0) And (TypeOfTag=1)";
                }

                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var myTags = new List<ScadaClasses.Tag>();

                while (reader.Read())
                {
                    myTags.Add(new ScadaClasses.Tag
                    {
                        TagID = reader.GetInt32(0),
                        Value = reader.GetDouble(1)
                    });
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (myTags);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public List<gridRow> GetCommProtocols()
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select Value,Category from Parameters where Name ='Name' order by Name";

                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();
                var myDataSources = new List<gridRow>();
                int nRow = 0;
                while (reader.Read())
                {
                    myDataSources.Add(new gridRow
                    {
                        col1text = "Protocol",
                        col1width = 300,
                        col2text = reader.GetString(0),
                        col2width = 400,
                        col3text = "",
                        col4text = "",
                        col5text = "",
                        col6text = "",
                        DataType = 1,
                        Id = reader.GetInt32(1),
                        Row = nRow
                    });
                    nRow++;
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (myDataSources);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public List<gridRow> GetParams(int Category)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select Name,Value,ID from Parameters where Category =@Category";

                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                cmdGetData.Parameters.AddWithValue("@Category", Category);
                SqlDataReader reader = cmdGetData.ExecuteReader();
                var myTagParams = new List<gridRow>();
                int nRow = 0;
                while (reader.Read())
                {
                    myTagParams.Add(new gridRow
                    {
                        col1text = reader.GetString(0),
                        col1width = 300,
                        col2text = reader.GetString(1),
                        col2width = 400,
                        col3text = "",
                        col4text = "",
                        col5text = "",
                        col6text = "",
                        DataType = 1,
                        Id = reader.GetInt32(2),
                        Row = nRow
                    });
                    nRow++;
                }

                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (myTagParams);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }



        public List<gridRow> GetTagParams(int TagID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select tagname,description,HL,LL,Color,Unit,StoreIntervalSec,alarmenable,TypeOfTag,driver from tags where TagID =@TagID";

                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var myTagParams = new List<gridRow>();
                string tagname = "", description = "", HL = "", LL = "", Color = "", Unit = "", TypeOfTag = "";
                string AlarmEnable = "0", StoreIntervalSec = "0", Driver = "0"; 

                while (reader.Read())
                {
                    tagname = reader.GetString(0);
                    description = reader.GetString(1);
                    HL = reader.GetDouble(2).ToString();
                    LL = reader.GetDouble(3).ToString();
                    Color = reader.GetString(4);
                    Unit = reader.GetString(5);
                    StoreIntervalSec = reader.GetInt32(6).ToString();
                    AlarmEnable = reader.GetInt32(7).ToString();
                    TypeOfTag = reader.GetInt32(8).ToString();
                    Driver = reader.GetInt32(9).ToString();
                }

                myTagParams.Add(new gridRow
                {
                    col1text = "Tagname",
                    col1width = 300,
                    col2text = tagname,
                    col2width = 400,
                    col3text = "",
                    col4text = "",
                    col5text = "",
                    col6text = "",
                    DataType = 1,
                    Row = 0
                });

                myTagParams.Add(new gridRow
                {
                    col1text = "Description",
                    col1width = 300,
                    col2text = description,
                    col2width = 400,
                    col3text = "",
                    col4text = "",
                    col5text = "",
                    col6text = "",
                    DataType = 1,
                    Row = 1
                });

                myTagParams.Add(new gridRow
                {
                    col1text = "High limit",
                    col1width = 300,
                    col2text = HL,
                    col2width = 400,
                    col3text = "",
                    col4text = "",
                    col5text = "",
                    col6text = "",
                    DataType = 1,
                    Row = 2
                });

                myTagParams.Add(new gridRow
                {
                    col1text = "Low limit",
                    col1width = 300,
                    col2text = LL,
                    col2width = 400,
                    col3text = "",
                    col4text = "",
                    col5text = "",
                    col6text = "",
                    DataType = 1,
                    Row = 3
                });

                myTagParams.Add(new gridRow
                {
                    col1text = "Color",
                    col1width = 300,
                    col2text = Color,
                    col2width = 400,
                    col3text = "",
                    col4text = "",
                    col5text = "",
                    col6text = "",
                    DataType = 0,
                    Row = 4
                });

                myTagParams.Add(new gridRow
                {
                    col1text = "Unit",
                    col1width = 300,
                    col2text = Unit,
                    col2width = 400,
                    col3text = "",
                    col4text = "",
                    col5text = "",
                    col6text = "",
                    DataType = 0,
                    Row = 5
                });

                myTagParams.Add(new gridRow
                {
                    col1text = "StoreInterval Sec",
                    col1width = 300,
                    col2text = StoreIntervalSec.ToString(),
                    col2width = 400,
                    col3text = "",
                    col4text = "",
                    col5text = "",
                    col6text = "",
                    DataType = 0,
                    Row = 6
                });

                myTagParams.Add(new gridRow
                {
                    col1text = "AlarmEnable",
                    col1width = 300,
                    col2text = AlarmEnable.ToString(),
                    col2width = 400,
                    col3text = "",
                    col4text = "",
                    col5text = "",
                    col6text = "",
                    DataType = 0,
                    Row = 7
                });

                myTagParams.Add(new gridRow
                {
                    col1text = "Type of tag",
                    col1width = 300,
                    col2text = TypeOfTag.ToString(),
                    col2width = 400,
                    col3text = "",
                    col4text = "",
                    col5text = "",
                    col6text = "",
                    DataType = 0,
                    Row = 8
                });
                myTagParams.Add(new gridRow
                {
                    col1text = "Driver",
                    col1width = 300,
                    col2text = Driver.ToString(),
                    col2width = 400,
                    col3text = "",
                    col4text = "",
                    col5text = "",
                    col6text = "",
                    DataType = 0,
                    Row = 9
                });

                cmdGetData.Dispose();        
                Connection.Close();
                Connection = null;
                return (myTagParams);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public List<gridRow> LoadTags()
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select tagID,driver,tagname,description,value,HL,LL,Unit,Color from tags " +                                
                                    "WHERE Driver <> 0 ";

                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();
                var gridRows = new List<gridRow>();
                int r = 0;
                while (reader.Read())
                {
                    int TagID = reader.GetInt32(0);
                    int Driver = reader.GetInt32(1);
                    string TagName = reader.GetString(2);
                    string TagDesc = reader.GetString(3);

                    string sPV = "";
                    double dPV = reader.GetDouble(4);
                    if (dPV > -99999)
                    {
                        sPV = dPV.ToString("0.0");
                    }
                    else
                    {
                        sPV = "--.-";
                    }

                    string sLL = reader.GetDouble(5).ToString("0.0");
                    string sHL = reader.GetDouble(6).ToString("0.0");
                    string sUnit = reader.GetString(7);
                    string sColor = reader.GetString(8);

                    gridRows.Add(new gridRow
                    {
                        Status = 0,
                        col1text = TagDesc,
                        col1width = 340F,
                        col2text = sPV + sUnit,
                        col2width = 100F,
                        col3text = "",
                        col3width = 50F,
                        col4text = "",
                        col4width = 50F,
                        col5text = "",
                        col5width = 50F,
                        col6text = "",
                        col6width = 50F,
                        TagID = TagID,
                        Row = r,
                        Color = sColor,
                        Id = 0
                    });
                    r++;

                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (gridRows);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }
        /*
        public List<gridRow> LoadParameters()
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select Name,Value,Type,ID from Parameters WHERE Type = 0";                        
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();
                var gridRows = new List<gridRow>();
                int r = 0;
                while (reader.Read())
                {
                    string Name = reader.GetString(0);
                    string Value = reader.GetString(1);
                    int Type = reader.GetInt32(2);
                    int ID = reader.GetInt32(3);
                    gridRows.Add(new gridRow
                    {              
                        col1text = Name,
                        col1width = 250F,
                        col2text = Value,
                        col2width = 250F,
                        col3text = "",
                        col3width = 50F,
                        col4text = "",
                        col4width = 50F,
                        col5text = "",
                        col5width = 50F,
                        col6text = "",
                        col6width = 50F,
                        DataType = Type,
                        Row = r,
                        Status = 0,
                        Id = ID
                    });
                    r++;
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (gridRows);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }
        */
        public int EnableAlarmOnTag(int TagID, bool Enable)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlUpdate = "update tags set AlarmEnable = @Enable where TagID = @TagID";
                SqlCommand cmdGetData = new SqlCommand(sqlUpdate, Connection);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.Parameters.AddWithValue("@Enable", Enable);

                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (-1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int SetTagStatus(int Driver, int StatusQuality)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlUpdate = "update tags set StatusQuality = @Status where Driver = @Driver";
                SqlCommand cmdGetData = new SqlCommand(sqlUpdate, Connection);
                cmdGetData.Parameters.AddWithValue("@Driver", Driver);
                cmdGetData.Parameters.AddWithValue("@Status", StatusQuality);

                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (-1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }




        public List<ScadaClasses.LibItem> LoadLibItems()
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select name,base64 from Library";
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var Rows = new List<ScadaClasses.LibItem>();

                while (reader.Read())
                {
                    Rows.Add(new ScadaClasses.LibItem
                    {
                        sNameInLib = reader.GetString(0),
                        sBase64 = reader.GetString(1),
                    });
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (Rows);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }



        public int newitem(ScadaClasses.Telegram oTelegram)

        {
            if (oTelegram.Text == null)
            {
                oTelegram.Text = "";
            }
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlInsertData = "INSERT INTO items values( @page,@itemtype,@posLeft,@posTop,@posWidth,@posHeight,@Nextpage,@Action,@TagID,@Text,DEFAULT,@Radius )";
                SqlCommand cmdGetData = new SqlCommand(sqlInsertData, Connection);
                cmdGetData.Parameters.AddWithValue("@page", oTelegram.Page);
                cmdGetData.Parameters.AddWithValue("@itemtype", oTelegram.ItemType);
                cmdGetData.Parameters.AddWithValue("@posLeft", oTelegram.Left);
                cmdGetData.Parameters.AddWithValue("@posTop", oTelegram.Top);
                cmdGetData.Parameters.AddWithValue("@posWidth", oTelegram.Width);
                cmdGetData.Parameters.AddWithValue("@posHeight", oTelegram.Height);
                cmdGetData.Parameters.AddWithValue("@Nextpage", oTelegram.Nextpage);
                cmdGetData.Parameters.AddWithValue("@Action", oTelegram.Action);
                cmdGetData.Parameters.AddWithValue("@TagID", oTelegram.TagID);
                cmdGetData.Parameters.AddWithValue("@Text", oTelegram.Text);
                cmdGetData.Parameters.AddWithValue("@Radius", oTelegram.Radius);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }



        public double GetDefaultWidth(int ItemType)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                double ItemWidth = 10;
                string sqlGetData = "select defaultWidth from ItemTypes where ItemType = @ItemType";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemType", ItemType);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        ItemWidth = reader.GetDouble(0);
                    }
                    else
                    {
                        ItemWidth = 0;
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return (ItemWidth);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public double GetDefaultHeight(int ItemType)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                double ItemWidth = 10;
                string sqlGetData = "select defaultHeight from ItemTypes where ItemType =@ItemType";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemType", ItemType);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        ItemWidth = reader.GetDouble(0);
                    }
                    else
                    {
                        ItemWidth = 0;
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;

                return (ItemWidth);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public ScadaClasses.Telegram defaultItemSize(ScadaClasses.Telegram oTelegram)

        {
            oTelegram.Width = GetDefaultWidth(oTelegram.ItemType);
            oTelegram.Height = GetDefaultHeight(oTelegram.ItemType);
            return (oTelegram);
        }


        public ScadaClasses.Telegram GetItemSize(ScadaClasses.Telegram oTelegram)

        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "select Width,Height from ItemSizes where ItemType =@ItemType and nSize =@Size";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemType", oTelegram.ItemType);
                cmdGetData.Parameters.AddWithValue("@Size", oTelegram.size);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        oTelegram.Width = reader.GetDouble(0);
                        oTelegram.Height = reader.GetDouble(1);
                    }
                    else
                    {

                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;

                return (oTelegram);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }


        }


        /*Layouts

        delete from items where itemtype=0
        INSERT INTO Items(page, itemtype, posLeft, posTop, posWidth, posHeight, nextpage, action, tagid, text, radius )
        SELECT  1, Layouts.ItemType,Layouts.[Left], Layouts.[Top], Layouts.Width, Layouts.Height,1,'',0,' ',5
        FROM Layouts
        WHERE Layouts.nLayout =1

        */

        public int SetItemType(ScadaClasses.Telegram oTelegram)
        {
            oTelegram = defaultItemSize(oTelegram);
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Items SET itemtype = @Value,posWidth=@w,posHeight=@h WHERE ( ItemID = @ItemID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", oTelegram.ItemID);
                cmdGetData.Parameters.AddWithValue("@Value", oTelegram.ItemType);
                cmdGetData.Parameters.AddWithValue("@w", oTelegram.Width);
                cmdGetData.Parameters.AddWithValue("@h", oTelegram.Height);

                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int SetItemSize(ScadaClasses.Telegram oTelegram)
        {
            oTelegram = GetItemSize(oTelegram);
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Items SET posWidth=@w,posHeight=@h WHERE ( ItemID = @ItemID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", oTelegram.ItemID);
                cmdGetData.Parameters.AddWithValue("@w", Convert.ToDouble(oTelegram.Width));
                cmdGetData.Parameters.AddWithValue("@h", Convert.ToDouble(oTelegram.Height));

                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int SetItemTag(int ItemID, int TagID, int TagSequence)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE ItemTags SET TagID = @TagID  WHERE ( ItemID = @ItemID ) and ( TagSequence = @TagSequence )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.Parameters.AddWithValue("@TagSequence", TagSequence);

                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int UpdateTagName(int TagID, string TagName)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Tags SET TagName = @TagName  WHERE ( TagID = @TagID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagName", TagName);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int UpdateTagDescription(int TagID, string Description)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Tags SET Description = @Description  WHERE ( TagID = @TagID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@Description", Description);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }
        public int UpdateHL(int TagID, string HL)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Tags SET HL = @HL  WHERE ( TagID = @TagID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@HL", HL);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int UpdateLL(int TagID, string LL)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Tags SET LL = @LL  WHERE ( TagID = @TagID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@LL", LL);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int UpdateTagColor(int TagID, string TagColor)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Tags SET Color = @TagColor  WHERE ( TagID = @TagID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagColor", TagColor);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int UpdateTagUnit(int TagID, string TagUnit)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Tags SET Unit = @TagUnit  WHERE ( TagID = @TagID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagUnit", TagUnit);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int UpdateAlarmEnable(int TagID, string AlarmEnable)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Tags SET AlarmEnable = @AlarmEnable  WHERE ( TagID = @TagID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@AlarmEnable", AlarmEnable);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int UpdateStoreInterval(int TagID, string StoreIntervalSec)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Tags SET StoreIntervalSec = @StoreIntervalSec  WHERE ( TagID = @TagID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@StoreIntervalSec", StoreIntervalSec);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }
        public int UpdateTagType(int TagID, string TagType)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Tags SET TypeOfTag = @TagType  WHERE ( TagID = @TagID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagType", TagType);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int UpdateTagValue(int TagID, double dValue,int Status)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Tags SET Value = @Value, StatusQuality = @Status WHERE ( TagID = @TagID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@Value", dValue);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.Parameters.AddWithValue("@Status", Status);

                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }



        public int StoreTagValue(int TagID, double Value)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "INSERT INTO ChannelData( tagID, Time, Value ) VALUES( @TagID, GetDate(), @Value )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@Value", Value);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        /* Starting point
        SELECT TOP(1) [tagID]
            ,[Time]
            ,[Value]
        FROM[scada].[dbo].[ChannelData]
        WHERE(tagID = 3) AND(time<DateADD(mi, -10, Getdate()) )
        order by time desc
        */
        public dValue GetLatestValue(ScadaClasses.Tag MyTag)
        {
            dValue v = new dValue();
            v.t = DateTime.Now;
            v.v = 0;
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "SELECT TOP(1) tagID,Time,Value FROM ChannelData WHERE( tagID = @sTag ) order by time desc";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                cmdGetData.Parameters.AddWithValue("@sTag", MyTag.TagID.ToString());
                SqlDataReader reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    v.t = reader.GetDateTime(1);
                    v.v = reader.GetDouble(2);
                }
                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (v);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }
        public dValue GetBeyondValue(ScadaClasses.Tag MyTag)
        {
            dValue v = new dValue();
            v.t = DateTime.Now;
            v.v = 0;
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "SELECT TOP(1) tagID,Time,Value FROM ChannelData WHERE( tagID = @sTag ) AND (time<DateADD(second, -360, Getdate()) ) order by time desc";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                cmdGetData.Parameters.AddWithValue("@sTag", MyTag.TagID.ToString());
                SqlDataReader reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    v.t = reader.GetDateTime(1);
                    v.v = reader.GetDouble(2);
                }

                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (v);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public List<ScadaClasses.Value> GetCoupleOfMinutesPlotData(string sYear, string sMonth, string sDay, ScadaClasses.Tag MyTag)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "SELECT Hour,Minute,Second,Max(Totals) AS[Value] FROM( " +
                        " SELECT " +
                        "[Year]  = DATEPART(YEAR, [time]), " +
                        "[Month]  = DATEPART(MONTH, [time]), " +
                        "[Day]  = DATEPART(DAY,  [time]), " +
                        "[Hour] = DATEPART(HOUR, [time]), " +
                        "[Minute] = DATEPART(MINUTE,[time]), " +
                        "[Second] = DATEPART(SECOND,[time]), " +

                                "Totals = Max(Value) " +
                                "FROM ChannelData " +
                                "WHERE ( tagID = @sTag ) AND (time > DateADD(second, -360, Getdate()) AND (time < DateADD(mi, 0, Getdate() )))" +     // time < Getdate()
                                "GROUP BY " +
                                  "DATEPART(YEAR, [time]), " +
                                  "DATEPART(MONTH, [time]), " +
                                  "DATEPART(DAY, [time]), " +
                                  "DATEPART(HOUR, [time]), " +
                                  "DATEPART(MINUTE, [time]), " +
                                  "DATEPART(SECOND, [time]) " +
                                " ) AS q " +
                                " GROUP BY [Second],[Minute],[Hour],[Day],[Month],[Year] " +
                                "ORDER BY Second,Minute ,Hour, Day, Month, Year ";


                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);

                cmdGetData.CommandText = sqlGetData;
                cmdGetData.Parameters.AddWithValue("@sTag", MyTag.TagID.ToString());
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var src = DateTime.Now;
                var hm = new DateTime(src.Year, src.Month, src.Day, src.Hour, src.Minute, 0);

                var TempValues = new List<ScadaClasses.Value>();
                var DiagramScale = new List<ChartValue>();

                var Scale = new DateTime(src.Year, src.Month, src.Day, src.Hour, src.Minute, src.Second);
                var EndScale = Scale.AddSeconds(-360);

                while (Scale > EndScale)
                {
                    var v = new ChartValue();
                    v.Hour = Scale.Hour;
                    v.Minute = Scale.Minute;
                    v.Second = Scale.Second;
                    v.sValue = "";
                    DiagramScale.Add(v);
                    Scale = Scale.AddSeconds(-1);
                }
                int i = 0;
                int Minute;
                int Second;
                DiagramScale.Reverse();

                while (reader.Read())
                {
                    Minute = reader.GetInt32(1);
                    Second = reader.GetInt32(2);

                    foreach (ChartValue v in DiagramScale)
                    {
                        if (v.Minute == Minute)
                        {
                            if (v.Second == Second)
                            {
                                v.sValue = reader.GetDouble(3).ToString("000.00");
                            }
                            /*
                              if ((v.Second < (Second+5)) && (v.Second > (Second-5)))
                              {
                                v.sValue = reader.GetDouble(3).ToString("000.00");                        
                              }
                            */
                        }
                    }
                    i++;
                }
                string sTimeText;
                foreach (ChartValue v in DiagramScale)
                {
                    sTimeText = "";
                    if (v.Second == 0)
                    {
                        sTimeText = v.Hour.ToString("00") + ":" + v.Minute.ToString("00");
                    }
                    // No bar chart on this 10 minute scale
                    /*
                     if (MyTag.TypeOfTag > 1)
                     {
                         MyTag.TypeOfTag = 1;
                     }
                     */
                    TempValues.Add(new ScadaClasses.Value { X = sTimeText, Y = v.sValue, Color = MyTag.Color, TypeOfTag = MyTag.TypeOfTag });
                }
                TempValues[TempValues.Count - 1].Y = TempValues[TempValues.Count - 2].Y;
                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                dValue BeyondValue = GetBeyondValue(MyTag);
                dValue LatestValue = GetLatestValue(MyTag);

                double Diff = ((TimeSpan)(DateTime.Now - LatestValue.t)).TotalSeconds;

                if (Diff > 354)
                {
                    TempValues[0].Y = "0";
                    TempValues[TempValues.Count - 1].Y = "0";
                }
                else
                {
                    TempValues[0].Y = BeyondValue.v.ToString("0.0");
                    TempValues[TempValues.Count - 1].Y = LatestValue.v.ToString("0.0");
                }
                return (TempValues);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public List<ScadaClasses.Value> GetOneMinutePlotData(string sYear, string sMonth, string sDay, string sTag)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "SELECT [Hour],[Minute],Max(Totals) AS[Value] FROM( " +
                        " SELECT " +
                        "[Year]  = DATEPART(YEAR, [time]), " +
                        "[Month]  = DATEPART(MONTH, [time]), " +
                        "[Day]  = DATEPART(DAY,  [time]), " +
                        "[Hour] = DATEPART(HOUR, [time]), " +
                        "[Minute] = DATEPART(MINUTE,[time]), " +

                                "Totals = Max(Value) " +
                                "FROM ChannelData " +
                                "WHERE ( tagID = @sTag ) AND (time > DateADD(mi, -120, Getdate()) AND (time < DateADD(mi, 2, Getdate() )))" +                        // time < Getdate()
                                "GROUP BY " +
                                  "DATEPART(YEAR, [time]), " +
                                  "DATEPART(MONTH, [time]), " +
                                  "DATEPART(DAY, [time]), " +
                                  "DATEPART(HOUR, [time]), " +
                                  "DATEPART(MINUTE, [time]) " +
                                " ) AS q " +
                                " GROUP BY [Minute],[Hour],[Day],[Month],[Year] " +
                                "ORDER BY Minute ,Hour, Day, Month, Year ";


                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);

                cmdGetData.CommandText = sqlGetData;
                cmdGetData.Parameters.AddWithValue("@sTag", sTag);
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var src = DateTime.Now;
                var hm = new DateTime(src.Year, src.Month, src.Day, src.Hour, src.Minute, 0);

                var TempValues = new List<ScadaClasses.Value>();

                var DiagramScale = new List<ChartValue>();

                int Hour = src.Hour;
                int EndHour = Hour - 2;
                int Minute = src.Minute;


                while (EndHour <= Hour)
                {
                    if (Hour != EndHour)
                    {
                        while (Minute > 0)
                        {
                            var v = new ChartValue();
                            v.Minute = Minute;
                            v.Hour = Hour;
                            v.sValue = "0";
                            DiagramScale.Add(v);
                            Minute--;
                        }
                    }
                    else
                    {
                        while (Minute > src.Minute)
                        {
                            var v = new ChartValue();
                            v.Minute = Minute;
                            v.Hour = Hour;
                            v.sValue = "0";
                            DiagramScale.Add(v);
                            Minute--;
                        }
                    }
                    var v2 = new ChartValue();
                    v2.Minute = Minute;
                    v2.Hour = Hour;
                    v2.sValue = "0";

                    DiagramScale.Add(v2);
                    Minute = 59;
                    Hour--;
                }
                DiagramScale.Reverse();


                while (reader.Read())
                {
                    Hour = reader.GetInt32(0);
                    Minute = reader.GetInt32(1);

                    foreach (ChartValue v in DiagramScale)
                    {
                        if (v.Hour == Hour)
                        {
                            if (v.Minute == Minute)
                            {
                                v.sValue = reader.GetDouble(2).ToString("000.00");
                            }
                        }
                    }
                }

                string sTimeText;
                foreach (ChartValue v in DiagramScale)
                {
                    sTimeText = "";
                    if ((v.Minute % 30) == 0)
                    {
                        sTimeText = v.Hour.ToString("00") + ":" + v.Minute.ToString("00");
                    }
                    TempValues.Add(new ScadaClasses.Value { X = sTimeText, Y = v.sValue });
                }
                TempValues[TempValues.Count - 1].Y = TempValues[TempValues.Count - 2].Y;

                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;

                return (TempValues);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public List<ScadaClasses.Value> GetOneHourPlotData(ScadaClasses.ChartSetting myChartSetting, ScadaClasses.Tag MyTag)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "SELECT [Hour],[Minute],Max(Totals) AS[Value] FROM( " +
                        " SELECT " +
                        "[Year]  = DATEPART(YEAR, [time]), " +
                        "[Month]  = DATEPART(MONTH, [time]), " +
                        "[Day]  = DATEPART(DAY,  [time]), " +
                        "[Hour] = DATEPART(HOUR, [time]), " +
                        "[Minute] = DATEPART(MINUTE,[time]), " +
                                "Totals = Max(Value) " +
                                "FROM ChannelData " +
                                "WHERE ( tagID = @sTag ) " +
                                "GROUP BY " +
                                  "DATEPART(YEAR, [time])," +
                                  "DATEPART(MONTH, [time])," +
                                  "DATEPART(DAY, [time])," +
                                  "DATEPART(WEEKDAY, [time])," +
                                  "DATEPART(HOUR, [time])," +
                                  "DATEPART(MINUTE, [time]) " +
                                " ) AS q " +
                                "WHERE [Year] = @sYear and [Month] = @sMonth and [Day] = @sDay and  [Hour] = @sHour" +
                                " GROUP BY [Minute],  [Hour], [Day], [Year] " +
                                "ORDER BY Hour, Minute";

                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);

                cmdGetData.CommandText = sqlGetData;
                cmdGetData.Parameters.AddWithValue("@sTag", MyTag.TagID.ToString());
                cmdGetData.Parameters.AddWithValue("@sHour", myChartSetting.iHour.ToString());
                cmdGetData.Parameters.AddWithValue("@sDay", myChartSetting.iDay.ToString());
                cmdGetData.Parameters.AddWithValue("@sMonth", myChartSetting.iMonth.ToString());
                cmdGetData.Parameters.AddWithValue("@sYear", myChartSetting.iYear.ToString());
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var DiagramScale = new List<ChartValue>();
                var TempValues = new List<ScadaClasses.Value>();

                int iHour = 0, iMinute = 0;

                var sHourScale = new[] { "00", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "15", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "30", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "45", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };

                foreach (string s in sHourScale)
                {
                    var v = new ChartValue();
                    v.Minute = iMinute;
                    v.Hour = myChartSetting.iHour;
                    v.sTime = s;
                    v.sValue = "0";
                    DiagramScale.Add(v);
                    iMinute++;
                }

                while (reader.Read())
                {
                    iHour = reader.GetInt32(0);
                    iMinute = reader.GetInt32(1);

                    foreach (ChartValue v in DiagramScale)
                    {
                        if (v.Minute == iMinute)
                        {
                            v.Hour = iHour;
                            v.sValue = reader.GetDouble(2).ToString("000.00");

                        }
                    }
                }


                string sTimeText;
                foreach (ChartValue v in DiagramScale)
                {
                    sTimeText = "";
                    if ((v.Minute % 15) == 0)
                    {
                        sTimeText = v.Hour.ToString("00") + ":" + v.Minute.ToString("00");
                    }
                    TempValues.Add(new ScadaClasses.Value { X = sTimeText, Y = v.sValue, Color = MyTag.Color, TypeOfTag = MyTag.TypeOfTag,Max=100F,Min=0F,TimeSpan=tsHour });
                }

                /*
                while (reader.Read( ) )
                {
                    iHour = reader.GetInt32(0);
                    iMinute = reader.GetInt32(1);
                    while ((oldMinute < (iMinute - 1)) && (oldMinute < 59))
                    {
                        oldMinute++;
                        //sMinute = oldMinute.ToString("00"); 
                        sMinute = iHour.ToString("00") + ":" + oldMinute.ToString("00");
                        if (!sMinute.EndsWith("0"))
                        {
                            sMinute = "";
                        }

                        TempValues.Add(new Value { X = sMinute, Y = "0" });
                    }
                    sMinute = iHour.ToString("00")+":"+ iMinute.ToString("00");
                    if (!sMinute.EndsWith("0"))
                    {
                        sMinute = "";
                    }

                    sValue = reader.GetDouble(2).ToString("000.00");
                    TempValues.Add(new Value { X = sMinute, Y = sValue });
                    oldMinute = iMinute;
                }
                */

                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (TempValues);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public List<ScadaClasses.Value> GetOneDayPlotData(ScadaClasses.ChartSetting myChartSetting, ScadaClasses.Tag MyTag)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "SELECT [Hour], Max(Totals) AS[Value] FROM( " +
                        " SELECT " +
                        "[Year]  = DATEPART(YEAR, [time]), " +
                        "[Month]  = DATEPART(MONTH, [time]), " +
                        "[Day]  = DATEPART(DAY, [time]), " +
                        "[Hour] = DATEPART(HOUR, [time]), " +
                                "Totals = Max(Value) " +
                                "FROM ChannelData " +
                                "WHERE tagID = @sTag " +
                                "GROUP BY " +
                                  "DATEPART(YEAR, [time])," +
                                  "DATEPART(MONTH, [time])," +
                                  "DATEPART(DAY, [time])," +
                                  "DATEPART(WEEKDAY, [time])," +
                                  "DATEPART(HOUR, [time]) " +
                                " ) AS q " +
                                "WHERE [Year] = @sYear and [Month] = @sMonth and [Day] = @sDay " +
                                " GROUP BY [Hour], [Day], [Month], [Year] " +
                                "ORDER BY Day, Hour ";


                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);

                cmdGetData.CommandText = sqlGetData;
                cmdGetData.Parameters.AddWithValue("@sDay", myChartSetting.iDay);
                cmdGetData.Parameters.AddWithValue("@sMonth", myChartSetting.iMonth);
                cmdGetData.Parameters.AddWithValue("@sYear", myChartSetting.iYear);
                cmdGetData.Parameters.AddWithValue("@sTag", MyTag.TagID);

                SqlDataReader reader = cmdGetData.ExecuteReader();
                var TempValues = new List<ScadaClasses.Value>();

                /*
                var DiagramScale = new List<string>();
                int hour = 11;
                int minute = 0;
                while (hour > 0)
                {
                    while ( minute > 0 )
                    {
                        DiagramScale.Add("");
                        minute--;
                    }
                    DiagramScale.Add(hour.ToString());
                    hour--;
                }

                // DiagramScale.Add("01");
                foreach (string s in DiagramScale)
                {
                    TempValues.Add(new Value { X = s, Y = "" });
                }
                */
                var sDayScale = new[] { "00:00", "", "", "03:00", "", "", "06:00", "", "", "09:00", "", "", "12:00", "", "", "15:00", "", "", "18:00", "", "", "21:00", "", "" };
                foreach (string s in sDayScale)
                {
                    TempValues.Add(new ScadaClasses.Value { X = s, Y = "0", Color = MyTag.Color, TypeOfTag = MyTag.TypeOfTag, Max = 100F, Min = 0F, TimeSpan = tsDay });
                }

                int iHour;
                while (reader.Read())
                {
                    iHour = reader.GetInt32(0);
                    TempValues[iHour].Y = reader.GetDouble(1).ToString("000.00");
                }

                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (TempValues);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        /*
        private static int WeekOfYearISO8601(DateTime date)
        {
            var day = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(date);
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.AddDays(4 - (day == 0 ? 7 : day)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }


        public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }
        */


        public List<ScadaClasses.Value> GetOneWeekPlotData(ScadaClasses.ChartSetting myChartSetting, ScadaClasses.Tag MyTag)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "SET datefirst 1 " +
                        "SELECT [Year],[Month],[WEEKDAY],[Hour], Max(Totals) AS[Value] FROM( " +
                        " SELECT " +
                        "[Year]  = DATEPART(YEAR, [time]), " +
                        "[Month] = DATEPART(MONTH, [time]), " +
                        "[Week]  = DATEPART(WEEK, [time]), " +
                        "[WEEKDAY]  = DATEPART(WEEKDAY, [time]), " +
                        "[Day]  = DATEPART(DAY, [time]), " +
                        "[Hour] = DATEPART(HOUR, [time]), " +
                                "Totals = Max(Value) " +
                                "FROM ChannelData " +
                                "WHERE tagID = @sTag " +
                                "GROUP BY " +
                                  "DATEPART(YEAR, [time])," +
                                  "DATEPART(MONTH, [time])," +
                                  "DATEPART(WEEK, [time])," +
                                  "DATEPART(DAY, [time])," +
                                  "DATEPART(WEEKDAY, [time])," +
                                  "DATEPART(HOUR, [time])" +
                                " ) AS q " +
                                "WHERE @sWeek = [Week] AND @sYear = [Year] " +
                                "GROUP BY [Hour], [Day], [WEEKDAY], [Month], [Year] " +
                                "ORDER BY Year,Month,Day,Hour";

                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.CommandText = sqlGetData;
                cmdGetData.Parameters.AddWithValue("@sYear", myChartSetting.iYear);
                cmdGetData.Parameters.AddWithValue("@sWeek", myChartSetting.iWeek);
                cmdGetData.Parameters.AddWithValue("@sTag", MyTag.TagID);

                /*
                int iWeekDay;
                var TempValues = new List<ScadaClasses.Value>();
                var sWeekScale = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                var sMonday = new[] { "Mon", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };

                foreach (string s in sWeekScale)
                {
                    TempValues.Add(new ScadaClasses.Value { X = s, Y = "0", Color = MyTag.Color, TypeOfTag = MyTag.TypeOfTag, Max = 100F, Min = 0F, TimeSpan = tsWeek });
                }
                int i = 0;
                SqlDataReader reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    iWeekDay = reader.GetInt32(2);
                    TempValues[iWeekDay - 1].Y = reader.GetDouble(4).ToString("000.00");
                    i++;
                }
                */

                var TempValues = new List<ScadaClasses.Value>();
                var DiagramScale = new List<ChartValue>();

                var Scale = new DateTime(myChartSetting.iYear, myChartSetting.iMonth, myChartSetting.iDay, 0, 0, 0);
                var EndScale = Scale.AddDays(-7);
                int w = 8;
                while (Scale > EndScale)
                {
                    var v = new ChartValue();
                    v.DayOfWeek = w;
                    v.Hour = Scale.Hour;
                    v.Minute = Scale.Minute;
                    v.Second = Scale.Second;
                    v.sValue = "";
                    DiagramScale.Add(v);
                    Scale = Scale.AddHours(-1);
                    if (v.Hour == 0) 
                    {
                        w = w - 1;
                    }
                }
                int i = 0;
                DiagramScale.Reverse();

                int DayOfWeek;
                int Hour;
                SqlDataReader reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    DayOfWeek = reader.GetInt32(2);
                    Hour = reader.GetInt32(3);

                    foreach (ChartValue v in DiagramScale)
                    {
                        if (v.DayOfWeek == DayOfWeek)
                        {
                            if (v.Hour == Hour)
                            {
                                v.sValue = reader.GetDouble(4).ToString("000.00");
                            }
                       
                        }
                    }
                    i++;
                }

                string sTimeText;
                foreach (ChartValue v in DiagramScale)
                {
                    sTimeText = "";
                    if (v.Hour  == 0)
                    {
                        if (v.DayOfWeek == 1)
                        {
                            sTimeText = v.Hour.ToString("Mon");
                        }
                        if (v.DayOfWeek == 2)
                        {
                            sTimeText = v.Hour.ToString("Tis");
                        }
                        if (v.DayOfWeek == 3)
                        {
                            sTimeText = v.Hour.ToString("Ons");
                        }
                        if (v.DayOfWeek == 4)
                        {
                            sTimeText = v.Hour.ToString("Tor");
                        }
                        if (v.DayOfWeek == 5)
                        {
                            sTimeText = v.Hour.ToString("Fre");
                        }
                        if(v.DayOfWeek == 6)
                        {
                            sTimeText = v.Hour.ToString("Sat");
                        }
                        if(v.DayOfWeek == 7)
                        {
                            sTimeText = v.Hour.ToString("Sun");
                        }
                    }
                    TempValues.Add(new ScadaClasses.Value { X = sTimeText, Y = v.sValue, Color = MyTag.Color, TypeOfTag = MyTag.TypeOfTag, Max = 100F, Min = 0F, TimeSpan = tsWeek });
                }


                /*
                var DiagramScale = new List<ChartValue>();         
                int iHour = 0, iMinute = 0;
                var sHourScale = new[] { "00", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "15", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "30", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "45", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
                foreach (string s in sHourScale)
                {
                    var v = new ChartValue();
                    v.Minute = iMinute;
                    v.Hour = myChartSetting.iHour;
                    v.sTime = s;
                    v.sValue = "0";
                    DiagramScale.Add(v);
                    iMinute++;
                }

                while (reader.Read())
                {
                    iHour = reader.GetInt32(0);
                    iMinute = reader.GetInt32(1);

                    foreach (ChartValue v in DiagramScale)
                    {
                        if (v.Minute == iMinute)
                        {
                            v.Hour = iHour;
                            v.sValue = reader.GetDouble(2).ToString("000.00");

                        }
                    }
                }


                string sTimeText;
                foreach (ChartValue v in DiagramScale)
                {
                    sTimeText = "";
                    if ((v.Minute % 15) == 0)
                    {
                        sTimeText = v.Hour.ToString("00") + ":" + v.Minute.ToString("00");
                    }
                    TempValues.Add(new ScadaClasses.Value { X = sTimeText, Y = v.sValue, Color = MyTag.Color, TypeOfTag = MyTag.TypeOfTag, Max = 100F, Min = 0F, TimeSpan = tsHour });
                }

                */


                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (TempValues);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public List<ScadaClasses.Value> GetOneMonthPlotData(ScadaClasses.ChartSetting myChartSetting, ScadaClasses.Tag MyTag)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "SELECT [Day], Max(Totals) AS[Value] FROM( " +
                        " SELECT " +
                        "[Year]  = DATEPART(YEAR, [time]), " +
                        "[Month]  = DATEPART(MONTH, [time]), " +
                        "[Day]  = DATEPART(DAY, [time]), " +
                                "Totals = Max(Value) " +
                                "FROM ChannelData " +
                                "WHERE tagID = @sTag " +
                                "GROUP BY " +
                                  "DATEPART(YEAR, [time])," +
                                  "DATEPART(MONTH, [time])," +
                                  "DATEPART(DAY, [time]) " +
                                " ) AS q " +
                                "WHERE [Month]= @sMonth and [Year] = @sYear " +
                                "GROUP BY [Day], [Month], [Year] " +
                                "ORDER BY Day";


                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);

                cmdGetData.CommandText = sqlGetData;
                cmdGetData.Parameters.AddWithValue("@sMonth", myChartSetting.iMonth);
                cmdGetData.Parameters.AddWithValue("@sYear", myChartSetting.iYear);
                cmdGetData.Parameters.AddWithValue("@sTag", MyTag.TagID);
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var TempValues = new List<ScadaClasses.Value>();
                int daysInMonth = DateTime.DaysInMonth(myChartSetting.iYear, myChartSetting.iMonth);

                var sMonthScale = new[] { "01", "", "", "", "", "", "", "08", "", "", "", "", "", "", "15", "", "", "", "", "", "", "22", "", "", "", "", "", "", "29", "", "" };
                foreach (string s in sMonthScale)
                {
                    TempValues.Add(new ScadaClasses.Value { X = s, Y = "0", Color = MyTag.Color, TypeOfTag = MyTag.TypeOfTag, Max = 100F, Min = 0F, TimeSpan = tsMonth });
                }

                /*
                while (reader.Read())
                {
                    iDay= reader.GetInt32(0);
                    while ((oldDay < (iDay-1)) && (oldDay < daysInMonth))
                    {
                        oldDay++;
                        sDay = oldDay.ToString("00");
                        TempValues.Add(new Value { X = sDay, Y = "0" });               
                    }
                    sDay = iDay.ToString("00");
                    sValue = reader.GetDouble(1).ToString("000.00");
                    TempValues.Add(new Value { X = sDay, Y = sValue });          
                    oldDay = iDay;
                }
                */
                int iDay;
                while (reader.Read())
                {
                    iDay = reader.GetInt32(0);
                    TempValues[iDay - 1].Y = reader.GetDouble(1).ToString("000.00");
                }

                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (TempValues);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public List<ScadaClasses.Value> GetOneYearPlotData(ScadaClasses.ChartSetting myChartSetting, ScadaClasses.Tag MyTag)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "SELECT [Day],[Month],Max(Totals) AS[Value] FROM( " +
                       " SELECT " +
                       "[Year]  = DATEPART(YEAR, [time]), " +
                       "[Month]  = DATEPART(MONTH, [time]), " +
                       "[Day]  = DATEPART(DAY, [time]), " +
                               "Totals = Max(Value) " +
                               "FROM ChannelData " +
                               "WHERE tagID = @sTag " +
                               "GROUP BY " +
                                 "DATEPART(YEAR, [time])," +
                                 "DATEPART(MONTH, [time])," +
                                 "DATEPART(DAY, [time]) " +
                               " ) AS q " +
                               "WHERE [Year] = @sYear " +
                               "GROUP BY [Day],[Month], [Year] " +
                               "ORDER BY [Day],[Month]";

                /*
                string sqlGetData = "SELECT [Month], Max(Totals) AS[Value] FROM( " +
                        " SELECT " +
                        "[Year]  = DATEPART(YEAR, [time]), " +
                        "[Month] = DATEPART(MONTH, [time]), " +
                        "[Day]  = DATEPART(DAY, [time]), " +
                                "Totals = Max(Value) " +
                                "FROM ChannelData " +
                                "WHERE tagID = @sTag " +
                                "GROUP BY " +
                                  "DATEPART(YEAR, [time])," +
                                  "DATEPART(MONTH, [time])," +
                                  "DATEPART(DAY, [time]) " +
                                " ) AS q " +
                                "WHERE [Year] = @sYear " +
                                "GROUP BY [Month] " +
                                "ORDER BY Month";
                */

                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);

                cmdGetData.CommandText = sqlGetData;
                cmdGetData.Parameters.AddWithValue("@sYear", myChartSetting.iYear);
                cmdGetData.Parameters.AddWithValue("@sTag", MyTag.TagID);

                var TempValues = new List<ScadaClasses.Value>();
                var DiagramScale = new List<ChartValue>();

                var Scale = new DateTime(myChartSetting.iYear, 1, 1, 0, 0, 0);
                var EndScale = Scale.AddDays(365);
                int w = 8;
                while (Scale < EndScale)
                {
                    var v = new ChartValue();              
                    v.Day = Scale.Day;
                    v.Month = Scale.Month;
                    v.sValue = "";
                    DiagramScale.Add(v);
                    Scale = Scale.AddDays(1);             
                }

                int i = 0;
             

                int Day;
                int Month;
                SqlDataReader reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    Day = reader.GetInt32(0);
                    Month = reader.GetInt32(1);

                    foreach (ChartValue v in DiagramScale)
                    {
                        if (v.Month == Month)
                        {
                            if (v.Day == Day)
                            {
                                v.sValue = reader.GetDouble(2).ToString("000.00");
                            }

                        }
                    }
                    i++;
                }

                string sTimeText;
                foreach (ChartValue v in DiagramScale)
                {
                    sTimeText = "";
                    if (v.Day == 1)
                    {
                        sTimeText = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(v.Month);                 
                    }            
                    TempValues.Add(new ScadaClasses.Value { X = sTimeText, Y = v.sValue, Color = MyTag.Color, TypeOfTag = MyTag.TypeOfTag, Max = 100F, Min = 0F, TimeSpan = tsYear });
                }








                /*
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var TempValues = new List<ScadaClasses.Value>();
                string sMonth, sValue;
                int iMonth, oldMonth = 1;

                while (reader.Read())
                {
                    iMonth = reader.GetInt32(0);
                    while ((oldMonth < (iMonth - 1)) && (oldMonth < 12))
                    {
                        oldMonth++;
                        sMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(oldMonth);
                        TempValues.Add(new ScadaClasses.Value { X = sMonth, Y = "0", Color = MyTag.Color, TypeOfTag = MyTag.TypeOfTag, Max = 100F, Min = 0F, TimeSpan = tsYear });
                    }

                    sMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(iMonth); ;
                    sValue = reader.GetDouble(1).ToString("000.00");
                    TempValues.Add(new ScadaClasses.Value { X = sMonth, Y = sValue, Color = MyTag.Color, TypeOfTag = MyTag.TypeOfTag, Max = 100F, Min = 0F, TimeSpan = tsYear });
                    oldMonth = iMonth;
                }
                */
                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;

                return (TempValues);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public ScadaClasses.ChartSetting GetChartSettings(int ItemID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "select Span,Year,Month,Week,Day,Hour,Minute from chartSettings where ItemID = @ItemID";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                SqlDataReader reader = cmdGetData.ExecuteReader();
                ScadaClasses.ChartSetting ChartSet = new ScadaClasses.ChartSetting();

                while (reader.Read())
                {
                    ChartSet.iSpan = reader.GetInt32(0);
                    ChartSet.iYear = reader.GetInt32(1);
                    ChartSet.iMonth = reader.GetInt32(2);
                    ChartSet.iWeek = reader.GetInt32(3);
                    ChartSet.iDay = reader.GetInt32(4);
                    ChartSet.iHour = reader.GetInt32(5);
                    ChartSet.iMinute = reader.GetInt32(6);
                }

                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();

                Connection = null;

                if (ChartSet.iSpan < 1)
                {
                    ChartSet.iSpan = 1;
                }
                if (ChartSet.iYear < 2000)
                {
                    ChartSet.iYear = 2000;
                }
                if (ChartSet.iMonth < 1)
                {
                    ChartSet.iMonth = 1;
                }
                if (ChartSet.iDay < 1)
                {
                    ChartSet.iDay = 1;
                }
                return ChartSet;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public List<ScadaClasses.Tag> GetItemTags(int ItemID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "select ItemTags.tagID,tags.Color,tags.TypeOfTag from ItemTags left join tags on tags.tagID = ItemTags.tagID  where ItemID = @ItemID";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                SqlDataReader reader = cmdGetData.ExecuteReader();

                List<ScadaClasses.Tag> GraphItems = new List<ScadaClasses.Tag>();

                while (reader.Read())
                {
                    GraphItems.Add(new ScadaClasses.Tag
                    {
                        TagID = reader.GetInt32(0),
                        Color = reader.GetString(1),
                        TypeOfTag = reader.GetInt32(2)
                    });
                }
                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (GraphItems);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public List<gridRow> GetItemSizes(int ItemType)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "select nSize, Width, Height, sDescription from ItemSizes where ItemType = @ItemType order by nSize";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemType", ItemType);
                SqlDataReader reader = cmdGetData.ExecuteReader();

                List<gridRow> sItems = new List<gridRow>();

                while (reader.Read())
                {
                    sItems.Add(new gridRow
                    {
                        Row = reader.GetInt32(0),
                        col1text = reader.GetString(3),
                        col1width = 20.0F

                    });
                }
                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (sItems);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }



        public int CreateChartSettings(int ItemID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "insert into ChartSettings Values( 1,2026,1,1,1,0,0, @ItemID ) ";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                cmdGetData.ExecuteNonQuery();

                cmdGetData.Dispose();
                Connection.Close();

                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int SetChartSettings(ScadaClasses.ChartSetting myChartsettiing, int ItemID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE chartSettings SET " +
                                    "Span = @Span, " +
                                    "Year = @Year, " +
                                    "Month = @Month, " +
                                    "Week = @Week, " +
                                    "Day = @Day, " +
                                    "Hour = @Hour, " +
                                    "Minute = @Minute " +
                                    "WHERE ItemID = @ItemID";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                cmdGetData.Parameters.AddWithValue("@Span", myChartsettiing.iSpan);
                cmdGetData.Parameters.AddWithValue("@Year", myChartsettiing.iYear);
                cmdGetData.Parameters.AddWithValue("@Month", myChartsettiing.iMonth);
                cmdGetData.Parameters.AddWithValue("@Week", myChartsettiing.iWeek);
                cmdGetData.Parameters.AddWithValue("@Day", myChartsettiing.iDay);
                cmdGetData.Parameters.AddWithValue("@Hour", myChartsettiing.iHour);
                cmdGetData.Parameters.AddWithValue("@Minute", myChartsettiing.iMinute);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

    

        public List<gridRow> ReadTags(string Filter, int TypeOfTag, int StartRow, int EndRow)
        {
            try
            {
                string sqlGetData = "";
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                sqlGetData = "select TagID,Description FROM( SELECT *, ROW_NUMBER() OVER (ORDER BY Description) as row FROM tags WHERE ( Driver > 0 ) ) a WHERE ( @TypeOfTag = TypeOfTag ) OR ( @TypeOfTag = 0 ) AND ( row > @StartRow and row <= @EndRow )";
                cmdGetData.Parameters.AddWithValue("@StartRow", StartRow);
                cmdGetData.Parameters.AddWithValue("@EndRow", EndRow);
                cmdGetData.Parameters.AddWithValue("@TypeOfTag", TypeOfTag);

                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var gridRows = new List<gridRow>();
                int idx = 1;
                while (reader.Read())
                {
                    gridRows.Add(new gridRow
                    {
                        TagID = reader.GetInt32(0),
                        col1text = reader.GetString(1),
                        col1width = 100F,
                        col2text = "",
                        col2width = 10F,
                        col3text = "",
                        col3width = 10F,
                        col4text = "",
                        col4width = 10F,
                        col5text = "",
                        col5width = 10F,
                        col6text = "",
                        col6width = 10F,
                        Status = 5,
                        Id = idx,
                        Row = idx
                    });
                    idx++;
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (gridRows);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }



        public List<ScadaClasses.Telegram> getItems(int Page)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "SELECT itemID,itemtype,posLeft,posTop,posWidth,posHeight,Value,HL,LL,page," +
                                    "nextpage,unit,Action,text,items.tagID,tags.tagName,tags.Color,radius,tags.StatusQuality " +
                                    "FROM Items " +
                                    "LEFT JOIN Tags on tags.tagID = Items.tagID " +
                                    "WHERE ( Page = @Page ) " +
                                    "ORDER BY itemtype asc";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@Page", Page);
                SqlDataReader reader = cmdGetData.ExecuteReader();
                List<ScadaClasses.Telegram> Items = new List<ScadaClasses.Telegram>();
                Items.Clear();

                while (reader.Read())
                {
                    ScadaClasses.Telegram oTempTele = new ScadaClasses.Telegram();
                    oTempTele.MessageType = ScadaClasses.Cmdgetitems;
                    oTempTele.PacketSeq = 0;
                    oTempTele.ItemID = reader.GetInt32(0);
                    oTempTele.ItemType = reader.GetInt32(1);

                    oTempTele.Left = reader.GetDouble(2);
                    oTempTele.Top = reader.GetDouble(3);
                    oTempTele.Width = reader.GetDouble(4);
                    oTempTele.Height = reader.GetDouble(5);
                    oTempTele.ItemValues = ReadItemValues(oTempTele.ItemID);

                    if (reader.IsDBNull(7) == false)
                    {
                        oTempTele.HL = reader.GetDouble(7).ToString("0.0").Replace(",", ".");
                    }
                    else
                    {
                        oTempTele.HL = "--.-";
                    }

                    if (reader.IsDBNull(8) == false)
                    {
                        oTempTele.LL = reader.GetDouble(8).ToString("0.0").Replace(",", ".");
                    }
                    else
                    {
                        oTempTele.LL = "--.-";
                    }

                    oTempTele.SV = "0.0";
                    oTempTele.Page = reader.GetInt32(9);
                    oTempTele.Nextpage = reader.GetInt32(10);

                    if (reader.IsDBNull(11) == false)
                    {
                        oTempTele.Unit = reader.GetString(11);
                    }
                    else
                    {
                        oTempTele.Unit = "";
                    }

                    oTempTele.Action = reader.GetString(12);
                    oTempTele.Text = reader.GetString(13);
                    oTempTele.TagID = reader.GetInt32(14);

                    if (reader.IsDBNull(16) == false)
                    {
                        oTempTele.TagName = reader.GetString(15);
                    }
                    else
                    {
                        oTempTele.TagName = "Not defined";
                    }

                    if (reader.IsDBNull(16) == false)
                    {
                        oTempTele.Color = reader.GetString(16);
                    }
                    else
                    {
                        oTempTele.Color = "rgba(255,255,255,253)";
                    }

                    oTempTele.Hoover = 0;
                    oTempTele.Fade = 0;
                    oTempTele.Radius = 1;

                    if (reader.IsDBNull(17) == false)
                    {
                        oTempTele.Radius = reader.GetDouble(17);
                    }
                    if (oTempTele.ItemType == ScadaClasses.uxAlarmGrid)
                    {
                        oTempTele.gridRows = ReadAlarmMessages("All");
                    }
                    if (oTempTele.ItemType == ScadaClasses.uxTagsGrid)
                    {
                        //oTempTele.gridRows = LoadTags();
                    }
                    if (oTempTele.ItemType == ScadaClasses.uxParameters)
                    {
                       // oTempTele.gridRows = ReadParameters("",1,5);
                    }

                    if (oTempTele.ItemType == ScadaClasses.uxHistoryChart)
                    {
                        oTempTele.Min = "0";
                        oTempTele.Max = "30";
                        var ChartSetting = GetChartSettings(oTempTele.ItemID);
                        var GraphItems = GetItemTags(oTempTele.ItemID);
                        foreach (var myGraph in GraphItems)
                        {
                            if (ChartSetting.iSpan == 1)
                            {
                                var src = DateTime.Now;
                                //var hm = new DateTime(src.Year, src.Month, src.Day, src.Hour, src.Minute, 0);
                                oTempTele.ListOfValues.Add(GetCoupleOfMinutesPlotData(src.Year.ToString(), src.Month.ToString(), src.Day.ToString(), myGraph));
                            }

                            if (ChartSetting.iSpan == 2)
                            {
                                oTempTele.ListOfValues.Add(GetOneHourPlotData(ChartSetting, myGraph));
                            }

                            if (ChartSetting.iSpan == 3)
                            {
                                oTempTele.ListOfValues.Add(GetOneDayPlotData(ChartSetting, myGraph));
                            }

                            if (ChartSetting.iSpan == 4)
                            {
                                oTempTele.ListOfValues.Add(GetOneWeekPlotData(ChartSetting, myGraph));
                            }
                            if (ChartSetting.iSpan == 5)
                            {
                                oTempTele.ListOfValues.Add(GetOneMonthPlotData(ChartSetting, myGraph));
                            }
                            if (ChartSetting.iSpan == 6)
                            {
                                oTempTele.ListOfValues.Add(GetOneYearPlotData(ChartSetting, myGraph));
                            }
                        }
                    }
                    Items.Add(oTempTele);
                }
                reader.Close();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (Items);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int DeleteAllTagsFromUxItem(int ItemID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlDeleteData = "delete from ItemTags where ItemID = @ItemID";
                SqlCommand cmdGetData = new SqlCommand(sqlDeleteData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int DeleteTagFromUxItem(int ItemID, int TagID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlDeleteData = "delete from ItemTags where ItemID = @ItemID and TagID = @TagID";
                SqlCommand cmdGetData = new SqlCommand(sqlDeleteData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }




        public List<gridRow> ReadAlarmMessages(string Filter)
        {
            const int cOngoing = 1, cConfirmed = 2, cDeleted = 3, cInactive = 4;

            try
            {
                string sqlGetData = "";
                var Connection = new SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);

                //Auto delete when PV returns within limits
                sqlGetData = "update alarms set deleted = GETDATE() where (PV < HL) and (PV > LL) and ( alarms.deleted is null ) and ( alarms.confirmed is not null )";
                cmdGetData.CommandText = sqlGetData;
                cmdGetData.ExecuteNonQuery();

                //check PV within limits               
                sqlGetData = "INSERT INTO alarms SELECT tags.tagID,tags.Value,tags.HL,tags.LL,null,null,GETDATE(),tags.description FROM tags where (tags.AlarmEnable > 0) and ((tags.Value > tags.HL) or(tags.Value < tags.LL))and(not exists(select top 1 tagID from alarms ))";
                cmdGetData.CommandText = sqlGetData;
                cmdGetData.ExecuteNonQuery();

                //check PV within limits               
                sqlGetData = "INSERT INTO alarms SELECT tags.tagID,tags.Value,tags.HL,tags.LL,null,null,GETDATE(),tags.description FROM tags where (tags.AlarmEnable > 0) and ((tags.Value > tags.HL) or(tags.Value < tags.LL))and(not exists(select top 1 tagID from alarms where ( deleted is null ) and (tags.tagID = alarms.tagID)))";
                cmdGetData.CommandText = sqlGetData;
                cmdGetData.ExecuteNonQuery();

                //check PV within limits when deleted                
                /*
                 sqlGetData = "INSERT INTO alarms SELECT tags.tagID,tags.Value,tags.HL,tags.LL,null,null,GETDATE(),tags.description FROM tags where (tags.AlarmEnable > 0) and ((tags.Value > tags.HL) or(tags.Value < tags.LL)) and ((select top 1 deleted from alarms where (tags.tagID = alarms.tagID) order by alarmtime desc ) is not null )";
                 cmdGetData.CommandText = sqlGetData;
                 cmdGetData.ExecuteNonQuery();
                 */
                //update PV in alarms on all non deleted alarms //,alarmtime = GetDate()
                sqlGetData = "update alarms set PV = tags.Value,HL = tags.HL, LL = tags.LL  from alarms JOIN tags on tags.tagID = alarms.tagID where tags.tagID = alarms.tagID and ((tags.Value < tags.HL) or (tags.Value > tags.LL)) and alarms.deleted is null";
                cmdGetData.CommandText = sqlGetData;
                cmdGetData.ExecuteNonQuery();

                //Get alarm list
                sqlGetData = "select top 10 Description,PV,HL,LL,Confirmed,Alarmtime,tagID,ID from alarms where deleted is null order by Alarmtime desc";
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var gridRows = new List<gridRow>();
                int r = 0;
                while (reader.Read())
                {
                    int iStatus = 0;
                    string StatusText = "";
                    string AlarmText = reader.GetString(0);
                    string sPV = reader.GetDouble(1).ToString("0.0");
                    string sLL = reader.GetDouble(2).ToString("0.0");
                    string sHL = reader.GetDouble(3).ToString("0.0");

                    if (reader.IsDBNull(4) == true)
                    {
                        if ((reader.GetDouble(1) > reader.GetDouble(2)) || (reader.GetDouble(1) < reader.GetDouble(3)))
                        {
                            iStatus = cOngoing;
                            StatusText = "Ongoing ";
                        }
                        else
                        {
                            iStatus = cInactive;
                            StatusText = "Inactive ";
                        }
                    }
                    else
                    {
                        iStatus = cConfirmed;
                        StatusText = "Confirmed ";
                    }


                    string AlarmTime = "No timestamp";
                    if (reader.IsDBNull(5) == false)
                    {
                        AlarmTime = reader.GetDateTime(5).ToString("yyyyMMdd HH:mm");
                    }

                    int iTagID = reader.GetInt32(6);
                    int ID = reader.GetInt32(7);
                    gridRows.Add(new gridRow
                    {
                        Status = iStatus,
                        col1text = StatusText,
                        col1width = 100F,
                        col2text = AlarmText,
                        col2width = 300F,
                        col3text = sPV,
                        col3width = 50F,
                        col4text = sHL,
                        col4width = 50F,
                        col5text = sLL,
                        col5width = 50F,
                        col6text = AlarmTime,
                        col6width = 100F,
                        TagID = iTagID,
                        Row = r,
                        Id = ID
                    });
                    r++;
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (gridRows);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }



        public int deleteAlarm(ScadaClasses.Telegram oTelegram)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "UPDATE Alarms SET Deleted = GetDate()  WHERE ( ID = @ID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ID", oTelegram.TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (1);
            }
            catch (Exception ex)
            {
                return (0);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int disableAlarm(int TagID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "UPDATE Tags SET AlarmEnable = 0  WHERE ( TagID = @TagID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (1);
            }
            catch (Exception ex)
            {
                return (0);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }




        public int confirmAlarm(ScadaClasses.Telegram oTelegram)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "UPDATE Alarms SET Confirmed = GetDate()  WHERE ( ID = @ID )";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ID", oTelegram.TagID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();

                Connection = null;
                return (1);
            }
            catch (Exception ex)
            {
                return (0);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }



        public List<gridRow> ReadPages(string Filter, int StartRow, int EndRow)
        {
            try
            {
                string sqlGetData = "";
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                sqlGetData = "select page,name FROM(SELECT *, ROW_NUMBER() OVER (ORDER BY page) as row FROM pages) a WHERE row > @StartRow and row <= @EndRow";

                cmdGetData.Parameters.AddWithValue("@StartRow", StartRow);
                cmdGetData.Parameters.AddWithValue("@EndRow", EndRow);

                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();
                var gridRows = new List<gridRow>();

                while (reader.Read())
                {
                    gridRows.Add(new gridRow
                    {
                        TagID = reader.GetInt32(0),
                        col1text = reader.GetString(1),
                        col1width = 100F,
                        col2text = "",
                        col2width = 10F,
                        col3text = "",
                        col3width = 10F,
                        col4text = "",
                        col4width = 10F,
                        col5text = "",
                        col5width = 10F,
                        col6text = "",
                        col6width = 10F,
                        Status = 5,
                        Id = 0
                    });
                }

                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (gridRows);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public List<gridRow> ReadItemTypes(string Filter, int StartRow, int EndRow)
        {
            try
            {
                string sqlGetData = "";
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                sqlGetData = "select * FROM( SELECT *, ROW_NUMBER() OVER (ORDER BY ItemType) as row FROM ItemTypes) a WHERE row > @StartRow and row <= @EndRow";
                cmdGetData.Parameters.AddWithValue("@StartRow", StartRow);
                cmdGetData.Parameters.AddWithValue("@EndRow", EndRow);

                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();
                var gridRows = new List<gridRow>();

                while (reader.Read())
                {
                    gridRows.Add(new gridRow
                    {
                        TagID = reader.GetInt32(0),
                        col1text = reader.GetString(1),
                        col1width = 100F,
                        col2text = "",
                        col2width = 10F,
                        col3text = "",
                        col3width = 10F,
                        col4text = "",
                        col4width = 10F,
                        col5text = "",
                        col5width = 10F,
                        col6text = "",
                        col6width = 10F,
                        Status = 5,
                        Id = 0
                    });
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (gridRows);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public List<ItemValue> ReadItemValues(int ItemID)
        {
            try
            {
                string sqlGetData = "";
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                sqlGetData = "select itemtags.TagID, tags.value,tags.unit,tags.StatusQuality from itemtags left join tags on itemtags.tagid = tags.tagid where ItemID = @ItemID order by TagSequence";
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();
                int theID = -1;
                float theValue = 0F;
                string sUnit = "";
                var ItemValues = new List<ItemValue>();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        theID = reader.GetInt32(0);
                    }
                    else
                    {
                        theID = -1;
                    }
                    if (reader.IsDBNull(1) == false)
                    {
                        theValue = (float)reader.GetDouble(1);
                    }
                    else
                    {
                        theValue = 0F;
                    }
                    if (reader.IsDBNull(2) == false)
                    {
                        sUnit = reader.GetString(2);
                    }
                    else
                    {
                        sUnit = "";
                    }

                    int StatusQuality = 1;
                    if (reader.IsDBNull(3) == false)
                    {
                        StatusQuality = reader.GetInt32(3);
                    }

                    ItemValues.Add(new ItemValue
                    {
                        TagID = theID,
                        Value = theValue,
                        Unit = sUnit,
                        StatusQuality = StatusQuality
                    });

                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (ItemValues);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public List<gridRow> ReadItemTags(int ItemID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlGetData = "select TagID,UxDescription,TagSequence from ItemTags where ItemID = @ItemID";
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);

                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var gridRows = new List<gridRow>();

                while (reader.Read())
                {
                    gridRows.Add(new gridRow
                    {
                        TagID = reader.GetInt32(0),
                        col1text = reader.GetString(1),
                        col1width = 100F,
                        col2text = "",
                        col2width = 10F,
                        col3text = "",
                        col3width = 10F,
                        col4text = "",
                        col4width = 10F,
                        col5text = "",
                        col5width = 10F,
                        col6text = "",
                        col6width = 10F,
                        Status = 5,
                        Id = reader.GetInt32(2)
                    });
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (gridRows);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public List<gridRow> ReadTimeSpans()
        {
            try
            {
                string sqlGetData = "";
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                sqlGetData = "select Description from ChartTimeSpans order by TagSequence";
                cmdGetData.CommandText = sqlGetData;
                SqlDataReader reader = cmdGetData.ExecuteReader();

                var gridRows = new List<gridRow>();
                int idx = 1;
                while (reader.Read())
                {
                    gridRows.Add(new gridRow
                    {
                        col1text = reader.GetString(0),
                        col1width = 100F,
                        col2text = "",
                        col2width = 10F,
                        col3text = "",
                        col3width = 10F,
                        col4text = "",
                        col4width = 10F,
                        col5text = "",
                        col5width = 10F,
                        col6text = "",
                        col6width = 10F,
                        Status = 5,
                        Id = idx,
                        Row = idx
                    });
                    idx++;
                }
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (gridRows);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int AddTag(Tag myTag)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlInsertData = "insert into Tags(Driver,TagName,Value,HL,LL,Unit,Description,Color,TypeOfTag ,AlarmEnable,ValueTime,StoreIntervalSec,StatusQuality) " +
                                       "values (1,@TagName,0,@HL,@LL,@Unit,@Description,@Color,@TypeOfTag ,@AlarmEnable,null,@StoreIntervalSec,-1)";
                SqlCommand cmdGetData = new SqlCommand(sqlInsertData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagName", myTag.Name);
                cmdGetData.Parameters.AddWithValue("@Description", myTag.Description);
                cmdGetData.Parameters.AddWithValue("@Value", myTag.Value);
                cmdGetData.Parameters.AddWithValue("@HL", myTag.HL);
                cmdGetData.Parameters.AddWithValue("@LL", myTag.LL);
                cmdGetData.Parameters.AddWithValue("@Unit", myTag.Unit);
                cmdGetData.Parameters.AddWithValue("@Color", myTag.Color);
                cmdGetData.Parameters.AddWithValue("@TypeOfTag", myTag.TypeOfTag);
                cmdGetData.Parameters.AddWithValue("@AlarmEnable", myTag.AlarmEnable);
                cmdGetData.Parameters.AddWithValue("@StoreIntervalSec", myTag.StoreIntervalSec);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (GetLatestTagID());
            }
            catch (Exception ex)
            {
                return (-1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int DeleteTagByName(string TagName)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlDeleteData = "delete from tags where tagName LIKE( @TagName )";

                SqlCommand cmdGetData = new SqlCommand(sqlDeleteData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagName", TagName);

                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (-1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }




        public int AddItemTag(int ItemID, int ItemType, int TagID, int TagSequence, string UxDescription)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlInsertData = "insert into ItemTags(ItemID, ItemType, TagID, TagSequence, UxDescription) values (@ItemID , @ItemType, @TagID, @TagSequence, @UxDescription )";
                SqlCommand cmdGetData = new SqlCommand(sqlInsertData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                cmdGetData.Parameters.AddWithValue("@ItemType", ItemType);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.Parameters.AddWithValue("@TagSequence", TagSequence);
                cmdGetData.Parameters.AddWithValue("@UxDescription", UxDescription);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int GetItemTag(int ItemID, int TagID)
        {
            int ResultTagID = -1;
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "SELECT TOP 1 TagID FROM ItemTags where ItemID=@ItemID and TagID=@TagID";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);

                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        ResultTagID = reader.GetInt32(0);
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
            return ResultTagID;
        }


        public int GetLatestItemID()
        {
            int ItemID = -1;
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "SELECT TOP 1 ItemID FROM Items Order By Created desc";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                //cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        ItemID = reader.GetInt32(0);
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
            return ItemID;
        }


        public int GetLatestTagID()
        {
            int TagID = -1;
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "SELECT TOP 1 TagID FROM Tags Order By Created desc";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);

                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        TagID = reader.GetInt32(0);
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
            return TagID;
        }



        public int copyItem(ScadaClasses.Telegram oTelegram)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlInsertData = "insert into items( page, itemtype, posLeft, posTop, posWidth, posHeight, nextpage , action, tagID, text, radius ) " +
                                       "select page , itemtype , 10, 10, posWidth, posHeight, nextpage, action, tagID, text,radius from items " +
                                       "where ItemID = @ItemID";

                SqlCommand cmdGetData = new SqlCommand(sqlInsertData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", oTelegram.ItemID);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (GetLatestItemID());

            }
            catch (Exception ex)
            {
                return (-1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int moveItem(ScadaClasses.Telegram oTelegram)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "UPDATE Items SET posLeft = @posLeft, posTop = @PosTop WHERE ItemID = @ItemID";
                Connection.Open();
                SqlCommand cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", oTelegram.ItemID);
                cmdGetData.Parameters.AddWithValue("@posLeft", oTelegram.Left);
                cmdGetData.Parameters.AddWithValue("@posTop", oTelegram.Top);

                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int AddItemDefaultTags(ScadaClasses.Telegram Item)
        {
            DeleteAllTagsFromUxItem(Item.ItemID);
            switch (Item.ItemType)
            {
                case ScadaClasses.uxNumeric:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Processvalue");
                    break;

                case ScadaClasses.uxButton:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Setvalue");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 2, "Feedback");
                    break;

                case ScadaClasses.uxToggle:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Setvalue");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 2, "Feedback");
                    break;

                case ScadaClasses.uxBarGraph:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Processvalue");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 2, "Setvalue");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 3, "Outputvalue");
                    break;

                case ScadaClasses.uxCircularProgress:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Processvalue");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 2, "Setvalue");
                    break;

                case ScadaClasses.uxCircularGauge:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Processvalue");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 2, "Setvalue");
                    break;
                case ScadaClasses.uxGyro:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Bankangle");
                    break;
                case ScadaClasses.uxCompass:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Processvalue");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 2, "Setvalue");
                    break;
                case ScadaClasses.uxAltimeter:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Altitude");
                    break;
                case ScadaClasses.uxAirspeed:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Airspeed");
                    break;
                case ScadaClasses.uxRobot:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Gripper");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 2, "Lower arm");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 3, "Upper arm");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 4, "X Traverse");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 5, "Y Traverse");
                    break;

                case ScadaClasses.uxHistoryChart:
                    CreateChartSettings(Item.ItemID);
                    break;

                case ScadaClasses.uxController:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Process value");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 2, "Set value");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 3, "Output value");
                    break;

                case ScadaClasses.uxSimulator:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Process value");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 2, "Acutator");
                    break;

                case ScadaClasses.uxInterference:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Process value");
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 2, "Output value");
                    break;

                case ScadaClasses.uxAutomotivePower:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Power");
                    break;
                case ScadaClasses.uxAutomotiveSpeed:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Speed");
                    break;
                case ScadaClasses.uxProgressBar:
                    AddItemTag(Item.ItemID, Item.ItemType, -1, 1, "Progress");
                    break;

            }
            return 0;
        }

        public int GetMaxPageNo()
        {
            int MaxPage = 1;

            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "select max(page) from items";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        MaxPage = reader.GetInt32(0) + 1;
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
            return (MaxPage);
        }


        public int addPage()
        {
            int MaxPage = 1;
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "select max(page) from items";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        MaxPage = reader.GetInt32(0) + 1;
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }

            ScadaClasses.Telegram oTele = new ScadaClasses.Telegram()
            {
                MessageType = 0,
                Page = MaxPage,
                Top = 20.0,
                Left = 47.5,
                ItemType = 3,
                TagID = 1,
                Text = " ",
                SV = "",
                TagName = "",
                Action = "",
                Radius = 10.0
            };
            oTele.Width = GetDefaultWidth(oTele.ItemType);
            oTele.Height = GetDefaultHeight(oTele.ItemType);

            addItem(oTele);
            int ItemID = GetLatestItemID();
            oTele.ItemID = ItemID;
            AddItemDefaultTags(oTele);
            return (MaxPage);
        }


        public int addItem(ScadaClasses.Telegram oTelegram)
        {
            if (oTelegram.Text == null)
            {
                oTelegram.Text = "";
            }

            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                Connection.Open();
                string sqlInsertData = "INSERT INTO items values( @page,@itemtype,@posLeft,@posTop,@posWidth,@posHeight,@Nextpage,@Action,@TagID,@Text,DEFAULT,@Radius )";
                SqlCommand cmdGetData = new SqlCommand(sqlInsertData, Connection);
                cmdGetData.Parameters.AddWithValue("@page", oTelegram.Page);
                cmdGetData.Parameters.AddWithValue("@itemtype", oTelegram.ItemType);
                cmdGetData.Parameters.AddWithValue("@posLeft", 49.0);
                cmdGetData.Parameters.AddWithValue("@posTop", 48.0);
                cmdGetData.Parameters.AddWithValue("@posWidth", Convert.ToDouble(oTelegram.Width));
                cmdGetData.Parameters.AddWithValue("@posHeight", Convert.ToDouble(oTelegram.Height));
                cmdGetData.Parameters.AddWithValue("@Nextpage", oTelegram.Nextpage);
                cmdGetData.Parameters.AddWithValue("@Action", oTelegram.Action);
                cmdGetData.Parameters.AddWithValue("@TagID", oTelegram.TagID);
                cmdGetData.Parameters.AddWithValue("@Text", oTelegram.Text);
                cmdGetData.Parameters.AddWithValue("@Radius", oTelegram.Radius);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                Connection.Close();
                Connection = null;
                return (0);
            }
            catch (Exception ex)
            {
                return (1);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int deleteItem(int ItemID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "DELETE FROM Items WHERE ItemID = @ItemID";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);

                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return (1);
            }
            catch (Exception ex)
            {
                return (0);
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public int SetItemPage(int ItemID, int Page)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "UPDATE Items SET Page = @Page WHERE ItemID = @ItemID";
                Connection.Open();
                var cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                cmdGetData.Parameters.AddWithValue("@Page", Page);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return 0;
            }
            catch (Exception ex)
            {
                return (-1);
                throw new Exception(ex.ToString(), ex);

            }
            finally
            {
            }
        }

        public int SetNextPage(int ItemID, int NextPage)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "UPDATE Items SET NextPage = @NextPage WHERE ItemID = @ItemID";
                Connection.Open();
                var cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@ItemID", ItemID);
                cmdGetData.Parameters.AddWithValue("@NextPage", NextPage);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return 0;
            }
            catch (Exception ex)
            {
                return (-1);
                throw new Exception(ex.ToString(), ex);

            }
            finally
            {
            }
        }


       
        public int GetTagType(int TagID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                int TagType = 0;
                string sqlGetData = "SELECT Digital FROM Tags WHERE TagID = @TagID";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        TagType = reader.GetInt32(0);
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return TagType;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public int GetTagIDByName(string sTagName)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                int TagID = 0;
                string sqlGetData = "SELECT TagID FROM Tags WHERE TagName = @TagName";
                Connection.Open();
                var cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagName", sTagName);

                cmdGetData.ExecuteNonQuery();
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        TagID = reader.GetInt32(0);
                    }
                }

                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return TagID;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }

        }

        public int SetTagValueByName(string sTagName, float Value)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "UPDATE Tags SET Value = @Value WHERE TagName = @TagName";
                Connection.Open();
                var cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagName", sTagName);
                cmdGetData.Parameters.AddWithValue("@Value", Value);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public float GetTagValueByName(string sTagName)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                double Value = -1;
                string sqlGetData = "SELECT Value FROM Tags WHERE TagName = @sTagName";
                Connection.Open();
                var cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@sTagName", sTagName);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        Value = reader.GetDouble(0);
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return (float)Value;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public string GetTagColorByName(string sTagName)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sColor = "";
                string sqlGetData = "SELECT Color FROM Tags WHERE TagName = @sTagName";
                Connection.Open();
                var cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@sTagName", sTagName);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        sColor = reader.GetString(0);
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return sColor;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public string GetTagColor(int TagID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sColor = "";
                string sqlGetData = "SELECT Color FROM Tags WHERE TagID = @TagID";
                Connection.Open();
                var cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        sColor = reader.GetString(0);
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return sColor;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public string SetTagColor(string sColor, int TagID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "UPDATE Tags SET Color = @Color WHERE TagID = @TagID";
                Connection.Open();
                var cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                cmdGetData.Parameters.AddWithValue("@Color", sColor);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return sColor;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

   

        public ScadaClasses.Tag GetTag(int TagID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };

                string sqlGetData = "SELECT TagName,Description,Color,Unit FROM Tags WHERE TagID = @TagID";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                var reader = cmdGetData.ExecuteReader();

                var myTag = new Tag();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        myTag.Name = reader.GetString(0);
                    }
                    if (reader.IsDBNull(1) == false)
                    {
                        myTag.Description = reader.GetString(1);
                    }
                    if (reader.IsDBNull(2) == false)
                    {
                        myTag.Color = reader.GetString(2);
                    }
                    if (reader.IsDBNull(3) == false)
                    {
                        myTag.Unit = reader.GetString(3);
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return myTag;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public string GetTagName(int TagID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string TagName = "";
                string sqlGetData = "SELECT TagName FROM Tags WHERE TagID = @TagID";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        TagName = reader.GetString(0);
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return TagName;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }


        public string GetTagDescription(int TagID)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string Description = "";
                string sqlGetData = "SELECT Description FROM Tags WHERE TagID = @TagID";
                Connection.Open();
                var cmdGetData = new Microsoft.Data.SqlClient.SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagID", TagID);
                var reader = cmdGetData.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0) == false)
                    {
                        Description = reader.GetString(0);
                    }
                }
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return Description;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        public string SetTagColorByTagName(string TagName, string sColor)
        {
            try
            {
                var Connection = new Microsoft.Data.SqlClient.SqlConnection
                {
                    ConnectionString = sConnection
                };
                string sqlGetData = "UPDATE Tags SET Color = @Color WHERE TagName = @TagName";
                Connection.Open();
                var cmdGetData = new SqlCommand(sqlGetData, Connection);
                cmdGetData.Parameters.AddWithValue("@TagName", TagName);
                cmdGetData.Parameters.AddWithValue("@Color", sColor);
                cmdGetData.ExecuteNonQuery();
                cmdGetData.Dispose();
                cmdGetData = null;
                Connection.Close();
                Connection = null;
                return sColor;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString(), ex);
            }
            finally
            {
            }
        }

        /*
        public void SetSystemColors(int ColorScheme)
        {
            switch (ColorScheme)
            {
                case 0:
                    var c = GetTagColorByName("System_Lt_BackgroundColor");
                    SetTagColorByTagName("System_BackgroundColor", "rgba(0,0,0,255)");
                    SetTagColorByTagName("System_PopupColor", "rgba(70,70,70,245)");
                    SetTagColorByTagName("System_PopupItemColor", "rgba(120,120,120,50)");
                    SetTagColorByTagName("System_ItemColor", "rgba(50,50,50,240)");
                    SetTagColorByTagName("System_ItemBackgroundColor", "rgba(40,40,40,240)");
                    SetTagColorByTagName("System_PanelColor", "rgba(30,30,30,150)");
                    SetTagColorByTagName("System_TextColor", "rgba(250,250,250,220)");
                    SetTagColorByTagName("System_HoverColor", "rgba(125,125,125,235)");
                    SetTagColorByTagName("System_TouchColor", "rgba(128,128,128,235)");
                    SetTagColorByTagName("System_OffColor", "rgba(118,118,118,235)");
                    SetTagColorByTagName("System_LightColor", "rgba(52,52,52,240)");
                    SetTagColorByTagName("System_GridThinColor", "rgba(40,40,40,200)");
                    SetTagColorByTagName("System_GridFatColor", "rgba(45,45,45,200)");
                    break;

                case 1:
                    SetTagColorByTagName("System_BackgroundColor", "rgba(225,225,225,255)");
                    SetTagColorByTagName("System_PopupColor", "rgba(170,170,170,245)");
                    SetTagColorByTagName("System_PopupItemColor", "rgba(135,135,135,50)");
                    SetTagColorByTagName("System_ItemColor", "rgba(180,180,180,255)");
                    SetTagColorByTagName("System_ItemBackgroundColor", "rgba(200,200,200,255)");
                    SetTagColorByTagName("System_PanelColor", "rgba(230,230,230,150)");
                    SetTagColorByTagName("System_TextColor", "rgba(0,0,0,250)");
                    SetTagColorByTagName("System_HoverColor", "rgba(210,210,210,245)");
                    SetTagColorByTagName("System_TouchColor", "rgba(128,128,128,255)");
                    SetTagColorByTagName("System_OffColor", "rgba(128,128,128,235)");
                    SetTagColorByTagName("System_LightColor", "rgba(170,170,170,255)");
                    SetTagColorByTagName("System_GridThinColor", "rgba(210,210,210,200)");
                    SetTagColorByTagName("System_GridFatColor", "rgba(205,205,205,200)");
                break;

            };
        }
        */
    }
}
