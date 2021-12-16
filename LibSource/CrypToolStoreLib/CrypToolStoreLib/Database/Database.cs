/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Tools;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace CrypToolStoreLib.Database
{
    /// <summary>
    /// The Database manages connections to the mysql database. It also offers methods to insert, update, and delete all objects of CrypToolStore in the database.
    /// Furthermore, it offers some check methods (e.g. developer's password)
    /// </summary>
    public class CrypToolStoreDatabase : IDisposable
    {
        private readonly Logger logger = Logger.GetLogger();
        private string databaseServer;
        private string databaseName;
        private string databaseUser;
        private string databasePassword;
        private DatabaseConnection[] connections;
        private static CrypToolStoreDatabase database;

        /// <summary>
        /// Return the instance of the database
        /// </summary>
        /// <returns></returns>
        public static CrypToolStoreDatabase GetDatabase()
        {
            if (database == null)
            {
                database = new CrypToolStoreDatabase();
            }

            return database;
        }

        /// <summary>
        /// Set constructor to private for singleton pattern
        /// </summary>
        private CrypToolStoreDatabase()
        {

        }

        /// <summary>
        /// Initializes the database and connects to the mysql database
        /// </summary>
        /// <param name="databaseServer"></param>
        /// <param name="databaseName"></param>
        /// <param name="databaseUser"></param>
        /// <param name="databasePassword"></param>
        /// <param name="numberOfConnections"></param>
        public bool InitAndConnect(string databaseServer, string databaseName, string databaseUser, string databasePassword, int numberOfConnections)
        {
            logger.LogText(string.Format("Connecting to mysql database with databaseServer={0}, databaseName={1}, databaseUser={2}", databaseServer, databaseName, databaseUser), this, Logtype.Info);
            try
            {
                this.databaseServer = databaseServer;
                this.databaseName = databaseName;
                this.databaseUser = databaseUser;
                this.databasePassword = databasePassword;
                CreateConnections(numberOfConnections);
                logger.LogText("Connection objects created", this, Logtype.Info);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogText(string.Format("Creating the connection objects failed with exception: {0}", ex.Message), this, Logtype.Error);
                return false;
            }
        }

        /// <summary>
        /// Creates connections to the mysql database; does NOT connect
        /// This "fixes" the problem, that when the server is rebootet and the database is not available,
        /// no connections can be made. Connections are made when they are actually needed now
        /// </summary>
        /// <param name="numberOfConnections"></param>
        private void CreateConnections(int numberOfConnections)
        {
            connections = new DatabaseConnection[numberOfConnections];
            for (int i = 0; i < connections.Length; i++)
            {
                connections[i] = new DatabaseConnection(databaseServer, databaseName, databaseUser, databasePassword);
            }
        }

        /// <summary>
        /// Returns next unused connection
        /// if all are used, returns a random one
        /// </summary>
        /// <returns></returns>
        private DatabaseConnection GetConnection()
        {
            foreach (DatabaseConnection connection in connections)
            {
                if (!connection.CurrentlyUsed())
                {
                    connection.CheckConnection();
                    return connection;
                }
            }
            Random random = new Random();
            int i = random.Next(0, connections.Length - 1);
            return connections[i];
        }

        /// <summary>
        /// Closes all open connections to mysql database
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Closes all open connections to mysql database
        /// </summary>
        public void Dispose()
        {
            logger.LogText("Closing all connections to database", this, Logtype.Debug);
            foreach (DatabaseConnection connection in connections)
            {
                try
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogText(string.Format("Exception occured while closing a connection to database: {0}", ex.Message), this, Logtype.Error);
                }
            }
            logger.LogText("All connections to database closed", this, Logtype.Debug);
        }

        #region database methods

        /// <summary>
        /// Creates a new developer account entry in the database
        /// uses pbkdf2 for creating the password hash
        /// uses RNGCryptoServiceProvider to create a salt for the hash
        /// </summary>
        /// <param name="username"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public void CreateDeveloper(string username, string firstname, string lastname, string email, string password, bool isAdmin = false)
        {
            logger.LogText(string.Format("Creating new developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Debug);
            string query = "insert into developers (username, firstname, lastname, email, password, passwordsalt, passworditerations, isadmin) values (@username, @firstname, @lastname, @email, @password, @passwordsalt, @passworditerations, @isadmin)";

            byte[] hash_bytes;
            byte[] salt_bytes = new byte[Constants.DATABASE_PBKDF2_HASH_LENGTH];
            using (RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(salt_bytes);
                Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt_bytes, Constants.DATABASE_PBKDF2_ITERATION_COUNT);
                hash_bytes = rfc2898DeriveBytes.GetBytes(Constants.DATABASE_PBKDF2_HASH_LENGTH);
            }

            string hash_string = Tools.Tools.ByteArrayToHexString(hash_bytes);
            string salt_string = Tools.Tools.ByteArrayToHexString(salt_bytes);

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@username", username},
                new object[]{"@firstname", firstname},
                new object[]{"@lastname", lastname},
                new object[]{"@email", email},
                new object[]{"@password", hash_string},
                new object[]{"@passwordsalt", salt_string},
                new object[]{"@passworditerations", Constants.DATABASE_PBKDF2_ITERATION_COUNT},
                new object[]{"@isadmin", isAdmin}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Created new developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Debug);
        }

        /// <summary>
        /// Checks, if a developer (username/password combination) exists
        /// returns false, if the username does not exist
        /// returns true, if the derived pbkdf2 hash from password is the same as the one in the database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool CheckDeveloperPassword(string username, string password)
        {
            DatabaseConnection connection = GetConnection();

            string query = "select * from developers where username=@username";

            object[][] parameters = new object[][]{
                new object[]{"@username", username},
            };

            List<Dictionary<string, object>> result = connection.ExecutePreparedStatement(query, parameters);

            //username does not exist, thus, return false
            if (result.Count == 0)
            {
                return false;
            }

            //otherwise, use salt and iterations to derive hash using pbkdf2
            string hash_from_database = (string)result[0]["password"];
            string salt_from_database = (string)result[0]["passwordsalt"];
            int password_iterations_from_database = (int)result[0]["passworditerations"];

            byte[] hash_bytes;
            byte[] salt_bytes = Tools.Tools.HexStringToByteArray(salt_from_database);
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt_bytes, password_iterations_from_database);
            hash_bytes = rfc2898DeriveBytes.GetBytes(hash_from_database.Length / 2);
            string hash_string = Tools.Tools.ByteArrayToHexString(hash_bytes);
            //finally return true, if hashes match; otherwise return false
            return hash_string.Equals(hash_from_database);
        }

        /// <summary>
        /// Returns the developer identified by his username
        /// if the developer does not exist returns null
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Developer GetDeveloper(string username)
        {
            DatabaseConnection connection = GetConnection();

            string query = "select username, firstname, lastname, email, isadmin from developers where username=@username";

            object[][] parameters = new object[][]{
                new object[]{"@username", username},
            };

            List<Dictionary<string, object>> result = connection.ExecutePreparedStatement(query, parameters);

            //username does not exist, thus, return false
            if (result.Count == 0)
            {
                return null;
            }

            //Create a new Developer object, fill it with data retrieved from database and return it
            Developer developer = new Developer
            {
                Username = (string)result[0]["username"],
                Firstname = (string)result[0]["firstname"],
                Lastname = (string)result[0]["lastname"],
                Email = (string)result[0]["email"],
                IsAdmin = (bool)result[0]["isadmin"]
            };
            return developer;
        }

        /// <summary>
        /// Updates an existing developer account entry in the database
        /// Does NOT update the password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="email"></param>
        /// <param name="isAdmin"></param>
        public void UpdateDeveloper(string username, string firstname, string lastname, string email, bool isAdmin = false)
        {
            logger.LogText(string.Format("Updating existing developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Debug);
            string query = "update developers set firstname=@firstname, lastname=@lastname, email=@email, isadmin=@isadmin where username=@username";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@firstname", firstname},
                new object[]{"@lastname", lastname},
                new object[]{"@email", email},
                new object[]{"@isadmin", isAdmin},
                new object[]{"@username", username},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updated existing developer: username={0}, firstname={1}, lastname={2}, email={3}, isadmin={4}", username, firstname, lastname, email, isAdmin == true ? "true" : "false"), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates an existing developer account entry in the database
        /// Does NOT update the password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="email"></param>
        /// <param name="isAdmin"></param>
        public void UpdateDeveloperNoAdmin(string username, string firstname, string lastname, string email)
        {
            logger.LogText(string.Format("Updating existing developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Debug);
            string query = "update developers set firstname=@firstname, lastname=@lastname, email=@email where username=@username";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@firstname", firstname},
                new object[]{"@lastname", lastname},
                new object[]{"@email", email},
                new object[]{"@username", username},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updated existing developer: username={0}, firstname={1}, lastname={2}, email={3}", username, firstname, lastname, email), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates an existing developer's account password in the database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void UpdateDeveloperPassword(string username, string password)
        {
            logger.LogText(string.Format("Updating existing developer's password: username={0}", username), this, Logtype.Debug);
            string query = "update developers set password=@password, passwordsalt=@passwordsalt, passworditerations=@passworditerations where username=@username";

            byte[] hash_bytes;
            byte[] salt_bytes = new byte[Constants.DATABASE_PBKDF2_HASH_LENGTH];
            using (RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(salt_bytes);
                Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt_bytes, Constants.DATABASE_PBKDF2_ITERATION_COUNT);
                hash_bytes = rfc2898DeriveBytes.GetBytes(Constants.DATABASE_PBKDF2_HASH_LENGTH);
            }

            string hash_string = Tools.Tools.ByteArrayToHexString(hash_bytes);
            string salt_string = Tools.Tools.ByteArrayToHexString(salt_bytes);

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@password", hash_string},
                new object[]{"@passwordsalt", salt_string},
                new object[]{"@passworditerations", Constants.DATABASE_PBKDF2_ITERATION_COUNT},
                new object[]{"@username", username}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updated existing developer's password: username={0}", username), this, Logtype.Debug);
        }

        /// <summary>
        /// Deletes a developer entry from the database
        /// </summary>
        /// <param name="username"></param>
        public void DeleteDeveloper(string username)
        {
            logger.LogText(string.Format("Deleting developer account: username={0}", username), this, Logtype.Debug);
            string query = "delete from developers where username=@username";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{

                new object[]{"@username", username}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Deleted developer account: username={0}", username), this, Logtype.Debug);
        }


        /// <summary>
        /// Returns a list of all developers currently stored in the database
        /// </summary>
        public List<Developer> GetDevelopers()
        {
            string query = "select username, firstname, lastname, email, isadmin from developers";

            DatabaseConnection connection = GetConnection();
            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, null);

            List<Developer> developers = new List<Developer>();

            foreach (Dictionary<string, object> entry in resultset)
            {
                Developer developer = new Developer
                {
                    Username = (string)entry["username"],
                    Firstname = (string)entry["firstname"],
                    Lastname = (string)entry["lastname"],
                    Email = (string)entry["email"],
                    IsAdmin = (bool)entry["isadmin"]
                };
                developers.Add(developer);
            }
            return developers;
        }

        /// <summary>
        /// Creates a new plugin for the dedicated developer, identified by his username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <param name="shortdescription"></param>
        /// <param name="longdescription"></param>
        /// <param name="authornames"></param>
        /// <param name="authoremails"></param>
        /// <param name="authorinstitutes"></param>
        /// <param name="icon"></param>
        public void CreatePlugin(string username, string name, string shortdescription, string longdescription, string authornames, string authoremails, string authorinstitutes, byte[] icon)
        {
            logger.LogText(string.Format("Creating new plugin: username={0}, name={1}, shortdescription={2}, longdescription={3}, authornames={4}, authoremails={5} authorinstitutes={6}, icon={7}",
                username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon != null ? icon.Length.ToString() : "null"), this, Logtype.Debug);
            string query = "insert into plugins (username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon) values (@username, @name, @shortdescription, @longdescription, @authornames, @authoremails, @authorinstitutes, @icon)";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@username", username},
                new object[]{"@name", name},
                new object[]{"@shortdescription", shortdescription},
                new object[]{"@longdescription", longdescription},
                new object[]{"@authornames", authornames},
                new object[]{"@authoremails", authoremails},
                new object[]{"@authorinstitutes", authorinstitutes},
                new object[]{"@icon", icon}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Created new plugin: username={0}, name={1}, shortdescription={2}, longdescription={3}, authornames={4}, authoremails={5} authorinstitutes={6}",
                username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates the dedicated plugin identified by its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="shortdescription"></param>
        /// <param name="longdescription"></param>
        /// <param name="authornames"></param>
        /// <param name="authoremails"></param>
        /// <param name="authorinstitutes"></param>
        /// <param name="icon"></param>
        public void UpdatePlugin(int id, string name, string shortdescription, string longdescription, string authornames, string authoremails, string authorinstitutes, byte[] icon)
        {
            logger.LogText(string.Format("Updating plugin: id={0}, name={1}, shortdescription={2}, longdescription={3}, authornames={4}, authoremails={5} authorinstitutes={6}, icon={7}",
                id, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon != null ? icon.Length.ToString() : "null"), this, Logtype.Debug);
            string query = "update plugins set name=@name, shortdescription=@shortdescription, longdescription=@longdescription, authornames=@authornames, authoremails=@authoremails, authorinstitutes=@authorinstitutes, icon=@icon where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@name", name},
                new object[]{"@shortdescription", shortdescription},
                new object[]{"@longdescription", longdescription},
                new object[]{"@authornames", authornames},
                new object[]{"@authoremails", authoremails},
                new object[]{"@authorinstitutes", authorinstitutes},
                new object[]{"@icon", icon},
                new object[]{"@id", id}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updated plugin: id={0}, name={1}, shortdescription={2}, longdescription={3}, authornames={4}, authoremails={5} authorinstitutes={6}, icon={7}",
                id, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon != null ? icon.Length.ToString() : "null"), this, Logtype.Debug);
        }

        /// <summary>
        /// Deletes the dedicated plugin identified by its id
        /// </summary>
        /// <param name="id"></param>
        public void DeletePlugin(int id)
        {
            logger.LogText(string.Format("Deleting plugin: id={0}", id), this, Logtype.Debug);
            string query = "delete from plugins where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@id", id}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Deleted plugin: id={0}", id), this, Logtype.Debug);
        }

        /// <summary>
        /// Returns a plugin from the database identified by its id
        /// If the plugin does not exist returns null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Plugin GetPlugin(int id)
        {
            string query = "select id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes, icon from plugins where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@id", id}
            };

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);
            if (resultset.Count == 0)
            {
                return null;
            }

            Plugin plugin = new Plugin
            {
                Id = (int)resultset[0]["id"],
                Username = (string)resultset[0]["username"],
                Name = (string)resultset[0]["name"],
                ShortDescription = (string)resultset[0]["shortdescription"],
                LongDescription = (string)resultset[0]["longdescription"],
                Authornames = (string)resultset[0]["authornames"],
                Authoremails = (string)resultset[0]["authoremails"],
                Authorinstitutes = (string)resultset[0]["authorinstitutes"],
                Icon = (byte[])resultset[0]["icon"]
            };

            return plugin;
        }

        /// <summary>
        /// Returns a list of plugins from the database
        /// If username is set, it only returns plugins of that user
        /// icons are NOT included to save bandwith for this request
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public List<Plugin> GetPlugins(string username = null)
        {
            string query;
            if (username == null)
            {
                query = "select id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes from plugins";
            }
            else
            {
                query = "select id, username, name, shortdescription, longdescription, authornames, authoremails, authorinstitutes from plugins where username=@username";
            }

            DatabaseConnection connection = GetConnection();

            object[][] parameters = null;

            if (username != null)
            {
                parameters = new object[][]{
                new object[]{"@username", username}
            };
            }

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);
            List<Plugin> plugins = new List<Plugin>();

            foreach (Dictionary<string, object> entry in resultset)
            {
                Plugin plugin = new Plugin
                {
                    Id = (int)entry["id"],
                    Username = (string)entry["username"],
                    Name = (string)entry["name"],
                    ShortDescription = (string)entry["shortdescription"],
                    LongDescription = (string)entry["longdescription"],
                    Authornames = (string)entry["authornames"],
                    Authoremails = (string)entry["authoremails"],
                    Authorinstitutes = (string)entry["authorinstitutes"]
                };
                plugins.Add(plugin);
            }
            return plugins;
        }

        /// <summary>
        /// Returns a list of PluginAndSource which are in publishstate (or lower)
        /// </summary>
        /// <param name="publishstate"></param>
        /// <returns></returns>
        public List<PluginAndSource> GetPublishedPlugins(PublishState publishstate)
        {
            string query = "SELECT a.id, a.username, a.pluginversion, a.buildversion, a.publishstate, a.name, a.shortdescription, a.longdescription, a.authornames, a.authoremails, " +
                           "a.authorinstitutes, a.icon, a.builddate FROM (SELECT p.id, p.username, p.name, p.shortdescription, p.longdescription, p.authornames, p.authoremails, " +
                           "p.authorinstitutes, p.icon, s.pluginid, s.pluginversion, s.publishstate, s.builddate, s.buildversion FROM plugins p " +
                           "INNER JOIN sources s ON p.id = s.pluginid WHERE s.publishstate IN ($LIST$) AND " +
                           "s.pluginversion = (select MAX(pluginversion) from sources where pluginid=p.id)) a GROUP BY a.id ORDER BY a.id ASC";

            DatabaseConnection connection = GetConnection();

            string list = "'DEVELOPER'";

            switch (publishstate)
            {
                default:
                case PublishState.DEVELOPER:
                    list = "'DEVELOPER', 'NIGHTLY','BETA','RELEASE'";
                    break;
                case PublishState.NIGHTLY:
                    list = "'NIGHTLY','BETA','RELEASE'";
                    break;
                case PublishState.BETA:
                    list = "'BETA','RELEASE'";
                    break;
                case PublishState.RELEASE:
                    list = "'RELEASE'";
                    break;
            }
            query = query.Replace("$LIST$", list);

            object[][] parameters = new object[][]{
            };

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);
            List<PluginAndSource> pluginsAndSources = new List<PluginAndSource>();

            foreach (Dictionary<string, object> entry in resultset)
            {
                Plugin plugin = new Plugin
                {
                    Id = (int)entry["id"],
                    Username = (string)entry["username"],
                    Name = (string)entry["name"],
                    ShortDescription = (string)entry["shortdescription"],
                    LongDescription = (string)entry["longdescription"],
                    Authornames = (string)entry["authornames"],
                    Authoremails = (string)entry["authoremails"],
                    Authorinstitutes = (string)entry["authorinstitutes"],
                    Icon = (byte[])entry["icon"]
                };

                Source source = new Source
                {
                    BuildVersion = Convert.ToInt32(entry["buildversion"]),
                    PluginId = plugin.Id,
                    PluginVersion = Convert.ToInt32(entry["pluginversion"]), // somehow, MAX() in MySQL does not return int32; thus we use convert
                    BuildDate = (DateTime)entry["builddate"],
                    PublishState = (string)entry["publishstate"]
                };

                PluginAndSource pluginAndSource = new PluginAndSource
                {
                    Plugin = plugin,
                    Source = source
                };
                pluginsAndSources.Add(pluginAndSource);
            }
            return pluginsAndSources;
        }

        /// <summary>
        /// Returns the newest plugin and source from the database identified by its id and its publishState
        /// If the plugin does not exist returns null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PluginAndSource GetPublishedPlugin(int id, PublishState publishstate)
        {
            string query = "SELECT a.id, a.username, a.pluginversion, a.buildversion, a.publishstate, a.name, a.shortdescription, a.longdescription, a.authornames, a.authoremails, " +
                           "a.authorinstitutes, a.icon, a.builddate FROM (SELECT p.id, p.username, p.name, p.shortdescription, p.longdescription, p.authornames, p.authoremails, " +
                           "p.authorinstitutes, p.icon, s.pluginid, s.pluginversion, s.publishstate, s.builddate, s.buildversion FROM plugins p " +
                           "INNER JOIN sources s ON p.id = s.pluginid WHERE s.publishstate IN ($LIST$) AND p.id=@id AND " +
                           "s.pluginversion = (select MAX(pluginversion) from sources where pluginid=p.id)) a GROUP BY a.id ORDER BY a.id ASC";

            DatabaseConnection connection = GetConnection();

            string list = "'DEVELOPER'";

            switch (publishstate)
            {
                default:
                case PublishState.DEVELOPER:
                    list = "'DEVELOPER', 'NIGHTLY','BETA','RELEASE'";
                    break;
                case PublishState.NIGHTLY:
                    list = "'NIGHTLY','BETA','RELEASE'";
                    break;
                case PublishState.BETA:
                    list = "'BETA','RELEASE'";
                    break;
                case PublishState.RELEASE:
                    list = "'RELEASE'";
                    break;
            }
            query = query.Replace("$LIST$", list);

            object[][] parameters = new object[][]{
                new object[]{"@id", id}
            };

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);
            if (resultset.Count == 0)
            {
                return null;
            }

            Plugin plugin = new Plugin
            {
                Id = (int)resultset[0]["id"],
                Username = (string)resultset[0]["username"],
                Name = (string)resultset[0]["name"],
                ShortDescription = (string)resultset[0]["shortdescription"],
                LongDescription = (string)resultset[0]["longdescription"],
                Authornames = (string)resultset[0]["authornames"],
                Authoremails = (string)resultset[0]["authoremails"],
                Authorinstitutes = (string)resultset[0]["authorinstitutes"]
            };

            Source source = new Source
            {
                BuildVersion = Convert.ToInt32(resultset[0]["buildversion"]),
                PluginId = plugin.Id,
                PluginVersion = Convert.ToInt32(resultset[0]["pluginversion"]), // somehow, MAX() in MySQL does not return int32; thus we use convert
                BuildDate = (DateTime)resultset[0]["builddate"],
                PublishState = (string)resultset[0]["publishstate"]
            };

            PluginAndSource pluginAndSource = new PluginAndSource
            {
                Plugin = plugin,
                Source = source
            };

            return pluginAndSource;
        }

        /// <summary>
        /// Creates a new source entry in the database
        /// </summary>
        /// <param name="source"></param>
        public void CreateSource(Source source)
        {
            logger.LogText(string.Format("Creating new source: pluginid={0}, pluginversion={1}, buildstate={2}, buildlog={3}", source.PluginId, source.PluginVersion, source.BuildState, source.BuildLog), this, Logtype.Debug);
            string query = "insert into sources (pluginid, pluginversion, zipfilename, assemblyfilename, buildstate, buildlog, publishstate) values (@pluginid, @pluginversion, @zipfilename, @assemblyfilename, @buildstate, @buildlog, @publishstate)";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@pluginid", source.PluginId},
                new object[]{"@pluginversion", source.PluginVersion},
                new object[]{"@zipfilename", string.Empty},
                new object[]{"@assemblyfilename", string.Empty},
                new object[]{"@buildstate", source.BuildState},
                new object[]{"@buildlog", source.BuildLog},
                new object[]{"@publishstate", PublishState.NOTPUBLISHED.ToString()}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Created new source: pluginid={0}, pluginversion={1}, buildstate={2}, buildlog={3}", source.PluginId, source.PluginVersion, source.BuildState, source.BuildLog), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates a source in the database identified by pluginid and pluginversion
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        /// <param name="zipfilename"></param>
        /// <param name="buildstate"></param>
        /// <param name="buildlog"></param>
        /// <param name="uploaddate"></param>
        public void UpdateSource(int pluginid, int pluginversion, string zipfilename, string buildstate, string buildlog, DateTime uploaddate)
        {
            logger.LogText(string.Format("Updating source: pluginid={0}, pluginversion={1}, zipfilename={2}, buildstate={3}, buildlog={4}, uploaddate={5}", pluginid, pluginversion, zipfilename, buildstate, buildlog, uploaddate), this, Logtype.Debug);
            string query = "update sources set zipfilename = @zipfilename, buildstate=@buildstate, buildlog=@buildlog, uploaddate=@uploaddate where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{

                new object[]{"@zipfilename", zipfilename},
                new object[]{"@buildstate", buildstate},
                new object[]{"@buildlog", buildlog},
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion},
                new object[]{"@uploaddate", uploaddate}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updating source: pluginid={0}, pluginversion={1}, zipfilename={2}, buildstate={3}, buildlog={4}, uploaddate={5}", pluginid, pluginversion, zipfilename, buildstate, buildlog, uploaddate), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates a source (only assembly file name) in the database identified by pluginid and pluginversion
        /// automatically sets the builddate to the uploadtime
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        /// <param name="assemblyfilename"></param>
        public void UpdateSource(int pluginid, int pluginversion, string assemblyfilename)
        {
            logger.LogText(string.Format("Updating source: pluginid={0}, pluginversion={1}, assemblyfilename={2}, builddate={3}", pluginid, pluginversion, assemblyfilename, DateTime.Now), this, Logtype.Debug);
            string query = "update sources set assemblyfilename=@assemblyfilename, builddate=@builddate where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{

                new object[]{"@assemblyfilename", assemblyfilename},
                new object[]{"@builddate", DateTime.Now},
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updated source: pluginid={0}, pluginversion={1}, assemblyfilename={2}, builddate={3}", pluginid, pluginversion, assemblyfilename, DateTime.Now), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates a source (only publishstate file name) in the database identified by pluginid and pluginversion
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        /// <param name="publishstate"></param>
        public void UpdateSource(int pluginid, int pluginversion, PublishState publishstate)
        {
            logger.LogText(string.Format("Updating source: pluginid={0}, pluginversion={1}, publishstate={2}", pluginid, pluginversion, publishstate.ToString()), this, Logtype.Debug);
            string query = "update sources set publishstate=@publishstate where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion},
                new object[]{"@publishstate", publishstate.ToString()}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updated source: pluginid={0}, pluginversion={1}, publishstate={2}", pluginid, pluginversion, publishstate.ToString()), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates a source in the database identified by pluginid and pluginversion
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        /// <param name="zipfilename"></param>
        /// <param name="buildstate"></param>
        /// <param name="buildlog"></param>
        public void UpdateSource(int pluginid, int pluginversion, string zipfilename, string buildstate, string buildlog, int buildversion)
        {
            logger.LogText(string.Format("Updating source: pluginid={0}, pluginversion={1}, zipfilename={2}, buildstate={3}, buildlog={4}, buildversion={5}", pluginid, pluginversion, zipfilename, buildstate, buildlog, buildversion), this, Logtype.Debug);
            string query = "update sources set zipfilename = @zipfilename, buildstate=@buildstate, buildlog=@buildlog, buildversion=@buildversion where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{

                new object[]{"@zipfilename", zipfilename},
                new object[]{"@buildstate", buildstate},
                new object[]{"@buildlog", buildlog},
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion},
                new object[]{"@buildversion", buildversion},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updated source: pluginid={0}, pluginversion={1}, zipfilename={2}, buildstate={3}, buildlog={4}, buildversion={5}", pluginid, pluginversion, zipfilename, buildstate, buildlog, buildversion), this, Logtype.Debug);
        }

        /// <summary>
        /// Deletes the dedicated source idenfified by pluginid and pluginversion
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        public void DeleteSource(int pluginid, int pluginversion)
        {
            logger.LogText(string.Format("Deleting source: pluginid={0}, pluginversion={1}", pluginid, pluginversion), this, Logtype.Debug);
            string query = "delete from sources where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Deleted source: pluginid={0}, pluginversion={1}", pluginid, pluginversion), this, Logtype.Debug);
        }

        /// <summary>
        /// Returns the dedicated Source identified by pluginid and pluginversion
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        /// <returns></returns>
        public Source GetSource(int pluginid, int pluginversion)
        {
            string query = "select pluginid, pluginversion, buildversion, zipfilename, buildstate, buildlog, assemblyfilename, uploaddate, builddate, publishstate from sources where pluginid=@pluginid and pluginversion=@pluginversion";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@pluginid", pluginid},
                new object[]{"@pluginversion", pluginversion}
            };

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);
            if (resultset.Count == 0)
            {
                return null;
            }

            Source source = new Source
            {
                PluginId = (int)resultset[0]["pluginid"],
                PluginVersion = (int)resultset[0]["pluginversion"],
                BuildVersion = (int)resultset[0]["buildversion"],
                ZipFileName = (string)resultset[0]["zipfilename"],
                BuildState = (string)resultset[0]["buildstate"],
                BuildLog = (string)resultset[0]["buildlog"],
                AssemblyFileName = (string)resultset[0]["assemblyfilename"],
                UploadDate = (DateTime)resultset[0]["uploaddate"],
                BuildDate = (DateTime)resultset[0]["builddate"],
                PublishState = (string)resultset[0]["publishstate"]
            };

            return source;
        }

        /// <summary>
        /// Returns a list of sources for the dedicated plugin idenfified by the pluginid
        /// </summary>
        /// <param name="pluginid"></param>
        /// <returns></returns>
        public List<Source> GetSources(int pluginid)
        {
            string query = "select pluginid, pluginversion, buildversion, buildstate, buildlog, uploaddate, builddate, zipfilename, assemblyfilename, publishstate from sources where pluginid=@pluginid";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@pluginid", pluginid},
            };

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);

            List<Source> sources = new List<Source>();

            foreach (Dictionary<string, object> entry in resultset)
            {
                Source source = new Source
                {
                    PluginId = (int)entry["pluginid"],
                    PluginVersion = (int)entry["pluginversion"],
                    BuildVersion = (int)entry["buildversion"],
                    BuildState = (string)entry["buildstate"],
                    BuildLog = (string)entry["buildlog"],
                    UploadDate = (DateTime)entry["uploaddate"],
                    BuildDate = (DateTime)entry["builddate"],
                    ZipFileName = (string)entry["zipfilename"],
                    AssemblyFileName = (string)entry["assemblyfilename"],
                    PublishState = (string)entry["publishstate"]
                };
                sources.Add(source);
            }
            return sources;
        }

        /// <summary>
        /// Returns a list of sources for with the dedicated buildstate
        /// <param name="buildstate"></param>
        /// </summary>
        public List<Source> GetSources(string buildstate)
        {
            string query = "select pluginid, pluginversion, buildversion, buildstate, buildlog, uploaddate, builddate, zipfilename, assemblyfilename, publishstate from sources where buildstate=@buildstate";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@buildstate", buildstate},
            };

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);

            List<Source> sources = new List<Source>();

            foreach (Dictionary<string, object> entry in resultset)
            {
                Source source = new Source
                {
                    PluginId = (int)entry["pluginid"],
                    PluginVersion = (int)entry["pluginversion"],
                    BuildVersion = (int)entry["buildversion"],
                    BuildState = (string)entry["buildstate"],
                    BuildLog = (string)entry["buildlog"],
                    UploadDate = (DateTime)entry["uploaddate"],
                    BuildDate = (DateTime)entry["builddate"],
                    ZipFileName = (string)entry["zipfilename"],
                    AssemblyFileName = (string)entry["assemblyfilename"],
                    PublishState = (string)entry["publishstate"]
                };
                sources.Add(source);
            }
            return sources;
        }

        /// <summary>
        /// Creates a new resource entry in the database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public void CreateResource(string username, string name, string description)
        {
            logger.LogText(string.Format("Creating new resource: username={0}, name={1}, description={2}", username, name, description), this, Logtype.Debug);
            string query = "insert into resources (username, name, description) values (@username, @name, @description)";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@username", username},
                new object[]{"@name", name},
                new object[]{"@description", description}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Created new resource: username={0}, name={1}, description={2}", username, name, description), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates the dedicated resource identified by its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public void UpdateResource(int id, string name, string description)
        {
            logger.LogText(string.Format("Updating resource: id={0}, name={1}, description={2}", id, name, description), this, Logtype.Debug);
            string query = "update resources set name=@name, description=@description where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@name", name},
                new object[]{"@description", description},
                new object[]{"@id", id}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updated resource: id={0}, name={1}, description={2}", id, name, description), this, Logtype.Debug);
        }

        /// <summary>
        /// Deletes the dedicated resource identified by its id
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public void DeleteResource(int id)
        {
            logger.LogText(string.Format("Deleting resource: id={0}", id), this, Logtype.Debug);
            string query = "delete from resources where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@id", id},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Deleted resource: id={0}", id), this, Logtype.Debug);
        }

        /// <summary>
        /// Returns the dedicated resource identified by its id
        /// Returns null, if the resource does not exist
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Resource GetResource(int id)
        {
            string query = "select id, username, name, description from resources where id=@id";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@id", id},
            };

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);
            if (resultset.Count == 0)
            {
                return null;
            }

            Resource resource = new Resource
            {
                Id = (int)resultset[0]["id"],
                Username = (string)resultset[0]["username"],
                Name = (string)resultset[0]["name"],
                Description = (string)resultset[0]["description"]
            };
            return resource;
        }

        /// <summary>
        /// Returns a list of resources from the database
        /// If username is set, it only returns resources of that user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Resource> GetResources(string username = null)
        {
            string query;
            if (username == null)
            {
                query = "select id, username, name, description from resources";
            }
            else
            {
                query = "select id, username, name, description from resources where username=@username";
            }

            DatabaseConnection connection = GetConnection();

            object[][] parameters = null;

            if (username != null)
            {
                parameters = new object[][]{
                new object[]{"@username", username}
            };
            }

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);
            List<Resource> resources = new List<Resource>();

            foreach (Dictionary<string, object> entry in resultset)
            {
                Resource resource = new Resource
                {
                    Id = (int)entry["id"],
                    Username = (string)entry["username"],
                    Name = (string)entry["name"],
                    Description = (string)entry["description"]
                };
                resources.Add(resource);
            }
            return resources;
        }

        /// <summary>
        /// Returns a list of ResourceAndResourceData which are in publishstate (or lower)
        /// </summary>
        /// <param name="publishstate"></param>
        /// <returns></returns>
        public List<ResourceAndResourceData> GetPublishedResources(PublishState publishstate)
        {
            string query = "SELECT " +
                "a.id, MAX(a.version) AS resourceversion, a.publishstate, a.name, a.description, a.username " +
                "FROM (SELECT resources.*, resourcesdata.* FROM resources INNER JOIN resourcesdata ON resources.id = resourcesdata.resourceid WHERE resourcesdata.publishstate IN ($LIST$)) a " +
                "GROUP BY a.id ORDER BY a.id ASC";

            DatabaseConnection connection = GetConnection();

            string list = "'DEVELOPER'";

            switch (publishstate)
            {
                default:
                case PublishState.DEVELOPER:
                    list = "'DEVELOPER', 'NIGHTLY','BETA','RELEASE'";
                    break;
                case PublishState.NIGHTLY:
                    list = "'NIGHTLY','BETA','RELEASE'";
                    break;
                case PublishState.BETA:
                    list = "'BETA','RELEASE'";
                    break;
                case PublishState.RELEASE:
                    list = "'RELEASE'";
                    break;
            }
            query = query.Replace("$LIST$", list);

            object[][] parameters = new object[][]{
            };

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);
            List<ResourceAndResourceData> resourcesAndResourceDatas = new List<ResourceAndResourceData>();

            foreach (Dictionary<string, object> entry in resultset)
            {
                Resource resource = new Resource
                {
                    Id = int.Parse(entry["id"].ToString()),
                    Username = (string)entry["username"],
                    Name = (string)entry["name"],
                    Description = (string)entry["description"]
                };

                ResourceData resourceData = new ResourceData
                {
                    ResourceId = resource.Id,
                    ResourceVersion = int.Parse(entry["resourceversion"].ToString()),
                    PublishState = (string)entry["publishstate"]
                };

                ResourceAndResourceData resourceAndResourceData = new ResourceAndResourceData
                {
                    Resource = resource,
                    ResourceData = resourceData
                };
                resourcesAndResourceDatas.Add(resourceAndResourceData);
            }
            return resourcesAndResourceDatas;
        }

        /// <summary>
        /// Returns the newest plugin and source from the database identified by its id and its publishState
        /// If the plugin does not exist returns null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ResourceAndResourceData GetPublishedResource(int id, PublishState publishstate)
        {
            string query = "SELECT " +
                "a.id, MAX(a.version) AS version, a.publishstate, a.name, a.description, a.username " +
                "FROM (SELECT resources.*, resourcesdata.* FROM resources INNER JOIN resourcesdata ON resources.id = resourcesdata.resourceid WHERE resourcesdata.publishstate IN ($LIST$) and resources.id=@id) a " +
                "GROUP BY a.id ORDER BY a.id ASC";

            DatabaseConnection connection = GetConnection();

            string list = "'DEVELOPER'";

            switch (publishstate)
            {
                default:
                case PublishState.DEVELOPER:
                    list = "'DEVELOPER', 'NIGHTLY','BETA','RELEASE'";
                    break;
                case PublishState.NIGHTLY:
                    list = "'NIGHTLY','BETA','RELEASE'";
                    break;
                case PublishState.BETA:
                    list = "'BETA','RELEASE'";
                    break;
                case PublishState.RELEASE:
                    list = "'RELEASE'";
                    break;
            }
            query = query.Replace("$LIST$", list);

            object[][] parameters = new object[][]{
                new object[]{"@id", id}
            };

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);
            if (resultset.Count == 0)
            {
                return null;
            }

            Resource resource = new Resource
            {
                Id = (int)resultset[0]["id"],
                Username = (string)resultset[0]["username"],
                Name = (string)resultset[0]["name"]
            };

            ResourceData resourceData = new ResourceData
            {
                ResourceId = resource.Id,
                ResourceVersion = (int)resultset[0]["version"],
                PublishState = (string)resultset[0]["publishstate"]
            };

            ResourceAndResourceData resourceAndResourceData = new ResourceAndResourceData
            {
                Resource = resource,
                ResourceData = resourceData
            };

            return resourceAndResourceData;
        }

        /// <summary>
        /// Creates a new resource data entry in the database
        /// </summary>
        /// <param name="version"></param>
        /// <param name="datafilename"></param>
        /// <param name="uploaddate"></param>
        public void CreateResourceData(int resourceid, int version, string datafilename, DateTime uploaddate)
        {
            logger.LogText(string.Format("Creating new resource data: resourceid={0}, version={1}, datafilename={2}, uploaddate={3}", resourceid, version, datafilename != null ? datafilename.Length.ToString() : "null", uploaddate), this, Logtype.Debug);
            string query = "insert into resourcesdata (resourceid, version, datafilename, uploaddate, publishstate) values (@resourceid, @version, @datafilename, @uploaddate, @publishstate)";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
                new object[]{"@datafilename", datafilename},
                new object[]{"@uploaddate", uploaddate},
                new object[]{"@publishstate", PublishState.NOTPUBLISHED.ToString()}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Created new resource data: resourceid={0}, version={1}, datafilename={2}, uploaddate={3}", resourceid, version, datafilename, uploaddate), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates a resource data entry in the database
        /// </summary>
        /// <param name="version"></param>
        /// <param name="datafilename"></param>
        /// <param name="uploaddate"></param>
        /// <param name="publishstate"></param>
        public void UpdateResourceData(int resourceid, int version, string datafilename, DateTime uploaddate, string publishstate)
        {
            logger.LogText(string.Format("Updating resource data: resourceid={0}, version={1}, datafilename={2}, uploaddate={3}", resourceid, version, datafilename, uploaddate), this, Logtype.Debug);
            string query = "update resourcesdata set datafilename=@datafilename, uploaddate=@uploaddate, publishstate=@publishstate where resourceid=@resourceid and version=@version";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@datafilename", datafilename},
                new object[]{"@uploaddate", uploaddate},
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
                new object[]{"@publishstate", publishstate},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updated resource data: resourceid={0}, version={1}, datafilename={2}, uploaddate={3}", resourceid, version, datafilename != null ? datafilename.Length.ToString() : "null", uploaddate), this, Logtype.Debug);
        }

        /// <summary>
        /// Updates a resource data entry in the database
        /// </summary>
        /// <param name="version"></param>
        /// <param name="datafilename"></param>
        /// <param name="uploaddate"></param>
        public void UpdateResourceData(int resourceid, int version, string datafilename)
        {
            logger.LogText(string.Format("Updating resource data: resourceid={0}, version={1}, datafilename={2}", resourceid, version, datafilename), this, Logtype.Debug);
            string query = "update resourcesdata set datafilename=@datafilename, uploaddate=@uploaddate where resourceid=@resourceid and version=@version";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@datafilename", datafilename},
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
                new object[]{"@uploaddate", DateTime.Now}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updated resource data: resourceid={0}, version={1}, datafilename={2}", resourceid, version, datafilename != null ? datafilename.Length.ToString() : "null"), this, Logtype.Debug);
        }

        /// Updates a resource data entry in the database
        /// </summary>
        /// <param name="resourceid"></param>
        /// <param name="resourceversion"></param>
        /// <param name="publishstate"></param>
        public void UpdateResourceData(int resourceid, int version, PublishState publishstate)
        {
            logger.LogText(string.Format("Updating resourcedata: resourceid={0}, version={1}, publishstate={2}", resourceid, version, publishstate.ToString()), this, Logtype.Debug);
            string query = "update resourcesdata set publishstate=@publishstate where resourceid=@resourceid and version=@version";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
                new object[]{"@publishstate", publishstate.ToString()}
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Updated resourcedata: resourceid={0}, version={1}, publishstate={2}", resourceid, version, publishstate.ToString()), this, Logtype.Debug);
        }

        /// <summary>
        /// Deletes a resource data entry in the database
        /// </summary>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <param name="uploaddate"></param>
        public void DeleteResourceData(int resourceid, int version)
        {
            logger.LogText(string.Format("Deleting resource data: resourceid={0}, version={1}", resourceid, version), this, Logtype.Debug);
            string query = "delete from resourcesdata where resourceid=@resourceid and version=@version";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
            };

            connection.ExecutePreparedStatement(query, parameters);

            logger.LogText(string.Format("Deleted resource data: resourceid={0}, version={1}", resourceid, version), this, Logtype.Debug);
        }

        /// <summary>
        /// Returns the dedicated resource data identified by its id
        /// Returns null, if the resource does not exist
        /// </summary>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <param name="uploaddate"></param>
        public ResourceData GetResourceData(int resourceid, int version)
        {
            string query = "select resourceid, version, datafilename, uploaddate, publishstate from resourcesdata where resourceid=@resourceid and version=@version";

            DatabaseConnection connection = GetConnection();

            object[][] parameters = new object[][]{
                new object[]{"@resourceid", resourceid},
                new object[]{"@version", version},
            };

            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);

            if (resultset.Count == 0)
            {
                return null;
            }

            ResourceData resourceData = new ResourceData
            {
                ResourceId = (int)resultset[0]["resourceid"],
                ResourceVersion = (int)resultset[0]["version"],
                DataFilename = (string)resultset[0]["datafilename"],
                UploadDate = (DateTime)resultset[0]["uploaddate"],
                PublishState = (string)resultset[0]["publishstate"]
            };

            return resourceData;
        }

        /// <summary>
        /// Returns a list of resource data from the database
        /// If resourceid is set, it only returns resources of that user
        /// </summary>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <param name="uploaddate"></param>
        public List<ResourceData> GetResourceDatas(int resourceid = -1)
        {
            string query;

            if (resourceid != -1)
            {
                query = "select resourceid, version, datafilename, uploaddate, publishstate from resourcesdata where resourceid=@resourceid";
            }
            else
            {
                query = "select resourceid, version, datafilename, uploaddate, publishstate from resourcesdata";
            }

            DatabaseConnection connection = GetConnection();

            object[][] parameters = null;

            if (resourceid != -1)
            {
                parameters = new object[][]{
                    new object[]{"@resourceid", resourceid}
                };
            }
            List<Dictionary<string, object>> resultset = connection.ExecutePreparedStatement(query, parameters);

            List<ResourceData> resourceDataList = new List<ResourceData>();

            foreach (Dictionary<string, object> entry in resultset)
            {
                ResourceData resourceData = new ResourceData
                {
                    ResourceId = (int)entry["resourceid"],
                    ResourceVersion = (int)entry["version"],
                    DataFilename = (string)entry["datafilename"],
                    UploadDate = (DateTime)entry["uploaddate"],
                    PublishState = (string)entry["publishstate"]
                };
                resourceDataList.Add(resourceData);
            }
            return resourceDataList;
        }

        #endregion
    }
}