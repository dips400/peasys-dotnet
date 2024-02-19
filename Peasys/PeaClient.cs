using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Peasys
{
    /// <summary>
    /// Represents the client part of the client-server architecture of the Peasys technology.
    /// </summary>
    public class PeaClient
    {
        public readonly string LicenseKey;
        public readonly string PartitionName;
        public readonly bool RetreiveStatistics;
        public readonly string UserName;
        public readonly int Port;
        public readonly string ConnectionMessage;
        public readonly int ConnectionStatus;

        private readonly NetworkStream Stream;
        private readonly TcpClient TcpClient = new();
        private readonly string EndPack = "dipsjbiemg";
        private readonly HttpClient httpClient;

        public readonly static Encoding Asen = Encoding.GetEncoding("ISO-8859-1");

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaClient"/> class. Initiates a connexion with the AS/400 server.
        /// </summary>
        /// <param name="partitionName">DNS name (name of the partition) of the remote AS/400 server.</param>
        /// <param name="port">Port used for the data exchange between the client and the server.</param>
        /// <param name="userName">Username of the AS/400 profile used for connexion.</param>
        /// <param name="password">Password of the AS/400 profile used for connexion.</param>
        /// <param name="licenseKey">License key delivered by DIPS after subscription.</param>
        /// <exception cref="PeaInvalidCredentialsException">Exception thrown when <param name="userName"> and/or <param name="password"> are wrong.</exception>
        /// <exception cref="PeaConnexionException">Exception thrown when the client was not able to succefully connect to the server.</exception>
        public PeaClient(string partitionName, int port, string userName, string password, string licenseKey, bool retreiveStatistics)
        {
            if (string.IsNullOrEmpty(partitionName) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(licenseKey))
            {
                throw new PeaInvalidCredentialsException("Parameters of the PeaClient should not be either null or empty");
            }

            if (userName.Length > 10 || password.Length > 10)
            {
                this.ConnectionStatus = -1;
                this.ConnectionMessage = "userName and/or password too long";
                throw new PeaInvalidCredentialsException("user & password must be less than 10 characters");
            }

            PartitionName = partitionName;
            Port = port;
            UserName = userName;
            LicenseKey = licenseKey;
            RetreiveStatistics = retreiveStatistics;

            // Retrieve token
            bool IsValid = true;
            string Token = "";
            try
            {
                httpClient = new()
                {
                    BaseAddress = new Uri("http://localhost:8080"), // TO be changed when deployed
                };

                using HttpResponseMessage response = httpClient.GetAsync($"api/license-key/retrieve-token/{partitionName}/{licenseKey}").Result;

                var jsonResponse = response.Content.ReadAsStringAsync().Result;

                JObject jsonObject = JObject.Parse(jsonResponse);
                IsValid = (bool)jsonObject.GetValue("isValid");
                Token = (string)jsonObject.GetValue("token");

                if (!IsValid)
                {
                    throw new PeaInvalidLicenseKeyException("Your subscription is not valid, visit TODO for more information.");
                }
            }
            catch (Exception ex)
            {
                throw new PeaConnexionException("Exception during license key check process", ex);
            }

            try
            {
                TcpClient.Connect(partitionName, port);
            }
            catch (Exception ex)
            {
                throw new PeaConnexionException("Error connecting the TCP client", ex);
            }
            Stream = TcpClient.GetStream();


            // ADD TOKEN 
            string login = userName.PadRight(10);

            login += password.PadRight(10);

            byte[] ba = Asen.GetBytes(login);
            Stream.Write(ba, 0, ba.Length);

            byte[] bb = new byte[1];
            _ = Stream.Read(bb, 0, 1);

            ConnectionStatus = int.Parse(Encoding.UTF8.GetString(bb));
            switch (ConnectionStatus)
            {
                case 1:
                    SendStatistics(new ConnexionUpdate(UserName, DateTime.Now, LicenseKey));
                    ConnectionMessage = "Connected";
                    break;
                case 2:
                    ConnectionMessage = "Unable to set profile";
                    throw new PeaConnexionException("Unable to set profile");
                case 3:
                    ConnectionMessage = "Invalid credential";
                    throw new PeaInvalidCredentialsException("Invalid userName or password, check again");
                case 4:
                    ConnectionMessage = "Invalid serial number/model";
                    throw new PeaInvalidCredentialsException("Invalid model or serial number");
                case 5:
                    ConnectionMessage = "Product expired";
                    throw new PeaConnexionException("Product expired");
                default:
                    throw new PeaConnexionException("Exception during connexion process, contact us for more informations");
            }

        }

        /// <summary>
        /// Sends the SELECT SQL query to the server that execute it and retrieve the desired data.
        /// </summary>
        /// <param name="query">SQL query that should starts with the SELECT keyword.</param>
        /// <returns>Returns a <see cref="PeaSelectResponse"/> object, further used to exploit data.</returns>
        /// <exception cref="PeaInvalidSyntaxQueryException">Thrown if the query syntax is invalid.</exception>
        public PeaSelectResponse ExecuteSelect(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new PeaInvalidSyntaxQueryException("Query should not be either null or empty");
            if (!query.ToUpper().StartsWith("SELECT")) throw new PeaInvalidSyntaxQueryException("Query should starts with the SELECT SQL keyword");

            (Dictionary<string, List<dynamic>> Result, string[] ColumnsName, int RowCount, string ReturnedSQLState, string ReturnedSQLMessage) = BuildData(query);

            return new PeaSelectResponse(ReturnedSQLState == "00000", ReturnedSQLMessage, ReturnedSQLState, Result, RowCount, ColumnsName);
        }

        /// <summary>
        /// Sends the UPDATE SQL query to the server that execute it and retrieve the desired data.
        /// </summary>
        /// <param name="query">SQL query that should starts with the UPDATE keyword.</param>
        /// <returns>Returns a <see cref="PeaUpdateResponse"/> object, further used to exploit data.</returns>
        /// <exception cref="PeaInvalidSyntaxQueryException">Thrown if the query syntax is invalid.</exception>
        public PeaUpdateResponse ExecuteUpdate(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new PeaInvalidSyntaxQueryException("Query should not be either null or empty");
            if (!query.ToUpper().StartsWith("UPDATE")) throw new PeaInvalidSyntaxQueryException("Query should starts with the UPDATE SQL keyword");

            (int row_count, string SqlState, string SqlMessage) = ModifyTable(query);
            bool has_succeeded = SqlState == "00000" || SqlState == "01504";

            return new PeaUpdateResponse(has_succeeded, SqlState, SqlMessage, row_count);
        }

        /// <summary>
        /// Sends the CREATE SQL query to the server that execute it and retrieve the desired data.
        /// </summary>
        /// <param name="query">SQL query that should starts with the CREATE keyword.</param>
        /// <returns>Returns a <see cref="PeaCreateResponse"/> object, further used to exploit data.</returns>
        /// <exception cref="PeaInvalidSyntaxQueryException">Thrown if the query syntax is invalid.</exception>
        public PeaCreateResponse ExecuteCreate(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new PeaInvalidSyntaxQueryException("Query should not be either null or empty");
            if (!query.ToUpper().StartsWith("CREATE")) throw new PeaInvalidSyntaxQueryException("Query should starts with the CREATE SQL keyword");

            (_, string SqlState, string SqlMessage) = ModifyTable(query);

            string[] query_words = query.Split(' ');

            Dictionary<string, ColumnInfo>? tb_schema = null;
            if (query_words[1].ToUpper() == "TABLE")
            {
                string[] names = query_words[2].Split('/');
                tb_schema = RetreiveTableSchema(names[1], names[0]);
            }

            return query_words[1].ToUpper() switch
            {
                "TABLE" => new PeaCreateResponse(SqlState == "00000", SqlMessage, SqlState, "", "", tb_schema),
                "INDEX" => new PeaCreateResponse(SqlState == "00000", SqlMessage, SqlState, "", query_words[2], tb_schema),
                "DATABASE" => new PeaCreateResponse(SqlState == "00000", SqlMessage, SqlState, query_words[2], "", tb_schema),
                _ => throw new PeaInvalidSyntaxQueryException(query),
            };
        }

        /// <summary>
        /// Sends the DELETE SQL query to the server that execute it and retrieve the desired data.
        /// </summary>
        /// <param name="query">SQL query that should starts with the DELETE keyword.</param>
        /// <returns>Returns a <see cref="PeaDeleteResponse"/> object, further used to exploit data.</returns>
        /// <exception cref="PeaInvalidSyntaxQueryException">Thrown if the query syntax is invalid.</exception>
        public PeaDeleteResponse ExecuteDelete(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new PeaInvalidSyntaxQueryException("Query should not be either null or empty");
            if (!query.ToUpper().StartsWith("DELETE")) throw new PeaInvalidSyntaxQueryException("Query should starts with the DELETE SQL keyword");

            (int row_count, string SqlState, string SqlMessage) = ModifyTable(query);

            return new PeaDeleteResponse(SqlState == "00000", SqlMessage, SqlState, row_count);
        }

        /// <summary>
        /// Sends the ALTER SQL query to the server that execute it and retrieve the desired data.
        /// </summary>
        /// <param name="query">SQL query that should starts with the ALTER keyword.</param>
        /// <returns>Returns a <see cref="PeaAlterResponse"/> object, further used to exploit data.</returns>
        /// <exception cref="PeaInvalidSyntaxQueryException">Thrown if the query syntax is invalid.</exception>
        public PeaAlterResponse ExecuteAlter(string query, bool retreiveTableSchema)
        {
            if (string.IsNullOrEmpty(query)) throw new PeaInvalidSyntaxQueryException("Query should not be either null or empty");
            if (!query.ToUpper().StartsWith("ALTER")) throw new PeaInvalidSyntaxQueryException("Query should starts with the ALTER SQL keyword");

            (_, string SqlState, string SqlMessage) = ModifyTable(query);

            Dictionary<string, ColumnInfo>? tb_schema = null;
            if (retreiveTableSchema)
            {
                string[] query_words = query.Split(' ');
                string[] names = query_words[2].Split('/');
                tb_schema = RetreiveTableSchema(names[1], names[0]);
            }

            return new PeaAlterResponse(SqlState == "00000", SqlMessage, SqlState, tb_schema);
        }

        /// <summary>
        /// Sends the DROP SQL query to the server that execute it and retrieve the desired data.
        /// </summary>
        /// <param name="query">SQL query that should starts with the DROP keyword.</param>
        /// <returns>Returns a <see cref="PeaDropResponse"/> object, further used to exploit data.</returns>
        /// <exception cref="PeaInvalidSyntaxQueryException">Thrown if the query syntax is invalid.</exception>
        public PeaDropResponse ExecuteDrop(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new PeaInvalidSyntaxQueryException("Query should not be either null or empty");
            if (!query.ToUpper().StartsWith("DROP")) throw new PeaInvalidSyntaxQueryException("Query should starts with the DROP SQL keyword");

            (_, string SqlState, string SqlMessage) = ModifyTable(query);

            return new PeaDropResponse(SqlState == "00000", SqlMessage, SqlState);
        }

        /// <summary>
        /// Sends the INSERT SQL query to the server that execute it and retrieve the desired data.
        /// </summary>
        /// <param name="query">SQL query that should starts with the INSERT keyword.</param>
        /// <returns>Returns a <see cref="PeaInsertResponse"/> object, further used to exploit data.</returns>
        /// <exception cref="PeaInvalidSyntaxQueryException">Thrown if the query syntax is invalid.</exception>
        public PeaInsertResponse ExecuteInsert(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new PeaInvalidSyntaxQueryException("Query should not be either null or empty");
            if (!query.ToUpper().StartsWith("INSERT")) throw new PeaInvalidSyntaxQueryException("Query should starts with the INSERT SQL keyword");

            (int row_count, string SqlState, string SqlMessage) = ModifyTable(query);

            return new PeaInsertResponse(SqlState == "00000", SqlState, SqlMessage, row_count);
        }

        /// <summary>
        /// Sends the SQL query to the server that execute it and retrieve the desired data. Use for sending complexe SQL queries to the server.
        /// </summary>
        /// <param name="query">SQL query.</param>
        /// <returns>Returns a <see cref="PeaInsertResponse"/> object, further used to exploit data.</returns>
        /// <exception cref="PeaInvalidSyntaxQueryException">Thrown if the query syntax is invalid.</exception>
        public PeaResponse ExecuteSQL(string query)
        {
            (int _, string SqlState, string SqlMessage) = ModifyTable(query);

            return new PeaResponse(SqlState == "00000", SqlState, SqlMessage);
        }

        /// <summary>
        /// Sends a OS/400 command to the server and retreives the potential warning messages.
        /// </summary>
        /// <param name="command">The command that will be sent to the server.</param>
        /// <returns>Returns a <see cref="PeaCommandResponse"/> object, further used to exploit data.</returns>
        /// <exception cref="">TODO</exception>
        public PeaCommandResponse ExecuteCommand(string command)
        {
            string customCmd = "exas" + command + EndPack;
            int descriptionOffset = 112;
            Regex rgx = new("[^a-zA-Z0-9 áàâäãåçéèêëíìîïñóòôöõúùûüýÿæœÁÀÂÄÃÅÇÉÈÊËÍÌÎÏÑÓÒÔÖÕÚÙÛÜÝŸÆŒ._'*/:-]");

            string result_raw = RetreiveData(customCmd);
            List<string> result = new();
            bool hasSucceeded = true;
            try
            {
                Regex rx = new("C[A-Z]{2}[0-9]{4}");
                foreach (Match match in rx.Matches(result_raw).Cast<Match>())
                {

                    if (!match.ToString().StartsWith("CPI"))
                    {
                        string description = result_raw.Substring(match.Index + descriptionOffset, result_raw.Length - match.Index - descriptionOffset);
                        description = description[..description.IndexOf('.')];
                        description = rgx.Replace(description, "");

                        result.Add(match + " " + description);
                    }

                    if (match.ToString().StartsWith("CPF"))
                    {
                        hasSucceeded = false;
                    }
                }
                return new PeaCommandResponse(hasSucceeded, result);
            }
            catch (Exception)
            {
                return new(false, new());
            }
        }

        /// <summary>
        /// Closes the TCP connexion with the server.
        /// </summary>
        public void Disconnect()
        {
            // use pea_deconnect
            // command to indiate end of connection to the server
            string end_block = "stopdipsjbiemg";

            // change command to byte array, send to server and cloe the TCP client
            byte[] ba = Asen.GetBytes(end_block);
            if (Stream != null)
            {
                Stream.Write(ba, 0, ba.Length);
                Stream.Flush();
                Stream.Close();
            }

            TcpClient.Close();
        }

        // private functions
        private (Dictionary<string, List<dynamic>>, string[], int, string, string) BuildData(string query)
        {
            string cmd1 = "geth" + query + EndPack;

            string header = RetreiveData(cmd1);

            ArrayList list_name = new(), list_type = new(), list_prec = new(), list_scal = new();
            string SqlState = "";
            string SqlMessage = "";
            try
            {
                JArray a = JArray.Parse(header);
                foreach (JObject o in a.Children<JObject>())
                {
                    foreach (JProperty p in o.Properties())
                    {
                        switch (p.Name)
                        {
                            case "name":
                                list_name.Add(p.Value);
                                break;
                            case "type":
                                list_type.Add(p.Value);
                                break;
                            case "prec":
                                list_prec.Add(p.Value);
                                break;
                            case "scal":
                                list_scal.Add(p.Value);
                                break;
                            default:
                                throw new PeaQueryException();
                        }
                    }
                }
            }
            catch (Exception)
            {
                SqlState = header.Substring(1, 5);
                SqlMessage = header[6..];
                return (null, null, 0, SqlMessage, SqlState);
            }

            int nb_col = list_prec.Count;
            string[] colname = new string[nb_col];

            int sum_precision = 0;
            for (int j = 0; j < nb_col; j++)
            {
                colname[j] = list_name[j].ToString().Trim().ToLower();
                sum_precision += int.Parse(list_prec[j].ToString());
            }

            Dictionary<string, List<dynamic>> result = new Dictionary<string, List<dynamic>>();
            for (int c = 0; c < nb_col; c++)
            {
                result.Add(colname[c], new List<dynamic>());
            }
            string cmd2 = "getd" + query + EndPack;

            string data = RetreiveData(cmd2);

            if (RetreiveStatistics)
            {
                SendStatistics(new DataUpdate("data_in", Asen.GetByteCount(query), DateTime.Now, LicenseKey));
                SendStatistics(new DataUpdate("data_out", Asen.GetByteCount(data), DateTime.Now, LicenseKey));
            }

            int nb_row = data.Length / sum_precision;
            int pointer = 0;
            while (!(pointer == data.Length))
            {
                for (int m = 0; m < nb_col; m++)
                {
                    int scale = int.Parse(list_scal[m].ToString());
                    int precision = int.Parse(list_prec[m].ToString());
                    int type = int.Parse(list_type[m].ToString());
                    // numeric packed
                    if ((type == 484 && scale != 0) || (type == 485 && scale != 0) || (type == 488 && scale != 0) || (type == 489 && scale != 0))
                    {
                        double temp_float_data = double.Parse(data.Substring(pointer, precision)) / Math.Pow(10, scale);
                        pointer += precision;
                        result[colname[m]].Add(temp_float_data);
                    }
                    // long
                    else if (type == 492 || type == 493)
                    {
                        result[colname[m]].Add(long.Parse(data.Substring(pointer, 20)));
                        pointer += 20;
                    }
                    // int
                    else if (type == 496 || type == 497)
                    {
                        result[colname[m]].Add(int.Parse(data.Substring(pointer, 10)));
                        pointer += 10;
                    }
                    // short
                    else if (type == 500 || type == 501)
                    {
                        result[colname[m]].Add(short.Parse(data.Substring(pointer, 5)));
                        pointer += 5;
                    }
                    // string, date, time, timestamp
                    else
                    {
                        if (type == 389)
                        {
                            result[colname[m]].Add(TimeOnly.Parse(data.Substring(pointer, precision)));
                        }
                        else if (type == 385)
                        {
                            result[colname[m]].Add(DateTime.Parse(data.Substring(pointer, precision)));
                        }
                        else
                        {
                            result[colname[m]].Add(data.Substring(pointer, precision));
                        }
                        pointer += precision;
                    }
                }
            }
            return (result, colname, nb_row, SqlState, SqlMessage);
        }

        private (int, string, string) ModifyTable(string query)
        {
            string cmd1 = "updt" + query + EndPack;
            string header = RetreiveData(cmd1);

            if (RetreiveStatistics)
            {
                SendStatistics(new DataUpdate("data_in", Asen.GetByteCount(query), DateTime.Now, LicenseKey));
                SendStatistics(new DataUpdate("data_out", Asen.GetByteCount(header), DateTime.Now, LicenseKey));
            }

            string SqlState = header.Substring(1, 5);
            string SqlMessage = header[6..].Trim();
            int row_count = 0;
            if (query.ToUpper().StartsWith("INSERT") || query.ToUpper().StartsWith("UPDATE") || query.ToUpper().StartsWith("DELETE"))
            {
                row_count = SqlState == "00000" ? int.Parse(SqlMessage[..1]) : 0;
            }

            return (row_count, SqlState, SqlMessage);
        }

        private string RetreiveData(string command)
        {
            StringBuilder data = new();

            byte[] ba = Asen.GetBytes(command);
            Stream.Write(ba, 0, ba.Length);
            Stream.Flush();

            if (command.StartsWith("geth") || command.StartsWith("updt"))
            {
                data.Append('[');
            }

            while (data.Length < EndPack.Length || data.ToString(data.Length - EndPack.Length, EndPack.Length) != EndPack)
            {
                byte[] bb = new byte[1];
                _ = Stream.Read(bb, 0, 1);

                data.Append(Convert.ToChar(bb[0]));
            }

            // build data as a string with the string building a remove the suffix 
            return data.ToString(0, data.Length - EndPack.Length);
        }

        private Dictionary<string, ColumnInfo> RetreiveTableSchema(string table_name, string schema_name)
        {
            string query =
                "SELECT COLUMN_NAME, ORDINAL_POSITION, DATA_TYPE, LENGTH, NUMERIC_SCALE, IS_NULLABLE, IS_UPDATABLE, LONG_COMMENT, NUMERIC_PRECISION " +
                $"FROM QSYS2.SYSCOLUMNS WHERE SYSTEM_TABLE_NAME = '{table_name.ToUpper()}' AND SYSTEM_TABLE_SCHEMA = '{schema_name.ToUpper()}'";

            string header = RetreiveData("geth" + query + EndPack);

            Console.WriteLine(header);

            ArrayList list_name = new(), list_type = new(), list_prec = new(), list_scal = new();
            try
            {
                JArray a = JArray.Parse(header);
                foreach (JObject o in a.Children<JObject>())
                {
                    foreach (JProperty p in o.Properties())
                    {
                        switch (p.Name)
                        {
                            case "name":
                                list_name.Add(p.Value);
                                break;
                            case "type":
                                list_type.Add(p.Value);
                                break;
                            case "prec":
                                list_prec.Add(p.Value);
                                break;
                            case "scal":
                                list_scal.Add(p.Value);
                                break;
                            default:
                                throw new PeaQueryException();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new PeaUnsupportedOperationException("It seems that the query your trying to make is not yet supported by Peasys, feel free to contact us");
            }

            string[] colname = new string[list_prec.Count];

            int nb_col = list_prec.Count;
            int sum_precision = 0;
            for (int j = 0; j < list_prec.Count; j++)
            {
                colname[j] = list_name[j].ToString().Trim().ToLower();
                sum_precision += int.Parse(list_prec[j].ToString());
            }

            string data = RetreiveData("getd" + query + EndPack);
            Console.WriteLine("===> data : " + data);

            List<List<dynamic>> result = new();
            int pointer = 0;
            while (!(pointer == data.Length))
            {
                List<dynamic> list = new List<dynamic>();
                for (int m = 0; m < nb_col; m++)
                {
                    int scale = int.Parse(list_scal[m].ToString());
                    int precision = int.Parse(list_prec[m].ToString());
                    int type = int.Parse(list_type[m].ToString());
                    // TODO add VARCHAR & VARGRAPHIC
                    // int
                    if (type == 496 || type == 497)
                    {
                        list.Add(int.Parse(data.Substring(pointer, 10)));
                        pointer += 10;
                    }
                    // string, date, time, timestamp
                    else
                    {
                        list.Add(data.Substring(pointer, precision));
                        pointer += precision;
                    }
                }
                result.Add(list);
            }

            Dictionary<string, ColumnInfo> tb_name = new Dictionary<string, ColumnInfo>();
            foreach (List<dynamic> list in result)
            {
                tb_name.Add(list[0], new ColumnInfo(list[0], list[1], list[2], list[3], list[4], list[5], list[6], list[7], list[8]));
            }

            return tb_name;
        }

        private void SendStatistics(Object data)
        {
            string output = JsonConvert.SerializeObject(data);
            _ = httpClient.PostAsync($"api/license-key/update", new StringContent(output, Encoding.UTF8, "application/json"));
        }

        private class ConnexionUpdate
        {
            public string Name;
            public DateTime Date;
            public string Key;

            public ConnexionUpdate(string name, DateTime date, string key)
            {
                Name = name;
                Date = date;
                Key = key;
            }
        }

        private class DataUpdate
        {
            public string Name;
            public int Bytes;
            public DateTime Date;
            public string Key;

            public DataUpdate(string name, int bytes, DateTime date, string key)
            {
                Name = name;
                Bytes = bytes;
                Date = date;
                Key = key;
            }
        }
    }
}