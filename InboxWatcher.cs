using System.Text.Json;
using OrderFlow.Models;
using OrderFlow.Services;

namespace OrderFlow.Services
{
    public class InboxWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly OrderPipeline _pipeline;
        private readonly string _inboxPath;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(2);

        public InboxWatcher(string path, OrderPipeline pipeline)
        {
            _inboxPath = path;
            _pipeline = pipeline;

            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "processed"));
            Directory.CreateDirectory(Path.Combine(path, "failed"));

            _watcher = new FileSystemWatcher(path, "*.json");
            _watcher.Created += OnCreated;
            _watcher.EnableRaisingEvents = true;
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            await _semaphore.WaitAsync();

            try
            {
                Console.WriteLine($"[Watcher] New file detected: {e.Name}");

                await Task.Delay(300); // чек щоб файл звільнився

                string json = await ReadFileWithRetry(e.FullPath);
                var orders = JsonSerializer.Deserialize<List<Order>>(json);

                if (orders == null)
                    throw new Exception("Invalid JSON");

                foreach (var order in orders)
                {
                    await _pipeline.ProcessOrderAsync(order);
                }

                string processedPath = Path.Combine(_inboxPath, "processed", Path.GetFileName(e.FullPath));
                File.Move(e.FullPath, processedPath, true);

                Console.WriteLine($"[Watcher] Moved to processed: {processedPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Watcher] ERROR: {ex.Message}");

                string failedPath = Path.Combine(_inboxPath, "failed", Path.GetFileName(e.FullPath));
                File.Move(e.FullPath, failedPath, true);

                File.WriteAllText(failedPath + ".error.txt", ex.ToString());
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<string> ReadFileWithRetry(string path)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    return await File.ReadAllTextAsync(path);
                }
                catch (IOException)
                {
                    await Task.Delay(200);
                }
            }

            throw new Exception("File is locked too long");
        }

        public void Dispose()
        {
            _watcher.Dispose();
            _semaphore.Dispose();
        }
    }
}