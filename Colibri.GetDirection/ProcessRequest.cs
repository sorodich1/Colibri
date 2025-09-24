using Colibri.GetDirection.Helpers;
using System;
using System.Collections.Generic;

namespace Colibri.GetDirection
{
    /// <summary>
    /// Класс для обработки запросов генерации маршрутов и расчетов расстояний
    /// </summary>
    public class ProcessRequest
    {
        /// <summary>
        /// Генерирует промежуточные точки маршрута между стартовой и конечной точками
        /// </summary>
        /// <param name="start">Начальная точка маршрута</param>
        /// <param name="stop">Конечная точка маршрута</param>
        /// <param name="interval">Интервал между точками в метрах</param>
        /// <returns>Список точек маршрута, включая начальную и конечную точки </returns>
        public static List<Point> GenerateRoutePoints(Point start, Point stop, int interval)
        {
            // Расчет расстояния между точками в метрах (умножаем км на 1000)
            double distance = Math.Round(CalculateDistance(start, stop) * 1000);

            // Вычисление количества промежуточных точек
            int numberOfPoints = (int)(distance / interval);

            List<Point> points = [];

            // Генерация точек маршрута
            for (int i = 0; i <= numberOfPoints; i++)
            {
                // Вычисление доли пройденного пути (от 0 до 1)
                double fraction = (double)i / numberOfPoints;

                // Линейная интерполяция широты
                double lat = start.Latitude + (stop.Latitude - start.Latitude) * fraction;

                // Линейная интерполяция долготы
                double lon = start.Longitude + (stop.Longitude - start.Longitude) * fraction;

                points.Add(new Point
                {
                    Latitude = lat,
                    Longitude = lon,
                });
            }

            return points;
        }

        /// <summary>
        /// Вычисляет расстояние между двумя географическими точками по формуле гаверсинуса
        /// </summary>
        /// <param name="start">Начальная точка</param>
        /// <param name="stop">Конечная точка</param>
        /// <returns>Расстояние в километрах</returns>
        public static double CalculateDistance(Point start, Point stop)
        {
            // Радиус Земли в километрах
            const double R = 6371;

            // Преобразование градусов в радианы
            double lat1 = ToRadiance(start.Latitude);
            double lon1 = ToRadiance(start.Longitude);
            double lat2 = ToRadiance(stop.Latitude);
            double lon2 = ToRadiance(stop.Longitude);

            // Разницы координат
            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            // Формула гаверсинуса
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            // Угловое расстояние в радианах
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // Расстояние в километрах
            double distance = R * c;

            return distance;
        }

        /// <summary>
        /// Преобразует градусы в радианы
        /// </summary>
        /// <param name="radiance">Значение в градусах</param>
        /// <returns>Значение в радианах</returns>
        private static double ToRadiance(double radiance)
        {
            return radiance * (Math.PI / 180);
        }
    }
}
