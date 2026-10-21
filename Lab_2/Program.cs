using System;
using System.Globalization;
using System.Text;

internal class Program
{
    private const int MaxSteps = 1_000_000;

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("Лабораторная работа № 2");
        Console.WriteLine("Отделение корней нелинейных уравнений");
        Console.WriteLine("Вариант 17\n");

        Console.WriteLine(
            "1. sqrt(x + 1) - 2*cos(pi*x/2) = 0");

        Console.WriteLine(
            "2. 2*x^3 - 3*x^2 - 12*x + 8 = 0");

        Console.WriteLine(
            "3. Выполнить оба уравнения с параметрами по умолчанию");

        Console.Write("\nВыбор [3]: ");
        string choice = (Console.ReadLine() ?? "").Trim();

        try
        {
            switch (choice)
            {
                case "":
                case "3":
                    SeparateRoots(
                        "Первое уравнение",
                        Function1,
                        -1.0, 3.0, 0.1);

                    SeparateRoots(
                        "Второе уравнение",
                        Function2,
                        -4.0, 4.0, 1.0);
                    break;

                case "1":
                    ReadAndRun(
                        "Первое уравнение",
                        Function1,
                        -1.0, 3.0, 0.1,
                        true);
                    break;

                case "2":
                    ReadAndRun(
                        "Второе уравнение",
                        Function2,
                        -4.0, 4.0, 1.0,
                        false);
                    break;

                default:
                    Console.WriteLine("Нет такого пункта меню.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nОшибка: " + ex.Message);
        }
    }

    private static double Function1(double x)
    {
        if (x < -1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(x),
                "Для первого уравнения необходимо x >= -1.");
        }

        // При x = -1 значение функции точно равно нулю.
        // Эта проверка исключает погрешность вычисления cos(-pi/2).
        if (x == -1.0)
        {
            return 0.0;
        }

        return Math.Sqrt(x + 1.0)
             - 2.0 * Math.Cos(Math.PI * x / 2.0);
    }

    private static double Function2(double x)
    {
        return 2.0 * x * x * x
             - 3.0 * x * x
             - 12.0 * x
             + 8.0;
    }

    private static void ReadAndRun(
        string title,
        Func<double, double> function,
        double defaultStart,
        double defaultEnd,
        double defaultStep,
        bool restrictDomain)
    {
        Console.WriteLine(
            "\nНажмите Enter, чтобы оставить значение по умолчанию.");

        double start = ReadNumber("Xнач", defaultStart);
        double end = ReadNumber("Xкон", defaultEnd);
        double step = ReadNumber("Шаг", defaultStep);

        if (restrictDomain && start < -1.0)
        {
            throw new ArgumentException(
                "Начальная точка первого уравнения должна быть >= -1.");
        }

        SeparateRoots(title, function, start, end, step);
    }

    private static double ReadNumber(
        string caption,
        double defaultValue)
    {
        Console.Write($"{caption} [{Format(defaultValue)}]: ");
        string text = (Console.ReadLine() ?? "").Trim();

        if (text.Length == 0)
        {
            return defaultValue;
        }

        // Допускаются и точка, и запятая.
        text = text.Replace(',', '.');

        double value;

        if (!double.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value)
            || !IsFinite(value))
        {
            throw new ArgumentException(
                "Необходимо ввести конечное действительное число.");
        }

        return value;
    }

    private static void SeparateRoots(
        string title,
        Func<double, double> function,
        double start,
        double end,
        double step)
    {
        ValidateParameters(start, end, step);

        double span = end - start;
        double countValue = Math.Ceiling(span / step);

        if (!IsFinite(countValue) || countValue > MaxSteps)
        {
            throw new ArgumentException(
                "Слишком много шагов. Увеличьте шаг " +
                "или уменьшите отрезок.");
        }

        int count = Math.Max(1, (int)countValue);

        Console.WriteLine("\n" + title);
        Console.WriteLine(
            $"Xнач = {Format(start)}; " +
            $"Xкон = {Format(end)}; " +
            $"h = {Format(step)}");

        Console.WriteLine(
            "\n{0,14} {1,18}   {2}",
            "x", "f(x)", "Комментарий");

        double previousX = start;
        double previousY = Calculate(function, previousX);

        int exactRoots = 0;
        int intervals = 0;

        string firstComment = "----";

        if (previousY == 0.0)
        {
            firstComment = "Точный корень x = " + Format(previousX);
            exactRoots++;
        }

        PrintRow(previousX, previousY, firstComment);

        for (int i = 1; i <= count; i++)
        {
            /* Последний узел всегда совпадает с правой границей.
             Если шаг не делит длину отрезка нацело,
             последний подотрезок получается короче шага. */
            double currentX = i == count
                ? end
                : Math.Min(end, start + i * step);

            if (currentX <= previousX)
            {
                throw new InvalidOperationException(
                    "Шаг слишком мал для выбранных чисел.");
            }

            double currentY = Calculate(function, currentX);
            string comment = "----";

            if (currentY == 0.0)
            {
                // Узловой корень выводится только один раз.
                comment = "Точный корень x = " + Format(currentX);
                exactRoots++;
            }
            else if (
                (previousY < 0.0 && currentY > 0.0) ||
                (previousY > 0.0 && currentY < 0.0))
            {
                // Сравнение знаков вместо умножения
                // защищает от переполнения произведения.
                comment =
                    "Корень на отрезке [" +
                    Format(previousX) + "; " +
                    Format(currentX) + "]";

                intervals++;
            }

            PrintRow(currentX, currentY, comment);

            previousX = currentX;
            previousY = currentY;

            if (currentX >= end)
            {
                break;
            }
        }

        Console.WriteLine("\nСтоп");
        Console.WriteLine(
            "Точных узловых корней: " + exactRoots);

        Console.WriteLine(
            "Отрезков смены знака: " + intervals);

        if (exactRoots == 0 && intervals == 0)
        {
            Console.WriteLine(
                "При выбранных параметрах корни не обнаружены.");
        }

        Console.WriteLine(
            "Смена знака доказывает существование корня " +
            "для непрерывной функции.");

        Console.WriteLine(
            "Единственность корня на отрезке необходимо " +
            "проверять отдельно.");
    }

    private static void ValidateParameters(
        double start,
        double end,
        double step)
    {
        if (!IsFinite(start) ||
            !IsFinite(end) ||
            !IsFinite(step))
        {
            throw new ArgumentException(
                "Все параметры должны быть конечными числами.");
        }

        if (end <= start)
        {
            throw new ArgumentException(
                "Xкон должен быть больше Xнач.");
        }

        if (step <= 0.0)
        {
            throw new ArgumentException(
                "Шаг должен быть положительным.");
        }

        if (start + step <= start)
        {
            throw new ArgumentException(
                "Шаг слишком мал для выбранного Xнач.");
        }
    }

    private static double Calculate(
        Func<double, double> function,
        double x)
    {
        double value = function(x);

        if (!IsFinite(value))
        {
            throw new ArithmeticException(
                "Невозможно вычислить функцию при x = " +
                Format(x));
        }

        return value;
    }

    private static bool IsFinite(double value)
    {
        return !double.IsNaN(value)
            && !double.IsInfinity(value);
    }

    private static void PrintRow(
        double x,
        double y,
        string comment)
    {
        Console.WriteLine(
            "{0,14} {1,18}   {2}",
            Format(x),
            y.ToString("F6", CultureInfo.InvariantCulture),
            comment);
    }

    private static string Format(double value)
    {
        return value.ToString(
            "0.############",
            CultureInfo.InvariantCulture);
    }
}