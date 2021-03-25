using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using CrypTool.Util.Logging;
using Org.BouncyCastle.Math;
using CrypTool.CertificateLibrary.Network;
using CrypTool.CertificateServer.Rules;

namespace CrypTool.CertificateServer
{
    /// <summary>
    /// Provides access to the certificate database
    /// </summary>
    public class CertificateDatabase
    {

        #region Constants

        private const int MAX_DB_CONNECTIONS = 11;

        #endregion


        #region Private members

        private MySqlConnection connection = null;
        
        private string dbpassword = null;
        
        private Object databaseLock = new Object();

        #endregion


        #region Constructors

        /// <summary>
        /// Creates a new CertificateDatabase object using localhost as hostname
        /// </summary>
        /// <param name="certificateDatabase"></param>
        /// <param name="password"></param>
        /// <param name="username"></param>
        /// <param name="port"></param>
        /// <param name="timeout">Connection timeout in seconds</param>
        public CertificateDatabase(string database,
                                   string password,
                                   string username,
                                   uint port,
                                   uint timeout)
            : this(database, "localhost", password, username, port, timeout)
        {
        }               

        /// <summary>
        /// Create a new CertificateDatabase object
        /// </summary>
        /// <param name="database"></param>
        /// <param name="host"></param>
        /// <param name="password"></param>
        /// <param name="username"></param>
        /// <param name="port"></param>
        /// <param name="timeout">Connection timeout in seconds</param>
        public CertificateDatabase(string database,
                                   string host,
                                   string password,
                                   string username,
                                   uint port,
                                   uint timeout)
        {
            Debug.Assert(database != null, "database can not be null!");
            Debug.Assert(host != null, "host can not be null!");
            Debug.Assert(password != null, "password can not be null!");
            Debug.Assert(username != null, "username can not be null!");
            
            this.DBName = database;
            this.Host = host;
            this.dbpassword = password;
            this.User = username;
            this.Port = port;
            this.Timeout = timeout;
        }

        #endregion


        #region Connection management

        /// <summary>
        /// Connect this object to the database
        /// </summary>
        /// <param name="timeout">timeout in seconds (default: 15)</param>
        /// <exception cref="DatabaseException"></exception>
        private void Connect()
        {
            if (IsConnected())
            {
                Log.Debug("DB Connect was called, but is already connected.");
                return;
            }

            try
            {
                MySqlConnectionStringBuilder myConnBuilder = new MySqlConnectionStringBuilder();
                myConnBuilder.Database = this.DBName;
                myConnBuilder.Server = this.Host;
                myConnBuilder.Password = this.dbpassword;
                myConnBuilder.UserID = this.User;
                myConnBuilder.Port = this.Port;
                myConnBuilder.ConnectionTimeout = this.Timeout;
                myConnBuilder.ConnectionProtocol = MySqlConnectionProtocol.Sockets;
                myConnBuilder.Pooling = true;
                myConnBuilder.MinimumPoolSize = 3;
                myConnBuilder.MaximumPoolSize = MAX_DB_CONNECTIONS;
                this.connection = new MySqlConnection(myConnBuilder.ConnectionString);
                this.connection.Open();

                if (this.ConnectionSuccesfullyEstablished != null)
                {
                    this.ConnectionSuccesfullyEstablished.Invoke(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                string msg = "Can not connect to database!";
                Log.Error(msg, ex);

                if (this.ConnectionBroken != null)
                {
                    this.ConnectionBroken.Invoke(this, new DatabaseErrorEventArgs(msg + ex.ToString()));
                }
                throw new DatabaseException(msg, ex);
            }
        }

        /// <summary>
        /// Closes the connection to the database
        /// </summary>
        private void Close()
        {
            if (this.connection != null)
            {
                this.connection.Close();
            }
        }

        /// <summary>
        /// Is there a connection to the database?
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (this.connection == null || this.connection.State == ConnectionState.Closed)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to ping the database.
        /// </summary>
        /// <returns>true if the pong was received from the database</returns>
        public bool CanPing()
        {
            try
            {
                Connect();
                return this.connection.Ping();
            }
            catch
            {
                return false;
            }
            finally
            {
                Close();
            }
        }

        #endregion


        #region Select commands

        /// <summary>
        /// Checks if a peer certificate or registration request for the given avatar and/or email address exist
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="avatar">The avatar to search for</param>
        /// <param name="email">The email address to search for</param>
        /// <returns>InvalidData object if duplicate entry, null otherwise</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public ProcessingError SelectAvatarEmailExist(BigInteger caSerial, string avatar, string email)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Could not check for duplicate entries! CA serialnumber is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM ((SELECT COUNT(*) AS avatarcount FROM peers WHERE avatar = @avatar AND caSerialnumber = @caSerial) AS avatartable, (SELECT COUNT(*) AS emailcount FROM peers WHERE email = @email AND caSerialnumber = @caSerial) AS emailtable)";
                    command.Parameters.AddWithValue("@avatar", avatar);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());

                    using (MySqlDataReader mysqlReader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!mysqlReader.HasRows)
                        {
                            return null;
                        }
                        if (!mysqlReader.Read())
                        {
                            throw new DatabaseException("Could not read MySQL data");
                        }
                        long avatarCount = mysqlReader.GetInt64(mysqlReader.GetOrdinal("avatarcount"));
                        long emailCount = mysqlReader.GetInt64(mysqlReader.GetOrdinal("emailcount"));

                        mysqlReader.Close();
                        if (avatarCount == 1)
                        {
                            return new ProcessingError(ErrorType.AvatarAlreadyExists);
                        }
                        if (emailCount == 1)
                        {
                            return new ProcessingError(ErrorType.EmailAlreadyExists);
                        }
                    }

                    command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM ((SELECT COUNT(*) AS avatarcount FROM registrationrequests WHERE avatar = @avatar AND caSerialnumber = @caSerial) AS avatartable, (SELECT COUNT(*) AS emailcount FROM registrationrequests WHERE email = @email AND caSerialnumber = @caSerial) AS emailtable)";
                    command.Parameters.AddWithValue("@avatar", avatar);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());

