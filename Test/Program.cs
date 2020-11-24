using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        public const string TARGET_FOLDER_PATH = @"\\scala1\TesztWeb\Image\Graphs\";
        public const double PRODUCTION_TARGET = 100;
        public const double WASTE_TARGET = 0.65;
        public static readonly Size FOUR_K = new Size(3840, 2160);
        public static double RENDER_WIDTH => (FOUR_K.Width / 3) * 0.85;
        public static double RENDER_HEIGHT => (FOUR_K.Height / 3 / 2) * 0.85;
        public static double LABEL_SIZE => RENDER_HEIGHT / 7;
        public const int IMAGE_DPI = 96;

        static void Main(string[] args)
        {
            var targetWidth = 519;
            var targetHeight = 156;
            Console.WriteLine("Target:\t" + "519" + "/" + "156");
            Console.WriteLine("Actual:\t" + RENDER_WIDTH + "/"+ RENDER_HEIGHT);
            Console.WriteLine("Coef.:\t" + (RENDER_WIDTH/targetWidth) + "/" + (RENDER_HEIGHT/targetHeight));
            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
