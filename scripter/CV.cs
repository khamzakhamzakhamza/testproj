using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Tesseract;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace scripter
{
    class CV
    {
        #region properties 
        const string TEMPLATE_NEW_ITEM = @".\templates\template__new_item.png";
        const string TEMPLATE_CUSTOMIZE = @".\templates\template_customise.png";
        const string TEMPLATE_ACTIVE_RECTANGLE = @".\templates\template_active_rect.png";
        const string TEMPLATE_ACTIVE_ROW = @".\templates\template_active_row.png";
        const string TEMPLATE_ACTIVE_ITEM = @".\templates\template_active_item.png";
        const string TEMPLATE_ITEM = @".\templates\template_item.png";

        Bitmap img { get;  set; }
        Bitmap tmpNewItemImg { get; set; }
        Bitmap tmpCustImg { get; set; }
        Bitmap tmpARectImg { get; set; }
        Bitmap tmpARowImg { get; set; }
        Bitmap tmpAItemImg { get; set; }
        Bitmap tmpItemImg { get; set; }
        #endregion

        #region public 
        
        public CV(Bitmap img)
        {
            this.img = img;

            tmpNewItemImg = new Bitmap(TEMPLATE_NEW_ITEM);
            tmpCustImg = new Bitmap(TEMPLATE_CUSTOMIZE);
            tmpARectImg = new Bitmap(TEMPLATE_ACTIVE_RECTANGLE);
            tmpARowImg = new Bitmap(TEMPLATE_ACTIVE_ROW);
            tmpAItemImg = new Bitmap(TEMPLATE_ACTIVE_ITEM);
            tmpItemImg = new Bitmap(TEMPLATE_ITEM);
        }

        /// <summary>
        /// Определяет тип картинки
        /// </summary>
        /// <returns></returns>
        public Screens GetImgScreen()
        {
            int threshold = 50000; 
            if (CompareImgs(img, tmpNewItemImg, threshold).X > 0)
                return Screens.NewItem;
            else if (CompareImgs(img, tmpCustImg, threshold).X > 0)
                return Screens.Customise;
            return Screens.Misc;
        }

        /// <summary>
        /// Узнает выбраный пункт в меню Customise
        /// </summary>
        /// <returns></returns>
        public int GetActiveRectangle()
        {
            var slcRctPoint = CompareImgs(img, tmpARectImg, int.MaxValue);

            int _x = img.Width / 5;
            int _y = img.Height / 2;
            for (int i = 1; i < 9; i++)
            {
                if (slcRctPoint.X < _x * i)
                {
                    if (slcRctPoint.Y < _y)
                        return i;
                    else
                        return i + 4;
                }
            }
            return -1;
        }

        /// <summary>
        /// Узнает выбраный пункт в меню New Item
        /// </summary>
        /// <returns></returns>
        public int GetActiveRow()
        {
            var slcRctPoint = CompareImgs(img, tmpARowImg, int.MaxValue);

            int topPadding = 71;

            for (int i = 1; i < 3; i++)
            {
                topPadding += tmpARowImg.Height;

                if (slcRctPoint.Y < topPadding)
                    return i;
            }
            return 3;
        }

        /// <summary>
        /// Находит выбраную карточку 
        /// </summary>
        /// <returns></returns>
        public int GetActiveItem()
        {
            var aItmPoint = CompareImgs(img, tmpAItemImg, int.MaxValue);

            int leftPadding = 73; int itmWidth = 62;

            return ((aItmPoint.X - leftPadding) / itmWidth) + 1;
        }

        /// <summary>
        /// Узнает кол-во карточек 
        /// </summary>
        /// <returns></returns>
        public int GetItemsCount()
        {
            var itemsCountRct = new Rectangle(new Point(792, 156), new Size(80, 30));
            var crpImg = img.Clone(itemsCountRct, img.PixelFormat);
            crpImg = ToGrayScale(crpImg);
            var newImg = ResizeImage(crpImg, crpImg.Width + 40, crpImg.Height + 40);
            var txt = ReadText(newImg);

            return Convert.ToInt32(Regex.Replace(txt, @"[^\d]", ""));
        }

        /// <summary>
        /// Читает назввания представленых карточек 
        /// </summary>
        /// <param name="itmCount"></param>
        /// <returns></returns>
        public string GetItemsTitles(int itmCount)
        {
            var titles = "";
            var kiRect = new Rectangle(new Point(54, 141), new Size(852, 247));
            var kiImg = img.Clone(kiRect, img.PixelFormat);

            for (int i = 0; i < itmCount; i++)
            {
                var slcItmPoint = CompareImgs(kiImg, tmpItemImg, int.MaxValue);

                var itmRct = new Rectangle(slcItmPoint, tmpItemImg.Size);
                var itmImg = img.Clone(itmRct, kiImg.PixelFormat);
                itmImg = ToGrayScale(itmImg);

                var titleRct = new Rectangle(new Point(0, 61), new Size(61, 27));
                var titleImg = itmImg.Clone(titleRct, itmImg.PixelFormat);
                titleImg = ResizeImage(kiImg, titleImg.Width, titleImg.Height);
                titles += ReadText(titleImg).Replace(" ", "").Replace("\n", "").Replace("\r", "") + "; ";
                kiImg = HideItem(kiImg, itmRct);
            }

            return titles;
        }

        /// <summary>
        /// Определяет тип выбраной карточки
        /// </summary>
        /// <returns></returns>
        public string SelectedItemType()
        {
            var aItmPoint = CompareImgs(img, tmpAItemImg, int.MaxValue);
            var aItmImg = img.Clone(new Rectangle(aItmPoint, tmpAItemImg.Size), img.PixelFormat);
            aItmImg.Save("item_test.png");
            tmpAItemImg.Save("item_tmp.png");
            int threshold = 100000000;
            if (CompareImgs(aItmImg, tmpAItemImg, threshold).X > -1)
                return "игрок";
            return "не игрок";
        }
        #endregion

        #region private 
        /// <summary>
        /// Сравнивает два изображения и находит схожие места
        /// </summary>
        /// <param name="img1"></param>
        /// <param name="img2"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        private Point CompareImgs(Bitmap img1, Bitmap img2, double threshold)
        {
            Mat result = new Mat();
            CvInvoke.MatchTemplate(img1.ToImage<Bgr, byte>(), img2.ToImage<Bgr, byte>(), result, TemplateMatchingType.Sqdiff);

            double minVal = 0.0;
            double maxVal = 0.0;
            var minLoc = new Point();
            var maxLoc = new Point();
            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            if (minVal <= threshold)
                return minLoc;
            return new Point(-1, -1);
        }

        /// <summary>
        /// Переводит изображение в оттенки одного цвета
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private Bitmap ToGrayScale(Bitmap img)
        {
            var newImg = img;

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    var pixelColor = img.GetPixel(x, y);
                    var newColor = Color.FromArgb(0, pixelColor.G, 0);
                    newImg.SetPixel(x, y, newColor);
                }
            }
            return newImg;
        }

        /// <summary>
        /// Читает текст на изображении
        /// </summary>
        /// <returns></returns>
        private string ReadText(Bitmap img)
        {
            try
            {
                using (var engine = new TesseractEngine(@".\tessdata", "eng", EngineMode.TesseractOnly))
                {
                    using (var page = engine.Process(img))
                    {
                        return page.GetText();
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return "";
            }
        }

        /// <summary>
        /// Убирает елемент с изображения
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private Bitmap HideItem(Bitmap img, Rectangle item)
        {
            var newImg = img;

            for (int x = 0; x < item.Width; x++)
            {
                for (int y = 0; y < item.Height; y++)
                {
                    newImg.SetPixel(item.X + x, item.Y + y, Color.Black);
                }
            }
            return newImg;
        }

        /// <summary>
        /// Меняет размер изображения
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Bitmap ResizeImage(Bitmap img, int width, int height)
        {
            var rct = new Rectangle(0, 0, width, height);
            var newImg = new Bitmap(width, height);

            newImg.SetResolution(img.HorizontalResolution, img.VerticalResolution);

            using (var graphics = Graphics.FromImage(newImg))
            {
                using (var wrapMode = new ImageAttributes())
                {
                    graphics.DrawImage(img, rct, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return newImg;
        }
        #endregion

    }
}
