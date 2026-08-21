using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Clio.Helpers;

/// <summary>
/// Вспомогательный класс для цветовых тем
/// </summary>
public static class ThemeHelper
{
    /// <summary>
    /// Генерирует линейный градиент программным способом с применением дизеринга.
    /// </summary>
    /// <param name="colorStart">
    /// Начальный цвет.
    /// </param>
    /// <param name="colorEnd">
    /// Конечный цвет.
    /// </param>
    /// <param name="alpha">
    /// Значение альфа-канала.
    /// </param>
    /// <returns>
    /// Новый экземпляр <see cref="BitmapSource"/>, содержащий линейный градиент.
    /// </returns>
    public static BitmapSource GenerateGradientWithDithering(Color colorStart, Color colorEnd, double alpha)
    {
        int width = 512;
        int height = 512;
        int bytesPerPixel = 4; // BGRA
        int stride = width * bytesPerPixel;
        byte[] pixelData = new byte[height * stride];

        Random rand = new Random();

        // Переводим альфу из диапазона 0.0..1.0 в байт 0..255
        byte alphaByte = (byte)Math.Clamp(alpha * 255, 0, 255);

        // Извлекаем стартовые каналы (Верх)
        double r1 = colorStart.R;
        double g1 = colorStart.G;
        double b1 = colorStart.B;

        // Извлекаем финальные каналы (Низ)
        double r2 = colorEnd.R;
        double g2 = colorEnd.G;
        double b2 = colorEnd.B;

        for (int y = 0; y < height; y++)
        {
            double t = (double)y / (height - 1);

            // Плавно смешиваем цвета по вертикали
            double r = r1 + (r2 - r1) * t;
            double g = g1 + (g2 - g1) * t;
            double b = b1 + (b2 - b1) * t;

            for (int x = 0; x < width; x++)
            {
                // Наш фирменный дизеринг против бандинга 8-битного цвета
                double noise = (rand.NextDouble() - 0.5) * 1.1;

                int pixelOffset = (y * stride) + (x * bytesPerPixel);

                // Записываем байты с защитой от вылета за границы 0..255
                pixelData[pixelOffset] = (byte)Math.Clamp(b + noise, 0, 255); // B
                pixelData[pixelOffset + 1] = (byte)Math.Clamp(g + noise, 0, 255); // G
                pixelData[pixelOffset + 2] = (byte)Math.Clamp(r + noise, 0, 255); // R
                pixelData[pixelOffset + 3] = alphaByte;                          // A (Теперь динамическая!)
            }
        }

        // Собираем картинку
        BitmapSource bitmap = BitmapSource.Create(
            width, height, 96, 96,
            PixelFormats.Bgra32, null,
            pixelData, stride);

        bitmap.Freeze(); // Замораживаем для максимальной скорости отрисовки в WPF
        return bitmap;
    }
}
