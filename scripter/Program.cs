using System;

namespace scripter
{
    enum Screens
    {
        NewItem,
        Customise,
        Misc
    }

    class Program
    {
        // Адреса по которым будут сохранены изображения
        const string NEW_ITEM_PATH = @".\scripter\new_item\";
        const string CUSTOMISE_PATH = @".\scripter\customise\";
        const string MISC_PATH = @".\scripter\misc\";

        static void Main(string[] args)
        {
            Console.WriteLine("Здравствуйте, эта программа разработана в рамках тестового задания Scripter, сейчас программа начнет обрабатывать полученные изображения\n");
            int imgId = 0;
            while (true)
            {
                var img = IOImage.GetImage();
                var cv = new CV(img);
                var screen = cv.GetImgScreen();

                switch (screen)
                {
                    case Screens.NewItem:
                        var niImgPath = NEW_ITEM_PATH + "img" + imgId + ".png";
                        int rowNum = cv.GetActiveRow();
                        var niText = MiscNICalc(cv, rowNum);

                        Console.WriteLine("На изображении " + niImgPath + " изображен пункт меню \"NEW ITEM\", " + niText + "\n");
                        IOImage.SaveImg(niImgPath, img);
                        break;
                    case Screens.Customise:
                        var cuImgPath = CUSTOMISE_PATH + "img" + imgId + ".png";
                        int rctNum = cv.GetActiveRectangle();
                        var rnd = new Random(); int rctTrg = rnd.Next(0, 9);
                        var path = GetPath(rctNum, rctTrg);

                        Console.WriteLine("На изображении " + cuImgPath + " изображено меню \"CUSTOMISE\" и выбран пунк меню под номером " + 
                            rctNum + ", чтобы попасть в пунк под номером " + rctTrg + " нужно пройти по маршруту" + path + "\n");
                        IOImage.SaveImg(cuImgPath, img);
                        break;
                    case Screens.Misc:
                        IOImage.SaveImg(MISC_PATH + "img" + imgId + ".png", img);
                        break;
                }

                imgId++;
            }
        }

        /// <summary>
        /// Строит путь из одного объекта меню в другой
        /// </summary>
        /// <param name="rctNum"></param>
        /// <param name="rctTrg"></param>
        /// <returns></returns>
        static private string GetPath(int rctNum, int rctTrg)
        {
            var path = "";

            int xRctTrg = rctTrg;
            if (rctTrg > 4) 
                xRctTrg -= 4;
            int xRctNum = rctNum;
            if (rctNum > 4)
                xRctNum -= 4;

            while (xRctNum != xRctTrg)
            {
                if (xRctTrg > xRctNum)
                {
                    xRctNum++; rctNum++;
                    path += " право";
                }
                else
                {
                    xRctNum--; rctNum--;
                    path += " лево";
                }
            }
            if (rctTrg > rctNum)
                path += " вниз";
            else if (rctTrg < rctNum)
                path += " вверх";

            return path;
        }

        /// <summary>
        /// Выполняет задания для меню NewItem
        /// </summary>
        /// <param name="img"></param>
        /// <param name="rowNum"></param>
        /// <returns></returns>
        static private string MiscNICalc(CV cv, int rowNum)
        {
            if (rowNum < 2)
                return "чтобы попасть в раздел \"KEEP ITEMS\" нажмите вниз" + "\n";
            else if (rowNum > 2)
                return "чтобы попасть в раздел \"KEEP ITEMS\" нажмите вверх" + "\n";
            else
            {
                int itmCount = cv.GetItemsCount();
                int activeItmId = cv.GetActiveItem();
                var cardsTitle = cv.GetItemsTitles(itmCount);

                return "в разделе находится " + itmCount + 
                    " карточек на которых представлены: " + cardsTitle +
                    "и выбрана карточка под номером " + activeItmId + 
                    " на которой изображен " + cv.SelectedItemType() + "\n";
            }

        }

    }
}
