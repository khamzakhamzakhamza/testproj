using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace scripter
{
    static class IOImage
    {
        /// <summary>
        /// Сохраняет изображение
        /// </summary>
        /// <param name="path"></param>
        /// <param name="img"></param>
        /// <returns></returns>
        static public bool SaveImg(string path, Bitmap img)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                img.Save(path);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Получает изображение от сервера
        /// </summary>
        /// <returns></returns>
        static public Bitmap GetImage()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://185.80.129.249:4222/getImage");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                using (var reader = new BinaryReader(response.GetResponseStream()))
                {
                    using (var ms = new MemoryStream(reader.ReadBytes(1000000)))
                    {
                        return new Bitmap(ms);
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
    }
}
