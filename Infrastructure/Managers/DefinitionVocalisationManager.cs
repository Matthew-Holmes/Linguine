using Microsoft.EntityFrameworkCore;
using Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataClasses;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure
{
    public class DefinitionVocalisationManager : DataManagerBase
    {
        public DefinitionVocalisationManager(LinguineDbContextFactory dbf) : base(dbf)
        {
        }

        public async Task CleanupMissingFilesAsync()
        {
            using var context = _dbf.CreateDbContext();

            var allVocalised = await context.VocalisedDefinitionFiles.ToListAsync();

            foreach (var record in allVocalised)
            {
                var fullPath = Path.Combine(ConfigManager.Config.AudioStoreDirectory, record.FileName);

                if (!File.Exists(fullPath))
                {
                    context.VocalisedDefinitionFiles.Remove(record);
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task<VocalisedDefinitionFile> AddVocalisationAsync(byte[] audioData,
                                               DictionaryDefinition def,
                                               Voice voice,
                                               string fileName)
        {
            var fullPath = Path.Combine(ConfigManager.Config.AudioStoreDirectory, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            await File.WriteAllBytesAsync(fullPath, audioData);

            VocalisedDefinitionFile toAdd = new VocalisedDefinitionFile
            {
                DictionaryDefinitionKey = def.DatabasePrimaryKey,
                Voice = voice,
                FileName = fileName,
            };

            return toAdd;
        }

        public async Task<List<VocalisedDefinitionFile>> GetAudioFilesForDefinitionAsync(int definitionId)
        {
            using var context = _dbf.CreateDbContext();

            return await context.VocalisedDefinitionFiles
                .Where(v => v.DictionaryDefinitionKey == definitionId)
                .ToListAsync();
        }

        public List<VocalisedDefinitionFile> GetAudioFilesForDefinition(int definitionId)
        {
            using var context = _dbf.CreateDbContext();

            return context.VocalisedDefinitionFiles
                .Where(v => v.DictionaryDefinitionKey == definitionId)
                .ToList();
        }

        public bool HasAnyFiles(DictionaryDefinition def, LinguineDbContext context)
        {
            return context.VocalisedDefinitionFiles
                          .Where(v => v.DictionaryDefinitionKey == def.ID)
                          .Any();
        }

        public bool HasAnyFilesSpecificVoice(DictionaryDefinition def, Voice voice, LinguineDbContext context)
        {
            return context.VocalisedDefinitionFiles
                .Where(v => v.DictionaryDefinitionKey == def.ID)
                .Where(v => v.Voice == voice)
                .Any();
        }   

        public async Task<bool> RemoveVocalisationAsync(int primaryKey)
        {
            using var context = _dbf.CreateDbContext();

            var record = await context.VocalisedDefinitionFiles.FindAsync(primaryKey);
            if (record == null)
                return false;

            var fullPath = Path.Combine(ConfigManager.Config.AudioStoreDirectory, record.FileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (IOException ex)
                {
                    Log.Error($"Error deleting file: {ex.Message}");
                }
            }

            context.VocalisedDefinitionFiles.Remove(record);
            await context.SaveChangesAsync();

            return true;
        }
    }
}
