using Microsoft.EntityFrameworkCore;
using Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataClasses;
using Serilog;

namespace Infrastructure
{
    class DefinitionVocalisationManager : ManagerBase
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

        public async Task AddVocalisationAsync(byte[] audioData, VocalisedDefinitionFile toAdd)
        {
            var fullPath = Path.Combine(ConfigManager.Config.AudioStoreDirectory, toAdd.FileName);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            using var context = _dbf.CreateDbContext();

            context.VocalisedDefinitionFiles.Add(toAdd);

            await context.SaveChangesAsync();

            await File.WriteAllBytesAsync(fullPath, audioData);
        }

        public async Task<List<VocalisedDefinitionFile>> GetAudioFilesForDefinitionAsync(int definitionId)
        {
            using var context = _dbf.CreateDbContext();

            return await context.VocalisedDefinitionFiles
                .Where(v => v.DictionaryDefinitionKey == definitionId)
                .ToListAsync();
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
