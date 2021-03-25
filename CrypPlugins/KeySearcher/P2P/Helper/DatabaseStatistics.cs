using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using CrypTool.PluginBase;
using KeySearcher.P2P.Presentation;

namespace KeySearcher.P2P.Helper
{
    /// <summary>
    /// Helper for evaluation of the thesis. Will be removed after finishing the the thesis.
    /// TODO REMOVE
    /// </summary>
    static class DatabaseStatistics
    {
        public static void PushToDatabase(StatusContainer status, long bruteForceTime, string identifier, KeySearcherSettings settings, KeySearcher keySearcher)
        {
            if (string.IsNullOrEmpty(settings.EvaluationHost))
                return;

            var connectionString = "Data Source=" + settings.EvaluationHost + ";";
            connectionString += "User ID=" + settings.EvaluationUser + ";";
            connectionString += "Password=" + settings.EvaluationPassword + ";";
            connectionString += "Initial Catalog=" + settings.EvaluationDatabase;

            var sqlConnection = new SqlConnection();

            try
            {
                sqlConnection.ConnectionString = connectionString;
                sqlConnection.Open();

                // You can get the server version 
                // SQLConnection.ServerVersion
            }
            catch (Exception ex)
            {
                sqlConnection.Dispose();
                keySearcher.GuiLogMessage("DB Error: " + ex.Message, NotificationLevel.Error);
                return;
            }

            var globalProgress = status.GlobalProgress.ToString(CultureInfo.CreateSpecificCulture("en-US"));
            var dhtTimeInMilliseconds = status.DhtOverheadInReadableTime.TotalMilliseconds.ToString(CultureInfo.CreateSpecificCulture("en-US"));
            var sqlStatement = string.Format("INSERT INTO [statistics] ([host],[date],[localFinishedChunks],[currentChunk],[globalProgress],[totalAmountOfParticipants],[totalDhtRequests],[requestsPerNode],[retrieveRequests],[removeRequests],[storeRequests],[dhtTimeInMilliseconds],[dhtOverheadInPercent],[storedBytes],[retrievedBytes],[totalBytes],[sentBytesByLinkManager],[receivedBytesByLinkManager],[totalBytesByLinkManager],[bruteForceTimeInMilliseconds],[identifier],[processID]) " 
                + "VALUES ('{0}', GetDate(), {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, '{11}', {12}, {13}, {14}, {15}, {16}, {17}, {18}, '{19}', {20});",
                Environment.MachineName, status.LocalFinishedChunks, status.CurrentChunk, globalProgress, status.TotalAmountOfParticipants, status.TotalDhtRequests, status.RequestsPerNode, status.RetrieveRequests, status.RemoveRequests, status.StoreRequests, dhtTimeInMilliseconds, status.DhtOverheadInPercent, status.StoredBytes, status.RetrievedBytes, status.TotalBytes, status.SentBytesByLinkManager, status.ReceivedBytesByLinkManager, status.TotalBytesByLinkManager, bruteForceTime, identifier, Process.GetCurrentProcess().Id);

            try
            {
                var command = new SqlCommand(sqlStatement, sqlConnection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                keySearcher.GuiLogMessage("DB Error: " + ex.Message, NotificationLevel.Error);
            }
            finally
            {
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
        }

    }
}
