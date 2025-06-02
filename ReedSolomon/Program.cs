using System;
using System.Linq;

namespace ReedSolomon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ShowPrimeExample("ЯАяа feofoefo 43434 *$*#$(#$)");
        }

        // Работаем над бинарным полем
        public static void ShowBinaryExample(string textMessage)
        {
            var field = new BinaryField(0x11D);
            var message = textMessage.Select(c => (int)c).ToArray();
            var generator = 0x02;
            var msglen = message.Length;
            var ecclen = 10;

            if (msglen + ecclen > field.Size)
                throw new ArgumentException("Сообщение слишком длинное для заданного поля");

            var rs = new ReedSolomon<int>(field, generator, msglen, ecclen);

            Console.WriteLine($"Original message: {textMessage}");
            Console.WriteLine($"Array message: [{string.Join(", ", message)}]");

            // Кодируем сообщение
            var codeword = rs.Encode(message);
            Console.WriteLine($"Encoded codeword: [{string.Join(", ", codeword)}]");

            var random = new Random();
            double probability = (ecclen / 2.0) / (msglen + ecclen);
            int perturbed = 0;

            for (int i = 0; i < codeword.Length; i++)
            {
                if (random.NextDouble() < probability)
                {
                    codeword[i] = field.Add(codeword[i], random.Next(1, field.Size));
                    perturbed++;
                }

                if (perturbed >= ecclen / 2) break;
            }

            Console.WriteLine($"Number of values perturbed: {perturbed}");
            Console.WriteLine($"Perturbed codeword: [{string.Join(", ", codeword)}]");

            // Пытаемся декодировать сообщение
            try
            {
                var decoded = rs.Decode(codeword);
                if (decoded != null)
                {
                    var decodedMessage = new string(decoded.Select(x => (char)x).ToArray());
                    Console.WriteLine($"Decoded message: {decodedMessage}");
                }
                else
                {
                    Console.WriteLine("Decoding failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decoding error: {ex.Message}");
            }

            Console.WriteLine();
        }

        // Работаем над конечным полем
        static void ShowPrimeExample(string textMessage)
        {
            var field = new PrimeField(1231);
            var message = textMessage.Select(c => (int)c).ToArray();
            var generator = 3;
            var msglen = message.Length;
            var ecclen = 16;
            var rs = new ReedSolomon<int>(field, generator, msglen, ecclen);

            Console.WriteLine($"Исходное сообщение: {textMessage}");
            Console.WriteLine($"Массив сообщения: [{string.Join(", ", message)}]");

            // Кодируем сообщение
            var codeword = rs.Encode(message);
            Console.WriteLine($"Закодированное слово: [{string.Join(", ", codeword)}]");

            var random = new Random();
            double probability = 0.3; // Исправленная вероятность (в оригинале было 300)
            int perturbed = 0;

            for (int i = 0; i < codeword.Length; i++)
            {
                if (random.NextDouble() < probability)
                {
                    codeword[i] = field.Add(codeword[i], random.Next(1, field.Modulus));
                    perturbed++;
                }

                if (perturbed >= ecclen / 2) break;
            }

            Console.WriteLine($"Количество измененных значений: {perturbed}");
            Console.WriteLine($"Измененное кодовое слово: [{string.Join(", ", codeword)}]");

            // Пытаемся декодировать сообщение
            try
            {
                var decoded = rs.Decode(codeword);
                if (decoded != null)
                {
                    var decodedMessage = new string(decoded.Select(x => (char)x).ToArray());
                    Console.WriteLine($"Декодированное сообщение: {decodedMessage}");
                }
                else
                {
                    Console.WriteLine("Декодирование не удалось");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка декодирования: {ex.Message}");
            }

            Console.WriteLine();
        }
    }
}