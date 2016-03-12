using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeCollectionsRevampServer.Models.ObjectHolderModels
{
    public abstract class ObjectHolder
    {
        protected bool ColumnExists(NpgsqlDataReader reader, string columnName)
        {
            bool status = false;

            // From http://stackoverflow.com/a/1213409/1751481
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).ToLower() == columnName.ToLower())
                {
                    status = true;
                    break;
                }
            }

            return status;
        }
    }
}
