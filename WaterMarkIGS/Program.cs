using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;
using IniParser;
using IniParser.Model;
using System.Text.RegularExpressions;
using System.Reflection.PortableExecutable;

int color_red;
int color_green;
int color_blue;
int color_aplpha;
int fontsie;
int angle;
int linespace;
int dx;
int dy;

var parser = new FileIniDataParser();
IniData data = parser.ReadFile("config.ini");

try
{
    color_red = int.Parse(data["color"]["red"]);
    color_green = int.Parse(data["color"]["green"]);
    color_blue = int.Parse(data["color"]["blue"]);
    color_aplpha = int.Parse(data["color"]["alpha"]);
    fontsie = int.Parse(data["text"]["font_size"]);
    angle = int.Parse(data["position"]["angle"]);
    linespace = (int.Parse(data["position"]["line_space"]) / 2);
    dx = int.Parse(data["position"]["dx"]);
    dy = int.Parse(data["position"]["dy"]);
}
catch (FormatException ex)
{
    Console.WriteLine("Ошибка! Что-то не так, с параметрами в файле config.ini");
    Console.WriteLine(ex.Message);
    return;
}

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

string[] pdffilespathes = Directory.GetFiles(".", "*.pdf").Where(x => !x.Contains("watermark")).ToArray();

if (pdffilespathes.Length.Equals(0))
{
    Console.WriteLine("Не найдено файлов с расширением pdf");
}

for (int i = 0; i < pdffilespathes.Length; i++)
{
    try
    {
        Console.Write($"Обрабатываем файл {pdffilespathes[i]}");
        string filename = Regex.Match(pdffilespathes[i], @"^\.\\(.*)\.pdf$", RegexOptions.IgnoreCase).Groups[1].Value;
        string filenamenew = $"{filename}_watermark.pdf";

        using (PdfReader pdf = new PdfReader(pdffilespathes[i]))
        {
            var rec = pdf.GetPageSize(1);
            int center_x = (int)(rec.Width / 2);
            int center_y = (int)(rec.Height / 2);

            using (FileStream stream = new FileStream(filenamenew, FileMode.Create, FileAccess.Write))
            {
                BaseColor wm_color = new BaseColor(color_red, color_green, color_blue, color_aplpha);
                using (PdfStamper stamper = new PdfStamper(pdf, stream))
                {
                    for (int k = 0; k < stamper.Reader.NumberOfPages; k++)
                    {
                        var pdfContentByte = stamper.GetOverContent(k + 1);
                        var font = BaseFont.CreateFont("arial.ttf", "Cp1251", true);
                        pdfContentByte.BeginText();
                        pdfContentByte.SetColorFill(wm_color);
                        pdfContentByte.SetFontAndSize(font, fontsie);
                        pdfContentByte.ShowTextAligned(PdfContentByte.ALIGN_CENTER, data["text"]["line1"], center_x + dx, center_y + dy + linespace, angle);
                        pdfContentByte.ShowTextAligned(PdfContentByte.ALIGN_CENTER, data["text"]["line2"], center_x + dx, center_y + dy - linespace, angle);
                        pdfContentByte.EndText();
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Ошибка при добавлении watermark");
        Console.WriteLine(ex.Message);
        return;
    }

    Console.WriteLine($" - ОК");
}

Console.WriteLine("Press any key...");
Console.ReadLine();