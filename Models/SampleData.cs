using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Design;
using System.Data.Entity.Infrastructure;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace ArtFight.Models
{
    public class SampleData : DontDropDbJustCreateTablesIfModelChanged<Context>
    {
        protected override void Seed(Context context)
        {
            new List<Client>
            {
                new Client {username = "admin", email = "admin@artfight.tk", password="admin", role="admin"},
                new Client {username = "moder", email = "moder@artfight.tk", password="moder", role="moder"},
                new Client {username = "user", email = "user@artfight.tk", password="user", role="user"}
            }.ForEach(a => context.Clients.Add(a));

            new List<Competition>
            {
                new Competition { title="Anime art", begin = DateTime.Now, end = DateTime.Now.Add(TimeSpan.FromHours(5)), description = "blablabla", status = 0, owner_username="moder" },
                new Competition { title="Movie art", begin = DateTime.Now.Add(TimeSpan.FromHours(7)), end = DateTime.Now.Add(TimeSpan.FromHours(8)), status = 0, owner_username="moder" }
            }.ForEach(a => context.Competitions.Add(a));
        }
    }



    // Original code by Joachim Lykke Andersen; http://devtalk.dk/2011/02/16/Workaround+EF+Code+First+On+AppHabour.aspx

    public class DontDropDbJustCreateTablesIfModelChanged<T>
                        : IDatabaseInitializer<T> where T : DbContext
    {
        private EdmMetadata _edmMetaData;

        public void InitializeDatabase(T context)
        {
            ObjectContext objectContext =
                    ((IObjectContextAdapter)context).ObjectContext;

            string modelHash = GetModelHash(objectContext);

            if (CompatibleWithModel(modelHash, context, objectContext))
                return;

            DeleteExistingTables(objectContext);
            CreateTables(objectContext);

            SaveModelHashToDatabase(context, modelHash, objectContext);
            Seed(context);
        }

        protected virtual void Seed(T context) { }

        private void SaveModelHashToDatabase(T context, string modelHash,
                                                ObjectContext objectContext)
        {
            if (_edmMetaData != null) objectContext.Detach(_edmMetaData);

            _edmMetaData = new EdmMetadata();
            context.Set<EdmMetadata>().Add(_edmMetaData);

            _edmMetaData.ModelHash = modelHash;
            context.SaveChanges();
        }

        private void CreateTables(ObjectContext objectContext)
        {
            string dataBaseCreateScript =
                objectContext.CreateDatabaseScript();
            objectContext.ExecuteStoreCommand(dataBaseCreateScript);
        }

        private void DeleteExistingTables(ObjectContext objectContext)
        {
            objectContext.ExecuteStoreCommand(Dropallconstraintsscript);
            objectContext.ExecuteStoreCommand(Deletealltablesscript);
        }

        private string GetModelHash(ObjectContext context)
        {
            var csdlXmlString = GetCsdlXmlString(context).ToString();
            return ComputeSha256Hash(csdlXmlString);
        }

        private bool CompatibleWithModel(string modelHash, DbContext context,
                                            ObjectContext objectContext)
        {
            var isEdmMetaDataInStore =
                objectContext.ExecuteStoreQuery<int>(LookupEdmMetaDataTable)
                .FirstOrDefault();

            if (isEdmMetaDataInStore == 1)
            {
                _edmMetaData = context.Set<EdmMetadata>().FirstOrDefault();
                if (_edmMetaData != null)
                {
                    return modelHash == _edmMetaData.ModelHash;
                }
            }
            return false;
        }

        private string GetCsdlXmlString(ObjectContext context)
        {
            if (context != null)
            {
                var entityContainerList = context.MetadataWorkspace
                    .GetItems<EntityContainer>(DataSpace.SSpace);

                if (entityContainerList != null)
                {
                    var entityContainer = entityContainerList.FirstOrDefault();
                    var generator =
                        new EntityModelSchemaGenerator(entityContainer);
                    var stringBuilder = new StringBuilder();
                    var xmlWRiter = XmlWriter.Create(stringBuilder);
                    generator.GenerateMetadata();
                    generator.WriteModelSchema(xmlWRiter);
                    xmlWRiter.Flush();
                    return stringBuilder.ToString();
                }
            }
            return string.Empty;
        }

        private static string ComputeSha256Hash(string input)
        {
            byte[] buffer = new SHA256Managed()
                .ComputeHash(Encoding.ASCII.GetBytes(input));

            var builder = new StringBuilder(buffer.Length * 2);
            foreach (byte num in buffer)
            {
                builder.Append(num.ToString("X2",
                    CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        private const string Dropallconstraintsscript =
            @"select  
            'ALTER TABLE ' + so.table_name + ' DROP CONSTRAINT ' 
            + so.constraint_name  
            from INFORMATION_SCHEMA.TABLE_CONSTRAINTS so";

        private const string Deletealltablesscript =
            @"declare @cmd varchar(4000)
            declare cmds cursor for 
            Select
                'drop table [' + Table_Name + ']'
            From
                INFORMATION_SCHEMA.TABLES
 
            open cmds
            while 1=1
            begin
                fetch cmds into @cmd
                if @@fetch_status != 0 break
                print @cmd
                exec(@cmd)
            end
            close cmds
            deallocate cmds";

        private const string LookupEdmMetaDataTable =
            @"Select COUNT(*) 
            FROM INFORMATION_SCHEMA.TABLES T 
            Where T.TABLE_NAME = 'EdmMetaData'";
    }
}