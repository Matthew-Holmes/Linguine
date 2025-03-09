using Infrastructure;
using Linguine.Tabs;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.SocialInfo;

namespace Linguine
{
    public partial class MainModel
    {
        private ConcurrentDictionary<string, CancellationTokenSource> _bulkCancellations = new();
        
        internal record ProcessingJobInfo(String TextName, decimal CurrentPct, bool IsProcessing);

        internal ProcessingJobInfo GetProcessingInfo(String name, bool isProcessing, LinguineDbContext context)
        {
            TextualMedia? tm = TextualMediaManager.GetByName(name, context);

            if (tm is null)
            {
                throw new Exception($"unexpected textual media name: {name}");
            }

            int sofar = StatementManager.IndexOffEndOfLastStatement(tm, context);
            decimal pct = 100.0m * ((decimal)(sofar) / (decimal)(tm.Text.Length));

            return new ProcessingJobInfo(name, pct, isProcessing);
        }

        internal List<ProcessingJobInfo> GetProcessingJobs()
        {
            List<ProcessingJobInfo> ret = new();

            using LinguineDbContext context = LinguineFactory.CreateDbContext();

            foreach (KeyValuePair<string, CancellationTokenSource> kvp in _bulkCancellations)
            {
                bool isProcessing = !kvp.Value.IsCancellationRequested;

                ret.Add(GetProcessingInfo(kvp.Key, isProcessing, context));
            }
            return ret;
        }

        internal async Task StartBulkProcessing(string textName, Action<decimal>? progressCallback = null)
        {
            using var context = LinguineFactory.CreateDbContext();
            TextualMedia? tm = TextualMediaManager?.GetByName(textName, context) ?? null;

            if (tm is null)
            {
                throw new Exception("Missing textual media manager");
            }

            var newCts = new CancellationTokenSource();

            var oldCts = _bulkCancellations.AddOrUpdate(
                textName,
                 _               => newCts, // if key does not exist, add new token
                (_, existingCts) =>
                {
                    if (!existingCts.Token.IsCancellationRequested)
                    {
                        return existingCts; // already processing, do nothing, will short circuit next
                    }

                    existingCts.Dispose(); // dispose of old token before replacing
                    return newCts;
                });

            if (oldCts != newCts)
            {
                // short circuit, since don't need to do anything!
                return;
            }

            var cts = newCts.Token;
            var stopwatch = new Stopwatch();

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    stopwatch.Restart();
                    decimal percentageComplete = await ProcessNextChunk(tm, cts);
                    stopwatch.Stop();

                    double stepTime_ms = stopwatch.Elapsed.TotalMilliseconds;
                    double stepTime_s = stepTime_ms / 1_000.0;

                    progressCallback?.Invoke(percentageComplete);

                    Log.Information("Done processing step for {textName}", textName);
                    Log.Information("Step took {stepTime_s}s", stepTime_s);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during bulk processing for {textName}", textName);
            }
            finally
            {
                _bulkCancellations.TryRemove(textName, out _);
                newCts.Dispose();
            }
        }

        internal void StopBulkProcessing(string textName)
        {
            if (_bulkCancellations.TryRemove(textName, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                Log.Information("Stopped bulk processing for {textName}", textName);
            }
            else
            {
                Log.Warning("Stop bulk processing requested, but no text found: {textName}", textName);
            }
        }

    }
}