                    using (MySqlDataReader mysqlReader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!mysqlReader.HasRows)
                        {
                            return null;
                        }
                        if (!mysqlReader.Read())
                        {
                            string msg = "Could not read MySQL data";
                            Log.Debug(msg);
                            throw new DatabaseException(msg);
                        }
                        long avatarCount = mysqlReader.GetInt64(mysqlReader.GetOrdinal("avatarcount"));
                        long emailCount = mysqlReader.GetInt64(mysqlReader.GetOrdinal("emailcount"));

                        mysqlReader.Close();
                        if (avatarCount == 1)
                        {
                            return new ProcessingError(ErrorType.AvatarAlreadyExists);
                        }
                        if (emailCount == 1)
                        {
                            return new ProcessingError(ErrorType.EmailAlreadyExists);
                        }
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Could not check for duplicate entries!";
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Returns the peer entry for the given avatar or email address. Does not give the PKCS #12 or public certificate binary!
        /// <para>Returns null if no entry exists</para>
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="avatarOrEmail">The avatar or email address</param>
        /// <param name="isEmail">Set to true, if this is an email address</param>
        /// <returns>A PeerEntry object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public CertificateEntry SelectPeerEntry(BigInteger caSerial, string avatarOrEmail, bool isEmail)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not select peer entry! CA serialnumber is null!");
            }
            if (avatarOrEmail == null)
            {
                throw new ArgumentNullException("avatarOrEmail", "Can not select peer entry! avatarOrEmail is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = (isEmail)
                        ? "SELECT serialnumber, avatar, email, world, dateofissue, dateofexpire, pwdCode, datePasswordReset, programName, programVersion, optionalInfo, extensions FROM peers WHERE email = @avatarOrEmail AND caSerialnumber = @caSerial"
                        : "SELECT serialnumber, avatar, email, world, dateofissue, dateofexpire, pwdCode, datePasswordReset, programName, programVersion, optionalInfo, extensions FROM peers WHERE avatar = @avatarOrEmail AND caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@avatarOrEmail", avatarOrEmail);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    
                    using (MySqlDataReader mysqlReader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!mysqlReader.HasRows)
                        {
                            return null;
                        }
                        if (!mysqlReader.Read())
                        {
                            throw new DatabaseException("Could not read MySQL data");
                        }

                        CertificateEntry peerEntry = new CertificateEntry(
                            mysqlReader.GetUInt64(mysqlReader.GetOrdinal("serialnumber")),
                            mysqlReader.GetString(mysqlReader.GetOrdinal("avatar")),
                            mysqlReader.GetString(mysqlReader.GetOrdinal("email")),
                            mysqlReader.GetString(mysqlReader.GetOrdinal("world")),
                            mysqlReader.GetDateTime(mysqlReader.GetOrdinal("dateofissue")),
                            mysqlReader.GetDateTime(mysqlReader.GetOrdinal("dateofexpire")),
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("pwdCode"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("pwdCode")) : null, 
                            mysqlReader.GetDateTime(mysqlReader.GetOrdinal("datePasswordReset")),
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("programName"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("programName")) : null,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("programVersion"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("programVersion")) : null,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("optionalInfo"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("optionalInfo")) : null,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("extensions"))) ? Extension.Parse(mysqlReader.GetString(mysqlReader.GetOrdinal("extensions"))) : new Extension[0]
                            );
                        mysqlReader.Close();
                        return peerEntry;
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Can not select peer entry!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Returns the peer entry for the given password verification code. Does not give the PKCS #12 or public certificate binary!
        /// <para>Returns null if no entry exists</para>
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="code">The password verification code</param>
        /// <returns>A PeerEntry object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public CertificateEntry SelectPeerEntry(BigInteger caSerial, string code)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not select peer entry! CA serialnumber is null!");
            }
            if (code == null)
            {
                throw new ArgumentNullException("code", "Can not select peer entry! code is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT serialnumber, avatar, email, world, dateofissue, dateofexpire, datePasswordReset, programName, programVersion, optionalInfo, extensions FROM peers WHERE pwdCode = @code AND caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@code", code);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());

                    using (MySqlDataReader mysqlReader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!mysqlReader.HasRows)
                        {
                            return null;
                        }
                        if (!mysqlReader.Read())
                        {
                            throw new DatabaseException("Could not read MySQL data");
                        }

                        CertificateEntry peerEntry = new CertificateEntry(
                            mysqlReader.GetUInt64(mysqlReader.GetOrdinal("serialnumber")),
                            mysqlReader.GetString(mysqlReader.GetOrdinal("avatar")),
                            mysqlReader.GetString(mysqlReader.GetOrdinal("email")),
                            mysqlReader.GetString(mysqlReader.GetOrdinal("world")),
                            mysqlReader.GetDateTime(mysqlReader.GetOrdinal("dateofissue")),
                            mysqlReader.GetDateTime(mysqlReader.GetOrdinal("dateofexpire")),
                            code,
                            mysqlReader.GetDateTime(mysqlReader.GetOrdinal("datePasswordReset")),
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("programName"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("programName")) : null,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("programVersion"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("programVersion")) : null,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("optionalInfo"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("optionalInfo")) : null,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("extensions"))) ? Extension.Parse(mysqlReader.GetString(mysqlReader.GetOrdinal("extensions"))) : new Extension[0]
                            );

                        mysqlReader.Close();
                        return peerEntry;
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Can not select peer entry!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Returns the registration entry for the given avatar or email address.
        /// <para>Returns null if no entry exists</para>
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="code">The verification code</param>
        /// <returns>A RegistrationEntry object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public RegistrationEntry SelectRegistrationEntry(BigInteger caSerial, string code)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not select registration entry! CA serialnumber is null!");
            }
            if (code == null)
            {
                throw new ArgumentNullException("code", "Can not select registration entry! code is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT avatar, email, world, dateofrequest, verified, pwdHash, programName, programVersion, optionalInfo, authorized, extensions FROM registrationrequests WHERE verificationCode = @code AND caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@code", code);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());

                    using (MySqlDataReader mysqlReader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!mysqlReader.HasRows)
                        {
                            return null;
                        }
                        if (!mysqlReader.Read())
                        {
                            throw new DatabaseException("Could not read MySQL data");
                        }

                        RegistrationEntry regEntry = new RegistrationEntry(
                            mysqlReader.GetString(mysqlReader.GetOrdinal("avatar")),
                            mysqlReader.GetString(mysqlReader.GetOrdinal("email")),
                            mysqlReader.GetString(mysqlReader.GetOrdinal("world")),
                            mysqlReader.GetDateTime(mysqlReader.GetOrdinal("dateofrequest")),
                            code,
                            (mysqlReader.GetUInt16(mysqlReader.GetOrdinal("verified")) == 1) ? true : false,
                            mysqlReader.GetString(mysqlReader.GetOrdinal("pwdHash")),
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("programName"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("programName")) : null,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("programVersion"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("programVersion")) : null,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("optionalInfo"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("optionalInfo")) : null,
                            (mysqlReader.GetUInt16(mysqlReader.GetOrdinal("authorized")) == 1) ? true : false,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("extensions"))) ? Extension.Parse(mysqlReader.GetString(mysqlReader.GetOrdinal("extensions"))) : new Extension[0]
                            );

                        mysqlReader.Close();
                        return regEntry;
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Can not select registration entry!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Returns a list of all registration enties. Returns an empty list if no entry exists.
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <returns>A list of RegistrationEntry object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public List<RegistrationEntry> SelectRegistrationEntries(BigInteger caSerial)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not select registration entries! CA serialnumber is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "SELECT avatar, email, world, dateofrequest, verified, programName, programVersion, optionalInfo, authorized, extensions FROM registrationrequests WHERE caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());

                    using (MySqlDataReader mysqlReader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        List<RegistrationEntry> result = new List<RegistrationEntry>();
                        if (!mysqlReader.HasRows)
                        {
                            return result;
                        }

                        while (mysqlReader.Read())
                        {
                            RegistrationEntry entry = new RegistrationEntry(
                                mysqlReader.GetString(mysqlReader.GetOrdinal("avatar")),
                                mysqlReader.GetString(mysqlReader.GetOrdinal("email")),
                                mysqlReader.GetString(mysqlReader.GetOrdinal("world")),
                                mysqlReader.GetDateTime(mysqlReader.GetOrdinal("dateofrequest")),
                                null,
                                (mysqlReader.GetUInt16(mysqlReader.GetOrdinal("verified")) == 1) ? true : false,
                                null,
                                (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("programName"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("programName")) : null,
                                (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("programVersion"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("programVersion")) : null,
                                (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("optionalInfo"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("optionalInfo")) : null,
                                (mysqlReader.GetUInt16(mysqlReader.GetOrdinal("authorized")) == 1) ? true : false,
                                (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("extensions"))) ? Extension.Parse(mysqlReader.GetString(mysqlReader.GetOrdinal("extensions"))) : new Extension[0]
                                );
                            result.Add(entry);
                        }
                        mysqlReader.Close();
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Can not select registration entries!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Returns the registration entry for the given avatar or email address.
        /// <para>Returns null if no entry exists</para>
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="code">The verification code</param>
        /// <returns>A RegistrationEntry object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public RegistrationEntry SelectRegistrationEntry(BigInteger caSerial, string avatarOrEmail, bool isEmail)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not select registration entry! CA serialnumber is null!");
            }
            if (avatarOrEmail == null)
            {
                throw new ArgumentNullException("avatarOrEmail", "Can not select registration entry! avatarOrEmail is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = (isEmail)
                        ? "SELECT avatar, email, world, dateofrequest, verificationCode, verified, pwdHash, programName, programVersion, optionalInfo, authorized, extensions FROM registrationrequests WHERE email = @avatarOrEmail AND caSerialnumber = @caSerial"
                        : "SELECT avatar, email, world, dateofrequest, verificationCode, verified, pwdHash, programName, programVersion, optionalInfo, authorized, extensions FROM registrationrequests WHERE avatar = @avatarOrEmail AND caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@avatarOrEmail", avatarOrEmail);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());

                    using (MySqlDataReader mysqlReader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!mysqlReader.HasRows)
                        {
                            return null;
                        }
                        if (!mysqlReader.Read())
                        {
                            throw new DatabaseException("Could not read MySQL data");
                        }

                        RegistrationEntry regEntry = new RegistrationEntry(
                            mysqlReader.GetString(mysqlReader.GetOrdinal("avatar")),
                            mysqlReader.GetString(mysqlReader.GetOrdinal("email")),
                            mysqlReader.GetString(mysqlReader.GetOrdinal("world")),
                            mysqlReader.GetDateTime(mysqlReader.GetOrdinal("dateofrequest")),
                            mysqlReader.GetString(mysqlReader.GetOrdinal("verificationCode")),
                            (mysqlReader.GetUInt16(mysqlReader.GetOrdinal("verified")) == 1) ? true : false,
                            mysqlReader.GetString(mysqlReader.GetOrdinal("pwdHash")),
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("programName"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("programName")) : null,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("programVersion"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("programVersion")) : null,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("optionalInfo"))) ? mysqlReader.GetString(mysqlReader.GetOrdinal("optionalInfo")) : null,
                            (mysqlReader.GetUInt16(mysqlReader.GetOrdinal("authorized")) == 1) ? true : false,
                            (!mysqlReader.IsDBNull(mysqlReader.GetOrdinal("extensions"))) ? Extension.Parse(mysqlReader.GetString(mysqlReader.GetOrdinal("extensions"))) : new Extension[0]
                            );

                        mysqlReader.Close();
                        return regEntry;
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Can not select registration entry!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Selects a PKCS #12 store or public certificate by its avatar or email.
        /// <para>returns null if there is none stored in db</para>
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="avatarOrEmail">The avatar or email address</param>
        /// <param name="isEmail">Set to true, if this is an email address</param>
        /// <returns>byte array containing the public certificate/PKCS #12 or null if no entry found</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public byte[] SelectPeerCertificate(bool getPkcs12, BigInteger caSerial, string avatarOrEmail, bool isEmail)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not select peer certificate! CA serialnumber is null!");
            }
            if (avatarOrEmail == null)
            {
                throw new ArgumentNullException("avatarOrEmail", "Can not select peer certificate! avatarOrEmail is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    string column = (getPkcs12) ? "pkcs12" : "certificate";
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = (isEmail)
                        ? String.Format("SELECT {0} FROM peers WHERE email = @avatarOrEmail AND caSerialnumber = @caSerial", column)
                        : String.Format("SELECT {0} FROM peers WHERE avatar = @avatarOrEmail AND caSerialnumber = @caSerial", column);
                    command.Parameters.AddWithValue("@avatarOrEmail", avatarOrEmail);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());

                    MySqlDataReader myReader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                    if (!myReader.HasRows)
                    {
                        return null;
                    }
                    byte[] peerCertificate = null;

                    MemoryStream memoryStream = new MemoryStream();
                    BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
                    try
                    {
                        int bufferSize = 100;
                        long retval;
                        long startIndex;
                        byte[] outbyte = new byte[bufferSize];

                        while (myReader.Read())
                        {
                            startIndex = 0;
                            retval = myReader.GetBytes(0, startIndex, outbyte, 0, bufferSize);

                            while (retval == bufferSize)
                            {
                                binaryWriter.Write(outbyte);
                                binaryWriter.Flush();

                                startIndex += bufferSize;
                                retval = myReader.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                            }

                            binaryWriter.Write(outbyte, 0, (int)retval);
                            binaryWriter.Flush();
                        }
                        peerCertificate = memoryStream.GetBuffer();
                    }
                    finally
                    {
                        if (binaryWriter != null)
                        {
                            binaryWriter.Close();
                            binaryWriter.Dispose();
                        }
                        if (memoryStream != null)
                        {
                            memoryStream.Close();
                            memoryStream.Dispose();
                        }
                        if (myReader != null)
                        {
                            myReader.Close();
                            myReader.Dispose();
                        }
                    }
                    return peerCertificate;
                }
                catch (Exception ex)
                {
                    string msg = "Can not select peer certificate!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Selects a CA or TLS certificate in PKCS #12 format
        /// <para>returns null if there is none stored in db</para>
        /// <para>returns null if there is no connection or no serial given</para>
        /// </summary>
        /// <param name="getCA">if true the CA certificate will be returned, the TLS certificate otherwise</param>
        /// <param name="serial">Serialnumber of the corresponding CA certificate</param>
        /// <returns>PKCS #12 store as byte array</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public byte[] SelectCaOrTlsPkcs12(bool getCA, BigInteger serial)
        {
            if (serial == null)
            {
                throw new ArgumentNullException("serial", "Can not select CA/TLS certificate! No serial given!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    string columnName = (getCA) ? "ca_pkcs12" : "tls_pkcs12";
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format("SELECT {0} FROM ca WHERE serial = @serial", columnName);
                    command.Parameters.AddWithValue("@serial", serial);

                    MySqlDataReader myReader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                    if (!myReader.HasRows)
                    {
                        return null;
                    }

                    byte[] certificate = null;
                    int bufferSize = 100;
                    long retval;
                    long startIndex;
                    byte[] outbyte = new byte[bufferSize];

                    MemoryStream memoryStream = new MemoryStream();
                    BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
                    try
                    {
                        while (myReader.Read())
                        {
                            startIndex = 0;
                            retval = myReader.GetBytes(0, startIndex, outbyte, 0, bufferSize);

                            while (retval == bufferSize)
                            {
                                binaryWriter.Write(outbyte);
                                binaryWriter.Flush();

                                startIndex += bufferSize;
                                retval = myReader.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                            }

                            binaryWriter.Write(outbyte, 0, (int)retval);
                            binaryWriter.Flush();
                            certificate = memoryStream.GetBuffer();
                        }
                    }
                    finally
                    {
                        if (binaryWriter != null)
                        {
                            binaryWriter.Close();
                            binaryWriter.Dispose();
                        }
                        if (memoryStream != null)
                        {
                            memoryStream.Close();
                            memoryStream.Dispose();
                        }
                        if (myReader != null)
                        {
                            myReader.Close();
                            myReader.Dispose();
                        }
                    }
                    return certificate;
                }
                catch (Exception ex)
                {
                    string msg = "Can not select CA/TLS certificate!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Returns a list of all undelivered email enties. Returns an empty list if no entry exists.
        /// Type: EmailVerificationCode = 0, PasswordResetCode = 1, RegistrationRequest = 2, RegistrationPerformed = 3, AuthorizationGranted = 4
        /// </summary>
        /// <returns>A list of RegistrationEntry object</returns>
        /// <exception cref="DatabaseException"></exception>
        public List<UndeliveredEmailEntry> SelectUndeliveredEmailEntries(BigInteger caSerial)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not select undelivered email entries! CA serialnumber is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "SELECT emailIndex, type, email, dateOfAttempt FROM undeliveredemails WHERE caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());

                    using (MySqlDataReader mysqlReader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        List<UndeliveredEmailEntry> result = new List<UndeliveredEmailEntry>();
                        if (!mysqlReader.HasRows)
                        {
                            return result;
                        }

                        while (mysqlReader.Read())
                        {
                            UndeliveredEmailEntry entry = new UndeliveredEmailEntry(
                                mysqlReader.GetUInt32(mysqlReader.GetOrdinal("emailIndex")),
                                mysqlReader.GetByte(mysqlReader.GetOrdinal("type")),
                                mysqlReader.GetString(mysqlReader.GetOrdinal("email")),
                                mysqlReader.GetDateTime(mysqlReader.GetOrdinal("dateOfAttempt"))
                                );
                            result.Add(entry);
                        }
                        mysqlReader.Close();
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Can not select registration entries!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Checks whether a specific password reset verification code already exists for the given CA serialnumber.
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="code">code to search for</param>
        /// <returns>true if the code already exists</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public bool SelectPasswordResetCodeExist(BigInteger caSerial, string code)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Could not select password reset verification code count! CA serialnumber is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT COUNT(*) FROM peers WHERE pwdCode = @code AND caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@code", code);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    long count = (long)command.ExecuteScalar();
                    return (count == 1) ? true : false;
                }
                catch (Exception ex)
                {
                    string msg = "Could not select password reset verification code count!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Checks whether a specific serialnumber already exists for the given CA serialnumber.
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="serialnumber">Serialnumber to search for</param>
        /// <returns>true if the serialnumber already exists</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public bool SelectSerialnumberExist(BigInteger caSerial, ulong serialnumber)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not select serialnumber count! CA serialnumber is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT COUNT(*) FROM peers WHERE serialnumber = @serialnumber AND caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@serialnumber", serialnumber);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    long count = (long)command.ExecuteScalar();
                    return (count == 1) ? true : false;
                }
                catch (Exception ex)
                {
                    string msg = "Could not select serialnumber count!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Reads the number of peer certificates signed by the CA certificate with the given serialnumber.
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public long SelectPeerCertificateCount(BigInteger caSerial)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Could not select peer certificate count! CA serialnumber is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT COUNT(*) FROM peers WHERE caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    return (long)command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    string msg = "Could not select peer certificate count!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Reads the number of registration requests for the CA certificate with the given serialnumber.
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public long SelectRegistrationRequestCount(BigInteger caSerial)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Could not select registration request count! CA serialnumber is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT COUNT(*) FROM registrationrequests WHERE caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    return (long)command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    string msg = "Could not select registration request count!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Returns the date and time of the last registered peer certificate.
        /// <para>Returns DateTime.MinValue if no certificate was registered with this CA certificate yet.</para>
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate that was used to sign the certificate</param>
        /// <returns>Date and time of last register or DateTime.MinValue if no certificate was registered yet</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public DateTime SelectDateOfLastRegister(BigInteger caSerial)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Could not select date of last register! CA serialnumber is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT MAX(dateofissue) FROM peers WHERE caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    object obj = command.ExecuteScalar();
                    if (obj == null || obj.ToString() == String.Empty)
                    {
                        return DateTime.MinValue;
                    }
                    return DateTime.Parse(obj.ToString());
                }
                catch (Exception ex)
                {
                    string msg = "Could not select date of last register!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Returns the date and time of the last registration request.
        /// <para>Returns DateTime.MinValue if no registration was requested for this CA certificate yet.</para>
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <returns>Date and time of last registration request or DateTime.MinValue if no request is open</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public DateTime SelectDateOfLastRegistrationRequest(BigInteger caSerial)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Could not select date of last registration request! CA serialnumber is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT MAX(dateofissue) FROM registrationrequests WHERE caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    object obj = command.ExecuteScalar();
                    if (obj == null || obj.ToString() == String.Empty)
                    {
                        return DateTime.MinValue;
                    }
                    return DateTime.Parse(obj.ToString());
                }
                catch (Exception ex)
                {
                    string msg = "Could not select date of last registration request!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        #endregion


        #region Insert commands

        /// <summary>
        /// Stores a peer certificate into the database
        /// </summary>
        /// <param name="serialnumber"></param>
        /// <param name="email"></param>
        /// <param name="avatar"></param>
        /// <param name="world"></param>
        /// <param name="dateOfIssue"></param>
        /// <param name="dateOfExpire"></param>
        /// <param name="caSerial"></param>
        /// <param name="certificate"></param>
        /// <param name="pkcs12"></param>
        /// <param name="emailCode"></param>
        /// <param name="pwdCode"></param>
        /// <param name="isAuthorized"></param>
        /// <param name="datePasswordReset"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void StorePeerCertificate(BigInteger serialnumber, string email, string avatar, string world, DateTime dateOfIssue, DateTime dateOfExpire, BigInteger caSerial, byte[] certificate, byte[] pkcs12, string pwdCode, DateTime datePasswordReset, string programName, string programVersion, string optionalInfo, Extension[] extensions)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email", "Can not store peer certificate! No email given!");
            }
            if (avatar == null)
            {
                throw new ArgumentNullException("avatar", "Can not store peer certificate! No avatar given!");
            }
            if (world == null)
            {
                throw new ArgumentNullException("world", "Can not store peer certificate! No world given!");
            }
            if (dateOfIssue== null)
            {
                throw new ArgumentNullException("dateOfIssue", "Can not store peer certificate! No date of issue given!");
            }
            if (dateOfExpire == null)
            {
                throw new ArgumentNullException("dateOfExpire", "Can not store peer certificate! No date of expire given!");
            }
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not store peer certificate! No CA serial given!");
            }
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate", "Can not store peer certificate! No certificate given!");
            }
            if (pkcs12 == null)
            {
                throw new ArgumentNullException("pkcs12", "Can not store peer certificate! No PKCS12 given!");
            }
            if (datePasswordReset == null)
            {
                throw new ArgumentNullException("datePasswordReset", "Can not store peer certificate! No date of password reset given!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO peers (serialnumber, email, avatar, world, dateofissue, dateofexpire, caSerialnumber, certificate, pkcs12, pwdCode, datePasswordReset, programName, programVersion, optionalInfo, extensions) VALUES (@serialnumber, @email, @avatar, @world, @dateOfIssue, @dateOfExpire, @caSerial, @certificate, @pkcs12, @pwdCode, @datePasswordReset, @programName, @programVersion, @optionalInfo, @extensions)";
                    command.Parameters.AddWithValue("@serialnumber", serialnumber);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@avatar", avatar);
                    command.Parameters.AddWithValue("@world", world);
                    command.Parameters.AddWithValue("@dateOfIssue", dateOfIssue);
                    command.Parameters.AddWithValue("@dateOfExpire", dateOfExpire);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    command.Parameters.AddWithValue("@certificate", certificate);
                    command.Parameters.AddWithValue("@pkcs12", pkcs12);
                    command.Parameters.AddWithValue("@pwdCode", pwdCode);
                    command.Parameters.AddWithValue("@datePasswordReset", datePasswordReset);
                    command.Parameters.AddWithValue("@programName", programName);
                    command.Parameters.AddWithValue("@programVersion", programVersion);
                    command.Parameters.AddWithValue("@optionalInfo", optionalInfo);
                    command.Parameters.AddWithValue("@extensions", (extensions.Length == 0) ? null : Extension.Convert(extensions));
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Can not store peer certificate!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Stores a peer certificate registration request into the database
        /// </summary>
        /// <param name="regEntry"></param>
        /// <param name="caSerial"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void StoreRegistrationRequest(RegistrationEntry regEntry, BigInteger caSerial)
        {
            if (regEntry == null)
            {
                throw new ArgumentNullException("regEntry", "Can not store registration request! Registration entry is null!");
            }
            if (regEntry.Email == null)
            {
                throw new ArgumentNullException("email", "Can not store registration request! No email given!");
            }
            if (regEntry.Avatar == null)
            {
                throw new ArgumentNullException("avatar", "Can not store registration request! No avatar given!");
            }
            if (regEntry.World == null)
            {
                throw new ArgumentNullException("world", "Can not store registration request! No world given!");
            }
            if (regEntry.DateOfRequest == null)
            {
                throw new ArgumentNullException("dateOfRequest", "Can not store registration request! No date of request given!");
            }
            if (regEntry.HashedPassword == null)
            {
                throw new ArgumentNullException("hashedPassword", "Can not store registration request! No hashedPassword given!");
            }
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not store registration request! No CA serial given!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO registrationrequests (email, avatar, world, dateofrequest, caSerialnumber, verificationCode, pwdHash, programName, programVersion, optionalInfo, authorized, extensions) VALUES (@email, @avatar, @world, @dateOfRequest, @caSerial, @code, @pwdHash, @programName, @programVersion, @optionalInfo, @isAuthorized, @extensions)";
                    command.Parameters.AddWithValue("@email", regEntry.Email);
                    command.Parameters.AddWithValue("@avatar", regEntry.Avatar);
                    command.Parameters.AddWithValue("@world", regEntry.World);
                    command.Parameters.AddWithValue("@dateOfRequest", regEntry.DateOfRequest);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    command.Parameters.AddWithValue("@code", regEntry.EmailCode);
                    command.Parameters.AddWithValue("@pwdHash", regEntry.HashedPassword);
                    command.Parameters.AddWithValue("@programName", regEntry.ProgramName);
                    command.Parameters.AddWithValue("@programVersion", regEntry.ProgramVersion);
                    command.Parameters.AddWithValue("@optionalInfo", regEntry.OptionalInfo);
                    command.Parameters.AddWithValue("@isAuthorized", (regEntry.IsAuthorized) ? 1 : 0);
                    command.Parameters.AddWithValue("@extensions", (regEntry.Extensions.Length == 0) ? null : Extension.Convert(regEntry.Extensions));
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Can not store registration request!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Stores a CA certificate into the database
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="distinguishedName"></param>
        /// <param name="dateOfIssue"></param>
        /// <param name="dateOfExpire"></param>
        /// <param name="caPkcs12"></param>
        /// <param name="tlsPkcs12"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void StoreCACertificate(BigInteger serial, string distinguishedName, DateTime dateOfIssue, DateTime dateOfExpire, byte[] caPkcs12, byte[] tlsPkcs12)
        {
            if (serial == null)
            {
                throw new ArgumentNullException("serial", "Can not store CA certificate! No serial given!");
            }
            if (distinguishedName == null)
            {
                throw new ArgumentNullException("distinguishedName", "Can not store CA certificate! No distinguished name given!");
            }
            if (dateOfIssue == null)
            {
                throw new ArgumentNullException("dateOfIssue", "Can not store CA certificate! No date of issue given!");
            }
            if (dateOfExpire == null)
            {
                throw new ArgumentNullException("dateOfExpire", "Can not store CA certificate! No date of expire given!");
            }
            if (caPkcs12 == null)
            {
                throw new ArgumentNullException("caPkcs12", "Can not store CA certificate! No CA certificate given!");
            }
            if (tlsPkcs12 == null)
            {
                throw new ArgumentNullException("tlsPkcs12", "Can not store CA certificate! No TLS certificate given!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO ca (serial, dn, dateofissue, dateofexpire, ca_pkcs12, tls_pkcs12) VALUES (@serial, @dn, @dateOfIssue, @dateOfExpire, @caPkcs12, @tlsPkcs12)";
                    command.Parameters.AddWithValue("@serial", serial);
                    command.Parameters.AddWithValue("@dn", distinguishedName);
                    command.Parameters.AddWithValue("@dateOfIssue", dateOfIssue);
                    command.Parameters.AddWithValue("@dateOfExpire", dateOfExpire);
                    command.Parameters.AddWithValue("@caPkcs12", caPkcs12);
                    command.Parameters.AddWithValue("@tlsPkcs12", tlsPkcs12);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Can not store CA certificate!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Stores an undelivered email entry in the database.
        /// Type: EmailVerificationCode = 0, PasswordResetCode = 1, RegistrationRequest = 2, RegistrationPerformed = 3, AuthorizationGranted = 4
        /// </summary>
        /// <param name="caSerial">The CA serialnumber</param>
        /// <param name="type">The type of email that could not be sent</param>
        /// <param name="email">The email address of the recipient</param>
        /// <param name="dateOfAttempt">The date and time the email sending attempt failed.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void StoreUndeliveredEmailEntry(BigInteger caSerial, byte type, string email, DateTime dateOfAttempt)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email", "Can not store undelivered email entry! No email given!");
            }
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not store undelivered email entry! No CA serial given!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO undeliveredemails (type, caSerialnumber, email, dateOfAttempt) VALUES (@type, @caSerial, @email, @dateOfAttempt)";
                    command.Parameters.AddWithValue("@type", type);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@dateOfAttempt", dateOfAttempt);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Can not store undelivered email entry!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        #endregion


        #region Update commands

        /// <summary>
        /// Updates the password reset code for the given avatar or email address.
        /// The method also depends on the CA certificate's serialnumber
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="avatarOrEmail">The avatar or email address</param>
        /// <param name="isEmail">Set to true, if this is an email address</param>
        /// <param name="code">The new code</param>
        /// <param name="datePasswordReset">Date of the password reset request</param>
        /// <param name="certificate">The public certificate</param>
        /// <param name="pkcs12">the PKCS #12 store</param>
        /// <exception cref="ArgumentException">If one of certificate and pkcs12 is null, but the other is not null</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void UpdatePasswordResetData(BigInteger caSerial, string avatarOrEmail, bool isEmail, string code, DateTime datePasswordReset, byte[] certificate = null, byte[] pkcs12 = null)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not update password reset code! CA serialnumber is null!");
            }
            if (avatarOrEmail == null)
            {
                throw new ArgumentNullException("avatarOrEmail", "Can not update password reset code! avatarOrEmail is null!");
            }
            if (datePasswordReset == null)
            {
                throw new ArgumentNullException("datePasswordReset", "Can not update password reset code! datePasswordReset is null!");
            }
            if ((certificate == null && pkcs12 != null) ^ (certificate != null && pkcs12 == null))
            {
                throw new ArgumentException("Can not update password reset data! Certificate and pkcs12 must be both null or both not null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    string column = (isEmail) ? "email" : "avatar";

                    if (certificate == null && pkcs12 == null)
                    {
                        command.CommandText = String.Format("UPDATE peers SET pwdCode = @code, datePasswordReset = @datePasswordReset WHERE {0} = @avatarOrEmail AND caSerialnumber = @caSerial", column);
                    }
                    else
                    {
                        command.CommandText = String.Format("UPDATE peers SET pwdCode = @code, datePasswordReset = @datePasswordReset, certificate = @certificate, pkcs12 = @pkcs12 WHERE {0} = @avatarOrEmail AND caSerialnumber = @caSerial", column);
                        command.Parameters.AddWithValue("@certificate", certificate);
                        command.Parameters.AddWithValue("@pkcs12", pkcs12);
                    }
                    command.Parameters.AddWithValue("@avatarOrEmail", avatarOrEmail);
                    command.Parameters.AddWithValue("@code", code);
                    command.Parameters.AddWithValue("@datePasswordReset", datePasswordReset);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Can not update password reset code!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Updates the email verification status for the given avatar or email address.
        /// The method also depends on the CA certificate's serialnumber
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="avatarOrEmail">The avatar or email address</param>
        /// <param name="isEmail">Set to true, if this is an email address</param>
        /// <param name="isVerified">true if the email address has been verified</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void UpdateEmailVerificationStatus(BigInteger caSerial, string avatarOrEmail, bool isEmail, bool isVerified)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Could not update email verification code! CA serialnumber is null!");
            }
            if (avatarOrEmail == null)
            {
                throw new ArgumentNullException("avatarOrEmail", "Could not update email verification code! avatarOrEmail is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = (isEmail)
                        ? "UPDATE registrationrequests SET verified = @isVerified WHERE email = @avatarOrEmail AND caSerialnumber = @caSerial"
                        : "UPDATE registrationrequests SET verified = @isVerified WHERE avatar = @avatarOrEmail AND caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@avatarOrEmail", avatarOrEmail);
                    command.Parameters.AddWithValue("@isVerified", (isVerified) ? 1 : 0);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Could not update email verification code!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Updates the authorization status for the given registration entries.
        /// The method also depends on the CA certificate's serialnumber
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="regEntries">List of RegistrationEntry objects</param>
        /// <param name="isAuthorized">The new authorized status</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void UpdateAuthorizationStatus(BigInteger caSerial, List<RegistrationEntry> regEntries, bool isAuthorized)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Could not update authorization status! CA serialnumber is null!");
            }
            if (regEntries == null)
            {
                throw new ArgumentNullException("avatarNames", "Could not update authorization status! avatarNames is null!");
            }
            if (regEntries.Count == 0)
            {
                return;
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    foreach (RegistrationEntry regEntry in regEntries)
                    {
                        MySqlCommand command = connection.CreateCommand();
                        command.CommandText = "UPDATE registrationrequests SET authorized = @authorized WHERE caSerialnumber = @caSerial AND avatar = @avatar ";
                        command.Parameters.AddWithValue("@authorized", (isAuthorized) ? 1 : 0);
                        command.Parameters.AddWithValue("@avatar", regEntry.Avatar);
                        command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Could not update authorization status!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Updates a peer pkcs12.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="caSerial"></param>
        /// <param name="pkcs12"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void UpdatePeerPkcs12(BigInteger caSerial, string email, byte[] pkcs12)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email", "Can not update peer certificate! No email given!");
            }
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not update peer certificate! No CA serial given!");
            }
            if (pkcs12 == null)
            {
                throw new ArgumentNullException("pkcs12", "Can not update peer certificate! No PKCS12 given!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "UPDATE peers SET pkcs12 = @pkcs12 WHERE email = @email AND caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    command.Parameters.AddWithValue("@pkcs12", pkcs12);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Can not update peer certificate!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Removes all password reset older then the number of days.
        /// The method also depends on the CA certificate's serialnumber.
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="days">The number of days</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">days is zero</exception>
        /// <exception cref="DatabaseException"></exception>
        public int DeleteOldPasswordReset(BigInteger caSerial, int days)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not delete old password reset requests! CA serialnumber is null!");
            }
            if (days <= 0)
            {
                throw new ArgumentOutOfRangeException("Can not delete old password reset request! Days value is zero or negative!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format("UPDATE peers SET pwdCode = @pwdCode, datePasswordReset = @datePasswordReset WHERE caSerialnumber = @caSerial AND datePasswordReset > @datePasswordReset AND DATE_SUB(CURDATE(),INTERVAL {0} DAY) >= datePasswordReset", days);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    command.Parameters.AddWithValue("@pwdCode", null);
                    command.Parameters.AddWithValue("@datePasswordReset", DateTime.MinValue);
                    return command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Can not delete old password reset request!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        #endregion


        #region Delete commands

        /// <summary>
        /// Deletes the certificate with the given avatar or email address.
        /// The method also depends on the CA certificate's serialnumber
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="avatarOrEmail">The avatar or email address</param>
        /// <param name="isEmail">Set to true, if this is an email address</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void DeleteCertificateEntry(BigInteger caSerial, string avatarOrEmail, bool isEmail)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Could not delete certificate entry! CA serialnumber is null!");
            }
            if (avatarOrEmail == null)
            {
                throw new ArgumentNullException("avatarOrEmail", "Could not delete certificate entry! avatarOrEmail is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = (isEmail)
                        ? "DELETE FROM peers WHERE email = @avatarOrEmail AND caSerialnumber = @caSerial"
                        : "DELETE FROM peers WHERE avatar = @avatarOrEmail AND caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@avatarOrEmail", avatarOrEmail);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Could not delete certificate entry!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Deletes the registration request with the given avatar or email address.
        /// The method also depends on the CA certificate's serialnumber
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="code">The verification code</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void DeleteRegistrationEntry(BigInteger caSerial, string code)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not delete registration entry! CA serialnumber is null!");
            }
            if (code == null)
            {
                throw new ArgumentNullException("code", "Can not delete registration entry! code is null!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM registrationrequests WHERE verificationCode = @code AND caSerialnumber = @caSerial";
                    command.Parameters.AddWithValue("@code", code);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Can not delete registration entry!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Deletes all registration requests with avatars of the given list.
        /// The method also depends on the CA certificate's serialnumber
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="regEntries">The list of registration entries</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void DeleteRegistrationEntries(BigInteger caSerial, List<RegistrationEntry> regEntries)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not delete registration entry! CA serialnumber is null!");
            }
            if (regEntries == null)
            {
                throw new ArgumentNullException("regEntries", "Can not delete registration entry! regEntries is null!");
            }
            if (regEntries.Count == 0)
            {
                return;
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    foreach (RegistrationEntry entry in regEntries)
                    {
                        MySqlCommand command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM registrationrequests WHERE avatar = @avatar AND caSerialnumber = @caSerial";
                        command.Parameters.AddWithValue("@avatar", entry.Avatar);
                        command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Can not delete registration entry!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Deletes all registration requests older then the specified number of days.
        /// The method also depends on the CA certificate's serialnumber
        /// </summary>
        /// <param name="caSerial">Serialnumber of the CA certificate</param>
        /// <param name="days">The number of days</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">days is zero</exception>
        /// <exception cref="DatabaseException"></exception>
        public int DeleteOldRegistrationEntries(BigInteger caSerial, int days)
        {
            if (caSerial == null)
            {
                throw new ArgumentNullException("caSerial", "Can not delete old registration entry! CA serialnumber is null!");
            }
            if (days <= 0)
            {
                throw new ArgumentOutOfRangeException("Can not delete old registration entry! Days value is zero or negative!");
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format("DELETE FROM registrationrequests WHERE caSerialnumber = @caSerial AND authorized = 1 AND DATE_SUB(CURDATE(),INTERVAL {0} DAY) >= dateofrequest", days);
                    command.Parameters.AddWithValue("@caSerial", caSerial.ToString());
                    return command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Can not delete old registration entry!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Deletes the undelivered email entries.
        /// </summary>
        /// <param name="indices">A list of indices that should be deleted</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void DeleteUndeliveredEmailEntries(List<uint> indices)
        {
            if (indices == null)
            {
                throw new ArgumentNullException("indices", "Could not delete undelivered email entries. Indices list can not be null.");
            }
            if (indices.Count == 0)
            {
                return;
            }

            lock (databaseLock)
            {
                Connect();
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("(");
                    for(int i = indices.Count - 1; i >= 0; i--)
                    {
                        sb.Append(indices[i]);
                        if (i > 0)
                        {
                            sb.Append(",");
                        }
                    }
                    sb.Append(")");
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format("DELETE FROM undeliveredemails WHERE emailIndex in {0}", sb.ToString());
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = "Can not delete undelivered email entry!";
                    Log.Error(msg, ex);
                    throw new DatabaseException(msg, ex);
                }
                finally
                {
                    Close();
                }
            }
        }

        #endregion


        #region Properties

        /// <summary>
        /// Database Host
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Database Port
        /// </summary>
        public uint Port { get; private set; }

        /// <summary>
        /// Database User
        /// </summary>
        public string User { get; private set; }

        /// <summary>
        /// Name of the used database
        /// </summary>
        public string DBName { get; private set; }

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        public uint Timeout { get; private set; }

        #endregion


        #region Events

        /// <summary>
        /// Invoked when the connection to the database is broken
        /// </summary>
        public event EventHandler<DatabaseErrorEventArgs> ConnectionBroken;

        /// <summary>
        /// Invoked when a database connection has been succsessfully established
        /// </summary>
        public event EventHandler<EventArgs> ConnectionSuccesfullyEstablished;

        #endregion

    }

    public class UndeliveredEmailEntry
    {
        public UndeliveredEmailEntry(uint index, byte type, string email, DateTime dateOfAttempt)
        {
            this.Index = index;
            this.Type = type;
            this.Email = email;
            this.DateOfAttempt = dateOfAttempt;
        }

        public uint Index;
        public byte Type;
        public string Email;
        public DateTime DateOfAttempt;
    }

    public class CertificateEntry : Entry
    {
        public CertificateEntry(ulong serialnumber, string avatar, string email, string world, DateTime dateOfIssue, DateTime dateOfExpire, string passwordCode, DateTime dateofPasswordReset, string programName, string programVersion, string optionalInfo, Extension[] extensions)
        {
            this.Serialnumber = serialnumber;
            this.Avatar = avatar;
            this.Email = email;
            this.World = world;
            this.DateOfIssue = dateOfIssue;
            this.DateOfExpire = dateOfExpire;
            this.PasswordCode = passwordCode;
            this.DateOfPasswordReset = dateofPasswordReset;
            this.ProgramName = programName;
            this.ProgramVersion = programVersion;
            this.OptionalInfo = optionalInfo;
            this.Extensions = extensions;
        }

        public ulong Serialnumber;
        public DateTime DateOfIssue;
        public DateTime DateOfExpire;
        public string PasswordCode;
        public DateTime DateOfPasswordReset;
    }

    public class RegistrationEntry : Entry
    {
        public RegistrationEntry(string avatar, string email, string world, DateTime dateOfRequest, string emailCode, bool isVerified, string hashedPassword, string programName, string programVersion, string optionalInfo, bool isAuthorized, Extension[] extensions)
        {
            this.Avatar = avatar;
            this.Email = email;
            this.World = world;
            this.DateOfRequest = dateOfRequest;
            this.EmailCode = emailCode;
            this.IsVerified= isVerified;
            this.HashedPassword = hashedPassword;
            this.ProgramName = programName;
            this.ProgramVersion = programVersion;
            this.OptionalInfo = optionalInfo;
            this.IsAuthorized = isAuthorized;
            this.Extensions = extensions;
        }

        public DateTime DateOfRequest;
        public string EmailCode;
        public bool IsVerified;
        public string HashedPassword;
        public bool IsAuthorized;
    }

    public class Entry
    {
        public string Avatar;
        public string Email;
        public string World;
        public string ProgramName;
        public string ProgramVersion;
        public string OptionalInfo;
        public Extension[] Extensions;
    }
}