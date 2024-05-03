using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SHA256_Test_NTT
{
    internal class Program
    {
        private static readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private const int BYTES_IN_MB = 1048576;
        private const int PRECENT_MULTI = 100;

        public static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                tokenSource.Cancel();
                eventArgs.Cancel = true;
            };

            List<string> hashes = new List<string>();
            WriteArgs();

            hashes = await CreateHashes(args[0], Convert.ToInt32(args[1]), tokenSource.Token);

            if (!tokenSource.IsCancellationRequested)
            {
                Console.WriteLine("\n\nХэш-сумма SHA256:");

                foreach (string hash in hashes)
                {
                    Console.WriteLine(hash);
                }

                Console.WriteLine($"Количество сегментов: {hashes.Count}");

                Console.WriteLine("\nГотово!");
                Console.WriteLine("Нажмите Enter для выхода.");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("\nХэширование файла было отменено пользователем!");
                Console.WriteLine("Нажмите Enter для выхода.");
                Console.ReadLine();
            }

            void WriteArgs()
            {
                Console.WriteLine("Тестовое задание - посегментное хэширование файлов по алгоритму SHA-256.");

                for (int i = 0; i < args.Length; i++)
                {
                    switch (i)
                    {
                        case 0: Console.WriteLine($"Путь к файлу: '{args[i]}'"); break;
                        case 1: Console.WriteLine($"Сегмент для хеширования (в байтах): {args[i]}"); break;
                        default: return;
                    }
                }

                Console.WriteLine($"Для остановки выполнения хэширования нажмите Ctrl + C");
                Console.WriteLine();
                return;
            }
        }

        async static Task<List<string>> CreateHashes(string path, int bufferSize, CancellationToken token)
        {
            List<string> result = new List<string>();

            try
            {
                byte[] buffer = new byte[bufferSize]; //1KB = 1024; 1MB = 1048576;
                SHA256 hash = SHA256.Create();

                using (FileStream m_File = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, buffer.Length, true))
                {
                    float totalRead = 0;

                    Console.WriteLine($"Длина потока: {string.Format("{0:F2}", Convert.ToDouble(m_File.Length) / BYTES_IN_MB)} МБ");
                    Console.Write($"Начало хэширования...");

                    while (!token.IsCancellationRequested)
                    {
                        var lastRead = m_File.ReadAsync(buffer, 0, buffer.Length).Result;

                        if (lastRead > 0)
                        {
                            totalRead += lastRead;
                            result.Add(BitConverter.ToString(hash.ComputeHash(buffer)).Replace("-", ""));
                            double progress = totalRead / m_File.Length * PRECENT_MULTI;
                            Console.Write("\rХэшировано {0:F2}% данных", progress);
                        }
                        else
                            break;
                    }

                    await Task.Yield();
                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nОШИБКА: {e.Message}");
                Console.WriteLine($"Трассировка: {e.StackTrace}");
                Console.ReadKey();
                return null;
            }
        }        
    }
}

